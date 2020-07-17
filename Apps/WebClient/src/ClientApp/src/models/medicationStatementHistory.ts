import MedicationSumary from "./medicationSumary";
import Pharmacy from "./pharmacy";

// Medication statement model
export default interface MedicationStatementHistory {
    // The medication statement identifier.
    prescriptionIdentifier?: string;
    // Medication statement prescription status.
    prescriptionStatus?: string;
    // Date the medication statement was dispensed.
    dispensedDate: Date;
    // Surname of the Practitioner who issued the medication statement.
    practitionerSurname?: string;
    // Drug medication discontinued date, if applicable.
    directions?: string;
    // Date the medication statement was entered.
    dateEntered?: Date;
    // The medication of this MedicationStatement.
    medicationSumary: MedicationSumary;
    // The pharmacy where the medication was filled.
    pharmacyId?: string;
    // Gets or sets the dispensing pharmacy for the current MedicationStatementHistory.
    dispensingPharmacy: Pharmacy;
}
