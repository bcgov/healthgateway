import { Dictionary } from "vue-router/types/router";
import { StringISODate } from "./dateWrapper";

export default class User {
    public hdid: string = "";
    public acceptedTermsOfService: boolean = false;
    public hasEmail: boolean = false;
    public verifiedEmail: boolean = false;
    public hasSMS: boolean = false;
    public verifiedSMS: boolean = false;
    public hasTermsOfServiceUpdated: boolean = false;
    public closedDateTime?: StringISODate;
    public preferences: Dictionary<string> = {};
}
