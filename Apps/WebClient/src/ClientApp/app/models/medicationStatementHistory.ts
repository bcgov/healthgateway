import MedicationSumary from "./medicationSumary";
import Pharmacy from "./pharmacy";

// Medication statement model
export default class MedicationStatementHistory {
  // The medication statement identifier.
  public prescriptionIdentifier?: string;
  // Medication statement prescription status.
  public prescriptionStatus?: string;
  // Date the medication statement was dispensed.
  public dispensedDate?: Date;
  // Surname of the Practitioner who issued the medication statement.
  public practitionerSurname?: string;
  // Drug medication discontinued date, if applicable.
  public directions?: string;
  // Date the medication statement was entered.
  public dateEntered?: Date;
  // The medication of this MedicationStatement.
  public medicationSumary: MedicationSumary = new MedicationSumary();
  // The pharmacy where the medication was filled.
  public pharmacyId?: string;
  // Gets or sets the dispensing pharmacy for the current MedicationStatementHistory.
  public dispensingPharmacy: Pharmacy = new Pharmacy();
}
