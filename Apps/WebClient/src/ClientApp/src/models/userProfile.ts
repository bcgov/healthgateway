import type { UserPreference } from "./userPreference";

export interface UserProfile {
    hdid: string;
    acceptedTermsOfService: boolean;
    email: string;
    termsOfServiceId: string;
    isEmailVerified: boolean;
    smsNumber: string;
    isSMSNumberVerified: boolean;
    hasTermsOfServiceUpdated?: boolean;
    lastLoginDateTime?: string;
    lastLoginDateTimes: string[];
    closedDateTime?: string;
    preferences: Record<string, UserPreference>;
    hasTourUpdated?: boolean;
    blockedDataSources?: string[];
    betaFeatures?: string[];
    notificationSettings: UserProfileNotificationSettingModel[];
}

export enum UserProfileNotificationType {
    BcCancerScreening = "BcCancerScreening",
}

export interface UserProfileNotificationSettingModel {
    type: UserProfileNotificationType | string;
    emailEnabled?: boolean | null;
    smsEnabled?: boolean | null;
}

export interface TermsOfService {
    id: string;
    content: string;
    effectiveDate: string;
}

export interface CreateUserRequest {
    profile: UserProfile;
}
