import MedicationSumary from "./medicationSumary";

// Medication statement model
export default interface MedicationStatement {
    // The prescription identifier for this statment.
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
}
