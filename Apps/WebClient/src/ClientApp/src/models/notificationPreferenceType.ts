import { ProfileNotificationType } from "@/constants/profileNotifications";

export interface NotificationPreferenceType {
    id: string;
    type: ProfileNotificationType;
    label: string;
    tooltip: string;
}
