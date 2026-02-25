import { NotificationPreferenceType } from "@/models/notificationPreferenceType";
import {
    UserProfileNotificationSettingModel,
    UserProfileNotificationType,
} from "@/models/userProfile";
import { UserProfileNotificationSettings } from "@/models/userProfileNotificationSettings";

/**
 * Represents backend-supported notification types.
 *
 * IMPORTANT:
 * - The string value must match the backend enum name (camelCase).
 * - This value is used for API communication and feature toggle mapping.
 *
 * When adding a new notification type:
 * 1. Add a new enum member here (e.g., DiagnosticImaging = "diagnosticImaging").
 * 2. Add a corresponding entry in NOTIFICATION_PREFERENCE_TYPES below.
 * 3. Add a matching feature toggle entry in featuretoggleconfig.json
 *    under profile.notifications.type with the same "name" value.
 */
export enum ProfileNotificationType {
    BcCancerScreening = "bcCancerScreening",
}

export enum ProfileNotificationPreference {
    Email = "email",
    Sms = "sms",
}

/**
 * UI configuration for profile notification preference types.
 *
 * Each entry:
 * - id: UI identifier (kebab-case; used as stable key in components)
 * - type: Backend notification type (must match ProfileNotificationType)
 * - label: Display text shown in the profile page
 * - tooltip: Help text shown in the info tooltip
 *
 * IMPORTANT:
 * - Every ProfileNotificationType must have a corresponding entry here.
 * - The feature toggle (featuretoggleconfig.json) must contain a matching
 *   "type.name" entry using the same camelCase value (e.g., "bcCancerScreening").
 */
export const NOTIFICATION_PREFERENCE_TYPES = [
    {
        id: "bc-cancer-screening",
        type: ProfileNotificationType.BcCancerScreening,
        label: "Eligible BC Cancer Screening Letters",
        tooltip: {
            text: "Learn more about BC Cancer notifications ",
            linkText: "here",
            href: "/not-found",
            suffix: ".",
        },
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
