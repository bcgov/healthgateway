import { StringISODate } from "@/models/dateWrapper";

export default interface MedicationRequest {
    // Gets or sets the drug name.
    drugName?: string;

    // Gets or sets the request status.
    requestStatus?: string;

    // Gets or sets the requested date.
    requestedDate: StringISODate;

    // Gets or sets the prescriber's firstname.
    prescriberFirstName?: string;

    // Gets or sets the prescriber's lastname.
    prescriberLastName?: string;

    // Gets or sets the effective date.
    effectiveDate?: StringISODate;

    // Gets or sets the expiry date.
    expiryDate?: StringISODate;

    // Gets or sets the reference number.
    referenceNumber: string;
}
