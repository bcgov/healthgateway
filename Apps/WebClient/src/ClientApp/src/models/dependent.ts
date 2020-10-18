import { StringISODate } from "@/models/dateWrapper";

export interface Dependent {
    // Gets or sets the id.
    id?: string;

    // Gets or sets the dependent first name.
    name: string;

    // Gets or sets the dependent last name.
    maskedPHN: string;

    // Gets or sets the dependent date of birth.
    dateOfBirth: StringISODate;

    // Gets or sets the dependent gender.
    gender: string

    // Gets or sets the comment db version.
    version: number;/// <summary>
}
