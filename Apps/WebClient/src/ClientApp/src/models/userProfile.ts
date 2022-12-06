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
    lastLoginDateTimes?: StringISODateTime[];

    // Date when the user profile will be deleted
    closedDateTime?: StringISODateTime;

    // The User Preference
    preferences: Dictionary<UserPreference>;
}

export interface CreateUserRequest {
    // User profile to create.
    profile: UserProfile;

    // Code used to validate if the user has an invite.
    inviteCode: string;
}
