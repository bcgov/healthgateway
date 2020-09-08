import { Dictionary } from "vue-router/types/router";

export default class User {
    public hdid: string = "";
    public acceptedTermsOfService: boolean = false;
    public hasEmail: boolean = false;
    public verifiedEmail: boolean = false;
    public hasSMS: boolean = false;
    public verifiedSMS: boolean = false;
    public hasTermsOfServiceUpdated: boolean = false;
    public SMSResendDateTime?: Date;
    public closedDateTime?: Date;
    public preferences: Dictionary<string> = {};
}
