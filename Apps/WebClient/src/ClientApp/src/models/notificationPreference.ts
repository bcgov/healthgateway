import type { NotificationPreferenceType } from "./notificationPreferenceType";

export type NotificationPreference = NotificationPreferenceType & {
    enabled: boolean;
};
