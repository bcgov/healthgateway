import { Dictionary } from "vue-router/types/router";
import { StringISODate } from "@/models/dateWrapper";

export default class User {
    public hdid: string = "";
    public acceptedTermsOfService: boolean = false;
    public hasEmail: boolean = false;
    public verifiedEmail: boolean = false;
    public hasSMS: boolean = false;
    public verifiedSMS: boolean = false;
    public hasTermsOfServiceUpdated: boolean = false;
<<<<<<< HEAD
    public closedDateTime?: StringISODate;
=======
    public SMSResendDateTime?: Date;
    public closedDateTime?: Date;
>>>>>>> master
    public preferences: Dictionary<string> = {};
}
