import { Dictionary } from "@/models/baseTypes";
import { StringISODateTime } from "@/models/dateWrapper";
import type { UserPreference } from "@/models/userPreference";

export default class User {
    public hdid = "";
    public acceptedTermsOfService = false;
    public hasEmail = false;
    public verifiedEmail = false;
    public hasSMS = false;
    public verifiedSMS = false;
    public hasTermsOfServiceUpdated = false;
    public lastLoginDateTime?: StringISODateTime;
    public lastLoginDateTimes: StringISODateTime[] = [];
    public closedDateTime?: StringISODateTime;
    public preferences: Dictionary<UserPreference> = {};
    public hasTourUpdated = false;
}

export interface OidcUserInfo {
    email_verified: string;
    family_name: string;
    given_name: string;
    hdid: string;
    idp: string;
    name: string;
    email: string;
}

export interface OidcTokenDetails {
    /** The id_token returned from the OIDC provider. */
    idToken: string;
    /** The session state value returned from the OIDC provider (opaque). */
    sessionState?: string;
    /** The access token returned from the OIDC provider. */
    accessToken: string;
    /** Refresh token returned from the OIDC provider (if requested). */
    refreshToken?: string;
    /** Whether the token is expired. */
    expired: boolean;
    /** Time when the access token should be refreshed, based on the OIDC "exp" claim. */
    refreshTokenTime: number;
    /** HDID for the user. */
    hdid: string;
}