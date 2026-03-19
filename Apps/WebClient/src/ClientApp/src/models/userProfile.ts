import { BetaFeature } from "@/constants/betaFeature";
import { DataSource } from "@/constants/dataSource";
import { Dictionary } from "@/models/baseTypes";
import { StringISODateTime } from "@/models/dateWrapper";
import type { UserPreference } from "@/models/userPreference";

export default interface UserProfile {
    // The user hdid.
    hdid: string;

    // Value indicating whether the user accepted the terms of service.
    acceptedTermsOfService: boolean;

    // The user email.
    email: string;

    // The ToS ID that the user has accepted.
    termsOfServiceId: string;

    // Indicates whether the email was verified.
    isEmailVerified: boolean;

    // The user SMS number.
    smsNumber: string;

    // Indicates whether the sms number was verified.
    isSMSNumberVerified: boolean;

    // Flag to know if the terms of service have been updated since last login
    hasTermsOfServiceUpdated?: boolean;

    // the user's last login time
    lastLoginDateTime?: StringISODateTime;

    // collection of the user's last login times
    lastLoginDateTimes: StringISODateTime[];

    // Date when the user profile will be deleted
    closedDateTime?: StringISODateTime;

    // The User Preference
    preferences: Dictionary<UserPreference>;

    // Has the app tour been updated
    hasTourUpdated?: boolean;

    // User's blocked access to data source(s)
    blockedDataSources?: DataSource[];

    // Beta features available to the user
    betaFeatures?: BetaFeature[];

    // Profile notification settings for the user
    notificationSettings: UserProfileNotificationSettingModel[];
}

export enum UserProfileNotificationType {
    BcCancerScreening = "BcCancerScreening",
}

export interface UserProfileNotificationSettingModel {
    type: UserProfileNotificationType;
    emailEnabled: boolean;
    smsEnabled: boolean;
}

export interface CreateUserRequest {
    // User profile to create.
    profile: UserProfile;
}
