import { ResultError } from "@/models/errors";
import Notification from "@/models/notification";
import { LoadStatus } from "@/models/storeOperations";

import { NotificationMutations, NotificationState } from "./types";

export const mutations: NotificationMutations = {
    setRequested(state: NotificationState) {
        state.status = LoadStatus.REQUESTED;
    },
    setNotifications(state: NotificationState, notifications: Notification[]) {
        state.notifications = notifications;
        state.error = undefined;
        state.status = LoadStatus.LOADED;
    },
    dismissNotification(state: NotificationState, notificationId: string) {
        state.notifications = state.notifications.filter(
            (n) => n.id != notificationId
        );
    },
    dismissAllNotifications(state: NotificationState) {
        state.notifications = [];
    },
    notificationError(state: NotificationState, error: ResultError) {
        state.error = error;
        state.statusMessage = error.resultMessage;
        state.status = LoadStatus.ERROR;
    },
};
