import { KeycloakUserInfo } from "keycloak-js";

import { BetaFeature } from "@/constants/betaFeature";
import { DataSource } from "@/constants/dataSource";
import { Dictionary } from "@/models/baseTypes";
import { StringISODateTime } from "@/models/dateWrapper";
import type { UserPreference } from "@/models/userPreference";

export default class User {
    public hdid = "";
    public acceptedTermsOfService = false;
    public email = "";
    public hasEmail = false;
    public verifiedEmail = false;
    public sms = "";
    public hasSms = false;
    public verifiedSms = false;
    public hasTermsOfServiceUpdated = false;
    public lastLoginDateTime?: StringISODateTime;
    public lastLoginDateTimes: StringISODateTime[] = [];
    public closedDateTime?: StringISODateTime;
    public preferences: Dictionary<UserPreference> = {};
    public hasTourUpdated = false;
    public blockedDataSources: DataSource[] = [];
    public betaFeatures: BetaFeature[] = [];
}

export interface OidcUserInfo extends KeycloakUserInfo {
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
