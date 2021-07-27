import { Dictionary } from "@/models/baseTypes";
import { StringISODate } from "@/models/dateWrapper";
import type { UserPreference } from "@/models/userPreference";

export default class User {
    public hdid = "";
    public acceptedTermsOfService = false;
    public hasEmail = false;
    public verifiedEmail = false;
    public hasSMS = false;
    public verifiedSMS = false;
    public hasTermsOfServiceUpdated = false;
    public closedDateTime?: StringISODate;
    public preferences: Dictionary<UserPreference> = {};
}

export interface OidcUserProfile {
    email_verified: string;
    family_name: string;
    given_name: string;
    hdid: string;
    idp: string;
    name: string;
    email: string;
}
