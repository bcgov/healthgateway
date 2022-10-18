import { StringISODateTime } from "@/models/dateWrapper";

export default interface User {
    personalHealthNumber: string;
    hdid: string;
    lastLoginDateTime: StringISODateTime;
}
