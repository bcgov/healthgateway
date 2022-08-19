import { StringISODate } from "@/models/dateWrapper";

export default interface AddDependentRequest {
    // Dependent first name.
    firstName: string;

    // The dependent last name.
    lastName: string;

    // Dependent date of birth.
    dateOfBirth: StringISODate;

    // Dependent PHN.
    PHN: string;
}
