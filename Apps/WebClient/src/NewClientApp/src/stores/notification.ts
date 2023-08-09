import { defineStore } from "pinia";
import { computed, ref } from "vue";

import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { DateWrapper } from "@/models/dateWrapper";
import { ResultError } from "@/models/errors";
import Notification from "@/models/notification";
import { LoadStatus } from "@/models/storeOperations";
import { ILogger, INotificationService } from "@/services/interfaces";
import { useErrorStore } from "@/stores/error";
import { useUserStore } from "@/stores/user";

export const useNotificationStore = defineStore("notification", () => {
    const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
    const notificationService = container.get<INotificationService>(
        SERVICE_IDENTIFIER.NotificationService
    );

    const userStore = useUserStore();
    const errorStore = useErrorStore();

    const notifications = ref<Notification[]>([]);
    const statusMessage = ref("");
    const error = ref<ResultError>();
    const status = ref(LoadStatus.NONE);
    const isNotificationCenterOpen = ref(false);

    const newNotifications = computed(() => {
        if (userStore.lastLoginDateTime) {
            const lastLoginDateTime = new DateWrapper(
                userStore.lastLoginDateTime,
                {
                    isUtc: true,
                    hasTime: true,
                }
            );
            return notifications.value.filter((n) =>
                new DateWrapper(n.scheduledDateTimeUtc, {
                    isUtc: true,
                    hasTime: true,
                }).isAfter(lastLoginDateTime)
            );
        }
        return notifications.value;
    });

    function setRequested() {
        status.value = LoadStatus.REQUESTED;
    }

    function setNotifications(incomingNotifications: Notification[]) {
        notifications.value = incomingNotifications;
        error.value = undefined;
        status.value = LoadStatus.LOADED;
    }

    function clearNotification(notificationId: string) {
        notifications.value = notifications.value.filter(
            (n) => n.id != notificationId
        );
    }

    function clearAllNotifications() {
        notifications.value = [];
    }

    function setNotificationError(incomingError: ResultError) {
        error.value = incomingError;
        statusMessage.value = incomingError.resultMessage;
        status.value = LoadStatus.ERROR;
    }

    function retrieve(): Promise<Notification[]> {
        const notificationsValue = notifications.value;
        if (status.value === LoadStatus.LOADED) {
            logger.debug(`Notifications found stored, not querying!`);
            return Promise.resolve(notificationsValue);
        } else {
            logger.debug(`Retrieving Notifications`);
            setRequested();
            return notificationService
                .getNotifications(userStore.hdid)
                .then((result) => {
                    setNotifications(result);
                    return result;
                })
                .catch((error: ResultError) => {
                    setNotificationError(error);
                    throw error;
                });
        }
    }

    function dismissNotification(notificationId: string): Promise<void> {
        return notificationService
            .dismissNotification(userStore.hdid, notificationId)
            .then(() => {
                clearNotification(notificationId);
            })
            .catch((error: ResultError) => {
                handleError(error, ErrorType.Delete);
                throw error;
            });
    }

    function dismissAllNotifications(): Promise<void> {
        return notificationService
            .dismissNotifications(userStore.hdid)
            .then(() => {
                clearAllNotifications();
            })
            .catch((error: ResultError) => {
                handleError(error, ErrorType.Delete);
                throw error;
            });
    }

    function handleError(errorRaised: ResultError, errorType: ErrorType) {
        setNotificationError(errorRaised);
        if (errorRaised.statusCode === 429) {
            const errorKey = "page";
            if (errorType === ErrorType.Retrieve) {
                errorStore.setTooManyRequestsWarning(errorKey);
            } else {
                errorStore.setTooManyRequestsError(errorKey);
            }
        } else {
            errorStore.addError(
                errorType,
                ErrorSourceType.Notification,
                errorRaised.traceId
            );
        }
    }

    return {
        notifications,
        statusMessage,
        error,
        status,
        isNotificationCenterOpen,
        newNotifications,
        dismissNotification,
        dismissAllNotifications,
        retrieve,
    };
});
