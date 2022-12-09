import Notification from "@/models/notification";

import { NotificationGetters, NotificationState } from "./types";

export const getters: NotificationGetters = {
    notifications(state: NotificationState): Notification[] {
        return state.notifications;
    },
};
