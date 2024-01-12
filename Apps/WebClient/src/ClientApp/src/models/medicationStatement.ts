import { StringISODate } from "@/models/dateWrapper";
import MedicationSummary from "@/models/medicationSummary";
import Pharmacy from "@/models/pharmacy";

// Medication statement model
export default interface MedicationStatement {
    // Brand name of the medication.
    prescriptionIdentifier: string;
    // Prescription status.
    prescriptionStatus?: string;
    // Date the medication statement was dispensed.
    dispensedDate: StringISODate;
    // Surname of the practitioner who prescribed the medication.
    practitionerSurname?: string;
    // Directions as prescribed.
    directions?: string;
    // Date the medication was entered.
    dateEntered?: StringISODate;
    // Medication summary.
    medicationSummary: MedicationSummary;
    // Pharmacy ID.
    pharmacyId?: string;
    // Dispensing pharmacy.
    dispensingPharmacy: Pharmacy;
}
