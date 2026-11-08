import type { UserPreference } from "./userPreference";
import type { UserProfileNotificationSettingModel } from "./userProfile";

export class User {
    hdid = "";
    acceptedTermsOfService = false;
    email = "";
    hasEmail = false;
    verifiedEmail = false;
    sms = "";
    hasSms = false;
    verifiedSms = false;
    hasTermsOfServiceUpdated = false;
    lastLoginDateTime?: string;
    lastLoginDateTimes: string[] = [];
    closedDateTime?: string;
    preferences: Record<string, UserPreference> = {};
    hasTourUpdated = false;
    blockedDataSources: string[] = [];
    betaFeatures: string[] = [];
    notificationSettings: UserProfileNotificationSettingModel[] = [];
}

/** Claims returned by the Keycloak userinfo endpoint. */
export interface OidcUserInfo {
    sub: string;
    email_verified?: boolean | string;
    family_name?: string;
    given_name?: string;
    hdid?: string;
    idp?: string;
    name?: string;
    email?: string;
    preferred_username?: string;
}

export interface OidcTokenDetails {
    idToken: string;
    sessionState?: string;
    accessToken: string;
    refreshToken?: string;
    expired: boolean;
    /** Unix time (seconds) when the access token should be refreshed. */
    refreshTokenTime: number;
    hdid: string;
}
