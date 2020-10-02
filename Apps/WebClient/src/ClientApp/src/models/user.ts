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
