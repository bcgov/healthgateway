import { useState } from "react";
import { View } from "react-native";

import {
    AppText,
    HgAlert,
    HgButton,
    HgSwitch,
    InformationDialog,
    SectionHeading,
} from "@/components/common";
import {
    getNotificationPreferenceTypes,
    getUserProfileNotificationSettings,
    getUserProfileNotificationType,
    type NotificationPreferenceType,
    ProfileNotificationPreference,
    ProfileNotificationType,
} from "@/constants/profileNotifications";
import { Action, Text, Type } from "@/constants/tracking";
import { useBreakpoint } from "@/hooks/useBreakpoint";
import { isSessionEnded, ResultError } from "@/models/errors";
import type { InformationModalContent } from "@/models/informationModal";
import { services } from "@/services/container";
import { describeError, logger } from "@/services/logger";
import { selectFeatureToggles, useConfigStore } from "@/stores/config.store";
import { useErrorStore } from "@/stores/error.store";
import { useUserStore } from "@/stores/user.store";
import { ConfigUtil } from "@/utils/configUtil";

type Channel = "email" | "sms";
interface ChannelState {
    emailEnabled: boolean;
    smsEnabled: boolean;
}

// .notification-type-col / .notification-toggle-col and the v-col paddings (pe-sm-8, pe-4, ps-sm-4)
const TYPE_COL = { minWidth: 245, maxWidth: 295, paddingRight: 32 };
const TOGGLE_COL = { minWidth: 68, paddingRight: 16 };

const emailEnabledFor = (type: ProfileNotificationType) =>
    ConfigUtil.isProfileNotificationPreferenceEnabled(
        type,
        ProfileNotificationPreference.Email
    );
const smsEnabledFor = (type: ProfileNotificationType) =>
    ConfigUtil.isProfileNotificationPreferenceEnabled(
        type,
        ProfileNotificationPreference.Sms
    );

/** Port of UserProfileNotificationsComponent.vue (rows and copy from constants/profileNotifications.ts). */
export function NotificationsSection() {
    // re-render when the toggles change; ConfigUtil reads the store directly
    useConfigStore(selectFeatureToggles);
    const { xs } = useBreakpoint();
    const user = useUserStore((s) => s.user);
    const update = useUserStore((s) => s.updateNotificationSettings);
    const setTooManyRequests = useErrorStore((s) => s.setTooManyRequestsError);
    const [saving, setSaving] = useState<Record<string, boolean>>({});
    const [saveError, setSaveError] = useState<string | null>(null);
    // optimistic switch values while a save is in flight (web client: channelState, rolled back on failure)
    const [pending, setPending] = useState<Record<string, ChannelState>>({});
    const [modal, setModal] = useState<InformationModalContent | null>(null);

    if (!ConfigUtil.isProfileNotificationsFeatureEnabled()) return null;

    const rows = getNotificationPreferenceTypes().filter((d) =>
        ConfigUtil.isProfileNotificationTypeEnabled(d.type)
    );
    const showEmail = rows.some((r) => emailEnabledFor(r.type));
    const showSms = rows.some((r) => smsEnabledFor(r.type));
    const emailChannelDisabled = !user.email || !user.verifiedEmail;
    const smsChannelDisabled = !user.sms || !user.verifiedSms;
    // single message per current business decision (web client NOTE)
    const verificationMessage =
        (showEmail && emailChannelDisabled) || (showSms && smsChannelDisabled)
            ? "You must verify your contact information to receive notifications."
            : null;

    const saved = new Map(
        getUserProfileNotificationSettings(user.notificationSettings).map(
            (s) => [s.type, s.preferences]
        )
    );

    /** The switch values: saved preferences, forced off when the toggle or the contact channel disallows them. */
    function channelState(row: NotificationPreferenceType): ChannelState {
        const prefs = pending[row.id] ?? {
            emailEnabled: (saved.get(row.type) ?? []).includes(
                ProfileNotificationPreference.Email
            ),
            smsEnabled: (saved.get(row.type) ?? []).includes(
                ProfileNotificationPreference.Sms
            ),
        };
        return {
            emailEnabled:
                prefs.emailEnabled &&
                emailEnabledFor(row.type) &&
                !emailChannelDisabled,
            smsEnabled:
                prefs.smsEnabled &&
                smsEnabledFor(row.type) &&
                !smsChannelDisabled,
        };
    }

    async function handleToggle(
        row: NotificationPreferenceType,
        channel: Channel,
        value: boolean
    ) {
        if (saving[row.id]) return;
        setSaveError(null);
        const next = { ...channelState(row) };
        if (channel === "email") next.emailEnabled = value;
        else next.smsEnabled = value;
        setPending((p) => ({ ...p, [row.id]: next }));

        services.tracking.trackEvent({
            action: Action.ButtonClick,
            text:
                channel === "email"
                    ? value
                        ? Text.ScreeningEmailToggledOn
                        : Text.ScreeningEmailToggledOff
                    : value
                      ? Text.ScreeningSmsToggledOn
                      : Text.ScreeningSmsToggledOff,
            type: Type.Notifications,
        });

        setSaving((s) => ({ ...s, [row.id]: true }));
        try {
            // a channel the toggle does not offer is sent as null; one the user cannot use is sent as off
            await update({
                type: getUserProfileNotificationType(row.type),
                emailEnabled: emailEnabledFor(row.type)
                    ? next.emailEnabled
                    : null,
                smsEnabled: smsEnabledFor(row.type) ? next.smsEnabled : null,
            });
        } catch (error) {
            if (isSessionEnded(error)) return;
            logger.error(describeError(error));
            if (error instanceof ResultError && error.statusCode === 429) {
                setSaveError(null);
                setTooManyRequests();
            } else {
                setSaveError(
                    "An unexpected error has occurred. Please try again later."
                );
            }
        } finally {
            // drop the optimistic values: on success the reloaded profile shows them, on failure this rolls back
            setPending((p) => {
                const rest = { ...p };
                delete rest[row.id];
                return rest;
            });
            setSaving((s) => ({ ...s, [row.id]: false }));
        }
    }

    function showInformation(row: NotificationPreferenceType) {
        services.tracking.trackEvent({
            action: Action.ButtonClick,
            text: Text.ScreeningNotificationsLearnMore,
            type: Type.Notifications,
        });
        setModal(row.modal.content);
    }

    const toggle = (
        row: NotificationPreferenceType,
        channel: Channel,
        state: ChannelState
    ) => {
        const isEmail = channel === "email";
        const disabled =
            (isEmail
                ? emailChannelDisabled || !emailEnabledFor(row.type)
                : smsChannelDisabled || !smsEnabledFor(row.type)) ||
            Boolean(saving[row.id]);
        return (
            <View style={TOGGLE_COL} className={xs ? "mb-2" : ""}>
                {/* phones: each switch carries its own label (the header row is hidden) */}
                {xs ? (
                    <AppText className="mb-1">
                        {isEmail ? "Email" : "Text (SMS)"}
                    </AppText>
                ) : null}
                <HgSwitch
                    testID={`profile-notification-preferences-${row.id}-${channel}-value`}
                    accessibilityLabel={`${row.label} ${isEmail ? "email" : "text (SMS)"} notifications`}
                    value={isEmail ? state.emailEnabled : state.smsEnabled}
                    disabled={disabled}
                    onValueChange={(v) => void handleToggle(row, channel, v)}
                />
            </View>
        );
    };

    return (
        <View className="mb-4">
            <SectionHeading
                title="Notifications"
                includeDivider
                level={2}
                testID="profile-notification-preferences-label"
            />
            {verificationMessage ? (
                <AppText
                    className="mt-2 tracking-normal"
                    testID="profile-notification-preferences-verification-message"
                >
                    {verificationMessage}
                </AppText>
            ) : null}
            {saveError ? (
                <HgAlert
                    type="error"
                    live="assertive"
                    variant="text"
                    text={saveError}
                    testID="profile-notification-preferences-save-error"
                />
            ) : null}
            {/* v-sheet max-width 960: keeps Email / SMS from drifting on very wide screens */}
            <View style={{ maxWidth: 960 }} className="mt-4">
                {rows.length > 0 ? (
                    <>
                        {!xs ? (
                            <View
                                className="flex-row mb-2"
                                testID="profile-notification-preferences-header"
                            >
                                <View
                                    style={[
                                        TYPE_COL,
                                        {
                                            flexGrow: 1,
                                            flexShrink: 1,
                                            flexBasis: 0,
                                        },
                                    ]}
                                    testID="profile-notification-preferences-header-type"
                                >
                                    <AppText className="font-bold">
                                        Notification Type
                                    </AppText>
                                </View>
                                {showEmail ? (
                                    <View
                                        style={TOGGLE_COL}
                                        testID="profile-notification-preferences-header-email"
                                    >
                                        <AppText className="font-bold">
                                            Email
                                        </AppText>
                                    </View>
                                ) : null}
                                {showSms ? (
                                    <View
                                        style={TOGGLE_COL}
                                        testID="profile-notification-preferences-header-sms"
                                    >
                                        <AppText className="font-bold">
                                            {"Text\n(SMS)"}
                                        </AppText>
                                    </View>
                                ) : null}
                                <View className="flex-1 pl-4" />
                            </View>
                        ) : null}
                        {rows.map((row) => {
                            const state = channelState(row);
                            return (
                                <View
                                    key={row.id}
                                    testID={`profile-notification-preferences-row-${row.id}`}
                                    className={
                                        xs
                                            ? "flex-row flex-wrap items-center mb-4"
                                            : "flex-row items-center"
                                    }
                                >
                                    <View
                                        style={
                                            xs
                                                ? { width: "100%" }
                                                : [
                                                      TYPE_COL,
                                                      {
                                                          flexGrow: 1,
                                                          flexShrink: 1,
                                                          flexBasis: 0,
                                                      },
                                                  ]
                                        }
                                        className={xs ? "mb-2" : ""}
                                    >
                                        <AppText
                                            className="tracking-normal"
                                            testID={`profile-notification-preferences-${row.id}-label-value`}
                                        >
                                            {row.label}
                                        </AppText>
                                    </View>
                                    {showEmail
                                        ? toggle(row, "email", state)
                                        : null}
                                    {showSms ? toggle(row, "sms", state) : null}
                                    <View
                                        className={
                                            xs
                                                ? "w-full pt-2 flex-row"
                                                : "flex-1 pl-4 flex-row items-center"
                                        }
                                    >
                                        {row.type ===
                                        ProfileNotificationType.BcCancerScreening ? (
                                            <HgButton
                                                testID={`profile-notification-preferences-${row.id}-learn-more`}
                                                text="LEARN MORE"
                                                variant="secondary"
                                                onPress={() =>
                                                    showInformation(row)
                                                }
                                            />
                                        ) : null}
                                    </View>
                                </View>
                            );
                        })}
                    </>
                ) : (
                    <AppText testID="profile-notification-preferences-empty">
                        No notification options are currently available.
                    </AppText>
                )}
            </View>
            {modal ? (
                <InformationDialog
                    visible
                    content={modal}
                    onClose={() => setModal(null)}
                />
            ) : null}
        </View>
    );
}
