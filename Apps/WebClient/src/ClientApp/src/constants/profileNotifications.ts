import { NotificationPreferenceType } from "@/models/notificationPreferenceType";

export enum ProfileNotificationType {
    BcCancerScreening = "bcCancerScreening",
}

export enum ProfileNotificationPreference {
    Email = "email",
    Sms = "sms",
}

export const NOTIFICATION_PREFERENCE_TYPES = [
    {
        id: "bc-cancer-screening",
        type: ProfileNotificationType.BcCancerScreening,
        label: "Eligible BC Cancer Screening Letters",
        tooltip: "Learn more about BC Cancer notifications here.",
    },
] as const satisfies ReadonlyArray<NotificationPreferenceType>;

export function getNotificationPreferenceTypes(): ReadonlyArray<NotificationPreferenceType> {
    return NOTIFICATION_PREFERENCE_TYPES;
}
