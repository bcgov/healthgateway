import {
    ProfileNotificationPreference,
    ProfileNotificationType,
} from "@/constants/profileNotifications";

export interface UserProfileNotificationSettings {
    type: ProfileNotificationType;
    preferences: ProfileNotificationPreference[];
}
