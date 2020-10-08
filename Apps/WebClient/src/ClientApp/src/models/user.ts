import { Dictionary } from "vue-router/types/router";
import { StringISODate } from "@/models/dateWrapper";

export default class User {
    public hdid = "";
    public acceptedTermsOfService = false;
    public hasEmail = false;
    public verifiedEmail = false;
    public hasSMS = false;
    public verifiedSMS = false;
    public hasTermsOfServiceUpdated = false;
    public closedDateTime?: StringISODate;
    public preferences: Dictionary<string> = {};
}

export interface OidcUserProfile {
    email_verified: string;
    family_name: string;
    given_name: string;
    hdid: string;
    idp: string;
    name: string;
}
