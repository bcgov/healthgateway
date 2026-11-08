import { create } from "zustand";

import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { LoadStatus } from "@/models/datasetState";
import { ResultError, rethrowIfSessionEnded } from "@/models/errors";
import type { Notification } from "@/models/notification";
import { services } from "@/services/container";
import { logger } from "@/services/logger";
import { fromIso } from "@/utils/dateUtil";

import { useErrorStore } from "./error.store";
import { selectLastLoginDateTime, useUserStore } from "./user.store";

/**
 * The web client's AB#16927: notifications are switched off in code while Classic is aligned with the Salesforce
 * version (`isNotificationCentreAvailable && false` in HeaderComponent.vue). Keep this false for parity until the
 * web client turns them back on.
 */
export const NOTIFICATION_CENTRE_SWITCHED_ON = false as boolean;

interface NotificationState {
    notifications: Notification[];
    status: LoadStatus;
    statusMessage: string;
    error?: ResultError;
    isNotificationCenterOpen: boolean;
    setNotificationCenterOpen: (open: boolean) => void;
    retrieve: () => Promise<Notification[]>;
    dismissNotification: (notificationId: string) => Promise<void>;
    dismissAllNotifications: () => Promise<void>;
    reset: () => void;
}

/** Port of stores/notification.ts */
export const useNotificationStore = create<NotificationState>((set, get) => {
    const hdid = () => useUserStore.getState().user.hdid;

    function setError(error: unknown) {
        const resultError = error instanceof ResultError ? error : undefined;
        set({
            error: resultError,
            statusMessage: resultError?.message ?? String(error),
            status: LoadStatus.ERROR,
        });
        return resultError;
    }

    function handleError(error: unknown) {
        const resultError = setError(error);
        const errors = useErrorStore.getState();
        if (resultError?.statusCode === 429)
            errors.setTooManyRequestsError("page");
        else
            errors.addError(
                ErrorType.Delete,
                ErrorSourceType.Notification,
                resultError?.traceId
            );
    }

    return {
        notifications: [],
        status: LoadStatus.NONE,
        statusMessage: "",
        error: undefined,
        isNotificationCenterOpen: false,
        setNotificationCenterOpen: (open) =>
            set({ isNotificationCenterOpen: open }),
        retrieve: async () => {
            if (get().status === LoadStatus.LOADED) {
                logger.debug("Notifications found stored, not querying!");
                return get().notifications;
            }
            logger.debug("Retrieving Notifications");
            set({ status: LoadStatus.REQUESTED });
            try {
                const result =
                    await services.notification.getNotifications(hdid());
                set({
                    notifications: result,
                    error: undefined,
                    status: LoadStatus.LOADED,
                });
                return result;
            } catch (error) {
                rethrowIfSessionEnded(error);
                setError(error);
                throw error;
            }
        },
        dismissNotification: async (notificationId) => {
            try {
                await services.notification.dismissNotification(
                    hdid(),
                    notificationId
                );
                set((s) => ({
                    notifications: s.notifications.filter(
                        (n) => n.id !== notificationId
                    ),
                }));
            } catch (error) {
                rethrowIfSessionEnded(error);
                handleError(error);
                throw error;
            }
        },
        dismissAllNotifications: async () => {
            try {
                await services.notification.dismissNotifications(hdid());
                set({ notifications: [] });
            } catch (error) {
                rethrowIfSessionEnded(error);
                handleError(error);
                throw error;
            }
        },
        reset: () =>
            set({
                notifications: [],
                status: LoadStatus.NONE,
                statusMessage: "",
                error: undefined,
                isNotificationCenterOpen: false,
            }),
    };
});

/** newNotifications: scheduled after the previous login, or all of them without one */
export function newNotificationsOf(
    notifications: Notification[],
    lastLoginDateTime: string | undefined
): Notification[] {
    if (!lastLoginDateTime) return notifications;
    const lastLogin = fromIso(lastLoginDateTime);
    return notifications.filter(
        (n) => fromIso(n.scheduledDateTimeUtc) > lastLogin
    );
}

export const useNewNotifications = () => {
    const notifications = useNotificationStore((s) => s.notifications);
    const lastLogin = useUserStore(selectLastLoginDateTime);
    return newNotificationsOf(notifications, lastLogin);
};
