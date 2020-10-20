import { StringISODate } from "@/models/dateWrapper";

export interface AddDependentRequest {
    // Gets or sets the id.
    id?: string;

    // Gets or sets the dependent first name.
    firstName: string;

    // Gets or sets the dependent last name.
    lastName: string;

    // Gets or sets the dependent date of birth.
    dateOfBirth: StringISODate;

    // Gets or sets the dependent PHN.
    PHN: string;

    // Gets or sets the dependent gender.
    gender: string;

    // Gets or sets the comment db version.
    version: number;
}
