import { StringISODate } from "@/models/dateWrapper";

export interface UserPreference {
    hdId?: string;
    preference: string;
    value: string;
    createdDateTime: StringISODate;
    version: number;
}
