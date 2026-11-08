/** Port of models/medicationRequest.ts (Special Authority requests) */
export interface MedicationRequest {
    drugName?: string;
    requestStatus?: string;
    /** ISO date */
    requestedDate: string;
    prescriberFirstName?: string;
    prescriberLastName?: string;
    effectiveDate?: string;
    expiryDate?: string;
    referenceNumber: string;
}
