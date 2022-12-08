import { DateWrapper } from "@/models/dateWrapper";
import Notification from "@/models/notification";
import User from "@/models/user";
import { RootState } from "@/store/types";

import { NotificationGetters, NotificationState } from "./types";

export const getters: NotificationGetters = {
    notifications(state: NotificationState): Notification[] {
        return state.notifications;
    },
    newNotifications(
        state: NotificationState,
        // eslint-disable-next-line
        _getters: any,
        _rootState: RootState,
        // eslint-disable-next-line
        rootGetters: any
    ): Notification[] {
        const user: User = rootGetters["user/user"];
        if (user.lastLoginDateTime) {
            const lastLoginDateTime = new DateWrapper(user.lastLoginDateTime);
            return state.notifications.filter((n) =>
                new DateWrapper(n.scheduledDateTimeUtc).isAfter(
                    lastLoginDateTime
                )
            );
        }
        return state.notifications;
    },
};
