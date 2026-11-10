import { useRouter } from "expo-router";
import { useState } from "react";
import {
    ActivityIndicator,
    Pressable,
    ScrollView,
    useWindowDimensions,
    View,
} from "react-native";

import EmptyStateNotifications from "@/assets/images/home/empty-state-notifications.svg";
import { AppText, HgButton, Icon, ModalSafeArea } from "@/components/common";
import { AppModal } from "@/components/common/AppModal";
import { Backdrop } from "@/components/common/Backdrop";
import { EntryType } from "@/constants/entryType";
import { Path } from "@/constants/path";
import { useThemeColors } from "@/hooks/useTheme";
import {
    type Notification,
    NotificationActionType,
} from "@/models/notification";
import { TimelineFilterBuilder } from "@/models/timeline/timelineFilter";
import { toHref } from "@/navigation/href";
import { describeError, logger } from "@/services/logger";
import {
    useNewNotifications,
    useNotificationStore,
} from "@/stores/notification.store";
import { useTimelineStore } from "@/stores/timeline.store";
import { formatDate, fromIso } from "@/utils/dateUtil";
import { imageProps } from "@/utils/decorative";
import { openExternal } from "@/utils/openExternal";

/** AlertCategory → the timeline's record type (getEntryType) */
const categoryEntryTypes: Record<string, EntryType> = {
    ClinicalDocument: EntryType.ClinicalDocument,
    COVID19Laboratory: EntryType.Covid19TestResult,
    HealthVisit: EntryType.HealthVisit,
    Immunization: EntryType.Immunization,
    Laboratory: EntryType.LabResult,
    Medications: EntryType.Medication,
    MyNote: EntryType.Note,
    SpecialAuthority: EntryType.SpecialAuthorityRequest,
    CancerScreening: EntryType.BcCancerScreening,
};
const BCT_ODR_CATEGORY = "BctOdr";

function actionText(actionType: NotificationActionType) {
    if (actionType === NotificationActionType.InternalLink) return "View";
    if (actionType === NotificationActionType.ExternalLink) return "More Info";
    return undefined;
}

/** Port of NotificationCentreComponent.vue: a temporary 500-wide drawer on the right. */
export function NotificationCentre() {
    const router = useRouter();
    const colors = useThemeColors();
    const { width } = useWindowDimensions();
    const isOpen = useNotificationStore((s) => s.isNotificationCenterOpen);
    const setOpen = useNotificationStore((s) => s.setNotificationCenterOpen);
    const notifications = useNotificationStore((s) => s.notifications);
    const dismissNotification = useNotificationStore(
        (s) => s.dismissNotification
    );
    const dismissAllNotifications = useNotificationStore(
        (s) => s.dismissAllNotifications
    );
    const setFilter = useTimelineStore((s) => s.setFilter);
    const newNotifications = useNewNotifications();
    const [loaders, setLoaders] = useState<string[]>([]);

    const isMessageLoading = (id?: string) =>
        loaders.includes("all") || (id !== undefined && loaders.includes(id));
    const isNew = (n: Notification) =>
        newNotifications.some((x) => x.id === n.id);

    function dismissAll() {
        setLoaders(["all"]);
        dismissAllNotifications()
            .catch((error: unknown) => logger.error(describeError(error)))
            .finally(() => setLoaders([]));
    }

    function dismiss(id: string) {
        setLoaders((l) => [...l, id]);
        dismissNotification(id)
            .catch((error: unknown) => logger.error(describeError(error)))
            .finally(() => setLoaders((l) => l.filter((x) => x !== id)));
    }

    function handleAction(notification: Notification) {
        const entryType = categoryEntryTypes[notification.categoryName];
        if (entryType) {
            setFilter(TimelineFilterBuilder.create().withEntryType(entryType));
            router.push(Path.Timeline);
        } else if (notification.categoryName === BCT_ODR_CATEGORY) {
            router.push(Path.Services);
        } else if (
            notification.actionType === NotificationActionType.InternalLink
        ) {
            const url = new URL(notification.actionUrl);
            router.push(toHref(url.href.replace(url.origin, "")));
        } else {
            void openExternal(notification.actionUrl);
        }
        setOpen(false);
    }

    return (
        <AppModal
            supportedOrientations={["portrait", "landscape"]}
            transparent
            visible={isOpen}
            animationType="fade"
            onRequestClose={() => setOpen(false)}
        >
            <Backdrop
                className="flex-1 bg-black/40 items-end"
                onPress={() => setOpen(false)}
            >
                {/* the panel swallows the press (responder on native, click on web): not a press on the scrim */}
                <View
                    className="h-full bg-surface"
                    style={{ width: Math.min(500, width) }}
                    onStartShouldSetResponder={() => true}
                    {...({
                        onClick: (e: Event) => e.stopPropagation(),
                    } as object)}
                >
                    <ModalSafeArea sides="right">
                        <View className="flex-1 p-4">
                            <View className="flex-row items-center">
                                <Pressable
                                    testID="notification-centre-close-button"
                                    accessibilityRole="button"
                                    accessibilityLabel="Close"
                                    hitSlop={8}
                                    onPress={() => setOpen(false)}
                                    className="p-2 mr-1"
                                >
                                    <Icon
                                        name="angles-right"
                                        size={16}
                                        color={colors["bc-grey"]}
                                    />
                                </Pressable>
                                <AppText
                                    variant="title-large"
                                    className="flex-1 text-primary"
                                >
                                    Notification Centre
                                </AppText>
                                {notifications.length > 0 ? (
                                    <HgButton
                                        testID="notification-centre-dismiss-all-button"
                                        variant="link"
                                        text="Clear All"
                                        loading={isMessageLoading()}
                                        onPress={dismissAll}
                                    />
                                ) : null}
                            </View>
                            {notifications.length === 0 ? (
                                <View className="flex-1 items-center justify-center">
                                    <EmptyStateNotifications
                                        width="66%"
                                        height={200}
                                        {...imageProps("No Notifications")}
                                    />
                                    <AppText
                                        variant="title-large"
                                        className="text-center text-primary mt-4"
                                    >
                                        No Notifications
                                    </AppText>
                                </View>
                            ) : (
                                <ScrollView
                                    testID="notifications-div"
                                    className="flex-1 mt-3"
                                >
                                    {notifications.map((notification) => {
                                        const muted = !isNew(notification);
                                        const action = actionText(
                                            notification.actionType
                                        );
                                        return (
                                            <View
                                                key={notification.id}
                                                className="mb-4"
                                            >
                                                <AppText
                                                    variant="body-medium"
                                                    className={`mb-1 ${muted ? "text-bc-grey" : ""}`}
                                                >
                                                    {formatDate(
                                                        fromIso(
                                                            notification.scheduledDateTimeUtc
                                                        ),
                                                        "MMMM d, yyyy"
                                                    )}
                                                </AppText>
                                                <View className="bg-surface-hover rounded shadow p-4">
                                                    <View className="flex-row">
                                                        <AppText
                                                            className={`flex-1 ${muted ? "text-bc-grey" : ""}`}
                                                        >
                                                            {
                                                                notification.displayText
                                                            }
                                                        </AppText>
                                                        <Pressable
                                                            testID={`notification-${notification.id}-dismiss-button`}
                                                            accessibilityRole="button"
                                                            accessibilityLabel="Dismiss"
                                                            hitSlop={8}
                                                            disabled={isMessageLoading(
                                                                notification.id
                                                            )}
                                                            onPress={() =>
                                                                dismiss(
                                                                    notification.id
                                                                )
                                                            }
                                                            className="p-1 ml-2"
                                                        >
                                                            {isMessageLoading(
                                                                notification.id
                                                            ) ? (
                                                                <ActivityIndicator
                                                                    color={
                                                                        colors.primary
                                                                    }
                                                                />
                                                            ) : (
                                                                <Icon
                                                                    name="xmark"
                                                                    size={16}
                                                                    color={
                                                                        colors[
                                                                            "bc-grey"
                                                                        ]
                                                                    }
                                                                />
                                                            )}
                                                        </Pressable>
                                                    </View>
                                                    {action ? (
                                                        <View className="items-end mt-2">
                                                            <HgButton
                                                                testID={`notification-${notification.id}-action-button`}
                                                                className="self-end"
                                                                variant="link"
                                                                text={action}
                                                                onPress={() =>
                                                                    handleAction(
                                                                        notification
                                                                    )
                                                                }
                                                            />
                                                        </View>
                                                    ) : null}
                                                </View>
                                            </View>
                                        );
                                    })}
                                </ScrollView>
                            )}
                        </View>
                    </ModalSafeArea>
                </View>
            </Backdrop>
        </AppModal>
    );
}
