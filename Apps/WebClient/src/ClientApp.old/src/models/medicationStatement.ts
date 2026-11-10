import { StringISODate } from "@/models/dateWrapper";
import MedicationSummary from "@/models/medicationSummary";
import Pharmacy from "@/models/pharmacy";

// Medication statement model
export default interface MedicationStatement {
    // Brand name of the medication.
    prescriptionIdentifier: string;
    // Date the medication statement was dispensed.
    dispensedDate: StringISODate;
    // Surname of the practitioner who prescribed the medication.
    practitionerSurname?: string;
    // Directions as prescribed.
    directions?: string;
    // Medication summary.
    medicationSummary: MedicationSummary;
    // Dispensing pharmacy.
    dispensingPharmacy: Pharmacy;
}
