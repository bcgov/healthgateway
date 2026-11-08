import { DateTime } from "luxon";
import { useEffect, useRef, useState } from "react";
import { View } from "react-native";

import { AppText, HgAlert, InternalLink } from "@/components/ui";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { Path } from "@/constants/path";
import {
    Action,
    Destination,
    InternalUrl,
    Origin,
    Text,
    Type,
} from "@/constants/tracking";
import { UserPreferenceType } from "@/constants/userPreferenceType";
import { isSessionEnded, ResultError } from "@/models/errors";
import { services } from "@/services/container";
import { describeError, logger } from "@/services/logger";
import { useErrorStore } from "@/stores/error.store";
import { selectLastLoginDateTime, useUserStore } from "@/stores/user.store";

/** Date before which a last login shows the BC Cancer notifications banner (HomeView.vue). */
const BC_CANCER_NOTIFICATIONS_BANNER_CUTOFF =
    DateTime.fromISO("2026-05-19T09:00");

/**
 * Port of the banners at the top of HomeView.vue:
 *  1. BC Cancer Screening notifications announcement (users whose previous login predates the cutoff)
 *  2. "Verify Contact Information" (unverified email / SMS, or the showSmsRemoved preference set by the
 *     backend after a long absence; that preference is reset to "false" once shown)
 */
export function HomeBanners() {
    const user = useUserStore((s) => s.user);
    const lastLogin = useUserStore(selectLastLoginDateTime);
    const setUserPreference = useUserStore((s) => s.setUserPreference);
    const errorStore = useErrorStore();

    const unverifiedEmail = !user.verifiedEmail && user.hasEmail;
    const unverifiedSms = !user.verifiedSms && user.hasSms;
    const preferenceShowSmsRemoved =
        user.preferences[UserPreferenceType.ShowSmsRemoved]?.value === "true";
    const showBcCancerNotificationsAlert =
        lastLogin !== undefined &&
        DateTime.fromISO(lastLogin) < BC_CANCER_NOTIFICATIONS_BANNER_CUTOFF;

    const [showSmsRemoved, setShowSmsRemoved] = useState(
        preferenceShowSmsRemoved
    );
    const [contactBannerClosed, setContactBannerClosed] = useState(false);
    const resetSent = useRef(false);

    // Once the "SMS removed" message has been shown, flip the preference back to "false" like the web client.
    useEffect(() => {
        if (!preferenceShowSmsRemoved || resetSent.current) return;
        resetSent.current = true;
        setShowSmsRemoved(true);
        const existing = user.preferences[UserPreferenceType.ShowSmsRemoved];
        const preference = existing
            ? { ...existing, value: "false" }
            : {
                  preference: UserPreferenceType.ShowSmsRemoved,
                  value: "false",
                  version: 0,
                  createdDateTime: DateTime.now().toISO() ?? "",
              };
        setUserPreference(preference).catch((error) => {
            if (isSessionEnded(error)) return;
            logger.error(describeError(error));
            if (error instanceof ResultError && error.statusCode === 429)
                errorStore.setTooManyRequestsError();
            else errorStore.addError(ErrorType.Update, ErrorSourceType.Profile);
        });
    }, [
        preferenceShowSmsRemoved,
        user.preferences,
        setUserPreference,
        errorStore,
    ]);

    const trackProfileLink = () =>
        services.tracking.trackEvent({
            action: Action.InternalLink,
            text: Text.VerifyContactInformation,
            origin: Origin.Home,
            destination: Destination.Profile,
            type: Type.InfoBanner,
            url: InternalUrl.Profile,
        });

    return (
        <View>
            {showBcCancerNotificationsAlert ? (
                <HgAlert type="info" testID="bc-cancer-notifications-banner">
                    <AppText>
                        Email and text notifications are now being sent for BC
                        Cancer Screening program letters. Review your
                        preferences in your{" "}
                        <InternalLink
                            href={Path.Profile}
                            testID="profile-notifications-link"
                            nativeID="profileNotificationsLink"
                        >
                            profile
                        </InternalLink>
                        .
                    </AppText>
                </HgAlert>
            ) : null}
            {(unverifiedEmail || unverifiedSms || showSmsRemoved) &&
            !contactBannerClosed ? (
                <HgAlert
                    type="info"
                    testID="incomplete-profile-banner"
                    title="Verify Contact Information"
                    closable
                    onClose={() => setContactBannerClosed(true)}
                >
                    {showSmsRemoved ? (
                        <AppText testID="sms-removed-message">
                            You have not logged in for a while. Please verify
                            your contact information in your{" "}
                            <InternalLink
                                href={Path.Profile}
                                testID="profile-preferences-link"
                                nativeID="profilePreferencesLink"
                                onPress={trackProfileLink}
                            >
                                profile
                            </InternalLink>
                            .
                        </AppText>
                    ) : (
                        <AppText testID="unverified-email-sms-message">
                            Your email or cell phone number has not been
                            verified. Visit your{" "}
                            <InternalLink
                                href={Path.Profile}
                                testID="profile-page-link"
                                nativeID="profilePageLink"
                                onPress={trackProfileLink}
                            >
                                profile
                            </InternalLink>{" "}
                            to verify your contact information.
                        </AppText>
                    )}
                </HgAlert>
            ) : null}
        </View>
    );
}
