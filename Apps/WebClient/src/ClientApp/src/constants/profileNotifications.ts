// Port of constants/profileNotifications.ts (+ models/notificationPreferenceType.ts and
// models/userProfileNotificationSettings.ts). Copy, ids and links must stay identical to the web client.
import type { InformationModalContent } from "@/models/informationModal";
import {
    type UserProfileNotificationSettingModel,
    UserProfileNotificationType,
} from "@/models/userProfile";

import { ExternalUrl } from "./externalUrls";
import { Destination, Origin, Text, Type } from "./tracking";

export enum ProfileNotificationType {
    BcCancerScreening = "bcCancerScreening",
}

export enum ProfileNotificationPreference {
    Email = "email",
    Sms = "sms",
}

export interface NotificationModalConfig {
    buttonText: string;
    content: InformationModalContent;
}

export interface NotificationPreferenceType {
    /** UI identifier (kebab-case; used as stable key and in test ids) */
    id: string;
    /** Backend notification type (must match ProfileNotificationType) */
    type: ProfileNotificationType;
    /** Display text shown on the profile page */
    label: string;
    /** Help text shown in the information modal */
    modal: NotificationModalConfig;
}

export interface UserProfileNotificationSettings {
    type: ProfileNotificationType;
    preferences: ProfileNotificationPreference[];
}

/**
 * Every ProfileNotificationType must have an entry here, and the feature toggle must contain a matching
 * "type.name" entry using the same camelCase value (e.g. "bcCancerScreening").
 */
export const NOTIFICATION_PREFERENCE_TYPES: readonly NotificationPreferenceType[] =
    [
        {
            id: "bc-cancer-screening",
            type: ProfileNotificationType.BcCancerScreening,
            label: "BC Cancer Screening Letters",
            modal: {
                buttonText: "Learn More",
                content: {
                    title: "BC Cancer Screening Program Letters",
                    paragraphs: [
                        {
                            segments: [
                                {
                                    type: "text",
                                    value: "Cancer screening program letters are available on Health Gateway. Health Gateway can email and/or text (SMS) you when new letters are added that show you need follow-up or when you are due to screen again.",
                                },
                            ],
                        },
                        {
                            segments: [
                                {
                                    type: "text",
                                    value: "Choosing to receive email and/or text (SMS) notifications will not change the paper letters that will be mailed to you.",
                                },
                            ],
                        },
                        {
                            segments: [
                                {
                                    type: "text",
                                    value: "For more information about BC Cancer Screening Program letters, visit ",
                                },
                                {
                                    type: "link",
                                    text: "screeningbc.ca/contact",
                                    href: ExternalUrl.ScreeningContact,
                                    analyticsText:
                                        Text.ScreeningNotificationsContactLink,
                                    analyticsOrigin: Origin.Profile,
                                    analyticsDestination:
                                        Destination.ScreeningContact,
                                    analyticsType: Type.Notifications,
                                    analysticsUrl: ExternalUrl.ScreeningContact,
                                },
                            ],
                        },
                    ],
                },
            },
        },
    ];

export function getNotificationPreferenceTypes(): readonly NotificationPreferenceType[] {
    return NOTIFICATION_PREFERENCE_TYPES;
}

/** "bcCancerScreening" → "BcCancerScreening" */
export function getUserProfileNotificationType(
    type: ProfileNotificationType
): UserProfileNotificationType {
    switch (type) {
        case ProfileNotificationType.BcCancerScreening:
            return UserProfileNotificationType.BcCancerScreening;
    }
}

/** "BcCancerScreening" → "bcCancerScreening"; unknown API types → null */
export function getProfileNotificationType(
    type: string
): ProfileNotificationType | null {
    switch (type) {
        case "BcCancerScreening":
            return ProfileNotificationType.BcCancerScreening;
        default:
            return null;
    }
}

export function getUserProfileNotificationSettings(
    settings: UserProfileNotificationSettingModel[] | undefined
): UserProfileNotificationSettings[] {
    return (settings ?? [])
        .map((setting) => {
            const type = getProfileNotificationType(setting.type);
            if (!type) return null;
            const preferences: ProfileNotificationPreference[] = [];
            if (setting.emailEnabled)
                preferences.push(ProfileNotificationPreference.Email);
            if (setting.smsEnabled)
                preferences.push(ProfileNotificationPreference.Sms);
            return { type, preferences };
        })
        .filter((r): r is UserProfileNotificationSettings => r !== null);
}
