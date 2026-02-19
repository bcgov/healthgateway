import { NotificationPreferenceType } from "@/models/notificationPreferenceType";
import {
    UserProfileNotificationSettingModel,
    UserProfileNotificationType,
} from "@/models/userProfile";
import { UserProfileNotificationSettings } from "@/models/userProfileNotificationSettings";

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

function assertNever(x: never): never {
    throw new Error(`Unhandled value: ${String(x)}`);
}

export function getUserProfileNotificationSettings(
    settings: UserProfileNotificationSettingModel[] | undefined
): UserProfileNotificationSettings[] {
    return (settings ?? [])
        .map((setting) => {
            const profileNotificationType = getProfileNotificationType(
                setting.type
            ); // "BcCancerScreening" to "bcCancerScreening"
            if (!profileNotificationType) return null;

            const preferences: ProfileNotificationPreference[] = [];
            if (setting.emailEnabled)
                preferences.push(ProfileNotificationPreference.Email);
            if (setting.smsEnabled)
                preferences.push(ProfileNotificationPreference.Sms);

            return { type: profileNotificationType, preferences };
        })
        .filter(
            (result): result is UserProfileNotificationSettings =>
                result !== null
        );
}

export function getUserProfileNotificationType(
    type: ProfileNotificationType
): UserProfileNotificationType {
    switch (type) {
        case ProfileNotificationType.BcCancerScreening:
            return UserProfileNotificationType.BcCancerScreening;
        default:
            return assertNever(type);
    }
}

export function getProfileNotificationType(
    type: UserProfileNotificationType
): ProfileNotificationType | null {
    switch (type) {
        case "BcCancerScreening":
            return ProfileNotificationType.BcCancerScreening;
        default:
            return null;
    }
}
