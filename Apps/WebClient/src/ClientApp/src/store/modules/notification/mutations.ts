import { ResultError } from "@/models/errors";
import Notification from "@/models/notification";
import { LoadStatus } from "@/models/storeOperations";

import { NotificationMutations, NotificationState } from "./types";

export const mutations: NotificationMutations = {
    deleteNotification(state: NotificationState, notification: Notification) {
        const notificationIndex = state.notifications.findIndex(
            (x) => x.id === notification.id
        );
        if (notificationIndex > -1) {
            state.notifications.splice(notificationIndex, 1);
        }
    },
    deleteNotifications(state: NotificationState) {
        state.notifications;
    },
    notificationError(state: NotificationState, error: ResultError) {
        state.error = error;
        state.statusMessage = error.resultMessage;
        state.status = LoadStatus.ERROR;
    },
};
