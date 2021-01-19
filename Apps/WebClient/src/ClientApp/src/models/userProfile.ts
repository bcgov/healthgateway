import { Dictionary } from "@/models/baseTypes";
import { StringISODate } from "@/models/dateWrapper";
import type { UserPreference } from "@/models/userPreference";

export default interface UserProfile {
    // The user hdid.
    hdid: string;

    // Value indicating whether the user accepted the terms of service.
    acceptedTermsOfService: boolean;

    // The user email.
    email: string;

    // The user SMS number.
    smsNumber: string;

    // Flag to know if the terms of service have been updated since last login
    hasTermsOfServiceUpdated?: boolean;

    // Datetime of the user's last login
    lastLoginDateTime?: StringISODate;

    // Date when the user profile will be deleted
    closedDateTime?: StringISODate;

    // The User Preference
    preferences: Dictionary<UserPreference>;
}

export interface CreateUserRequest {
    // User profile to create.
    profile: UserProfile;

    // Code used to validate if the user has an invite.
    inviteCode: string;
}
