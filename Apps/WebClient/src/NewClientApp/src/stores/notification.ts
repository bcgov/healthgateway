import { defineStore } from "pinia";
import { ref } from "vue";
import { ResultError } from "@/models/errors";
import { LoadStatus } from "@/models/storeOperations";
import Notification from "@/models/notification";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { ILogger, INotificationService } from "@/services/interfaces";
import { container } from "@/ioc/container";
import { useUserStore } from "@/stores/user";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { useErrorStore } from "@/stores/error";

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
        const user = userStore.user;
        const notificationsValue = notifications.value;
        if (status.value === LoadStatus.LOADED) {
            logger.debug(`Notifications found stored, not querying!`);
            return Promise.resolve(notificationsValue);
        } else {
            logger.debug(`Retrieving Notifications`);
            setRequested();
            return notificationService
                .getNotifications(user.hdid)
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
        const user = userStore.user;
        return notificationService
            .dismissNotification(user.hdid, notificationId)
            .then(() => {
                clearNotification(notificationId);
            })
            .catch((error: ResultError) => {
                handleError(error, ErrorType.Delete);
                throw error;
            });
    }

    function dismissAllNotifications(): Promise<void> {
        const user = userStore.user;
        return notificationService
            .dismissNotifications(user.hdid)
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
        dismissNotification,
        dismissAllNotifications,
        retrieve,
    };
});
