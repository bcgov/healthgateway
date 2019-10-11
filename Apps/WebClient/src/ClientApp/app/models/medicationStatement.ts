import Medication from "./medication";
import Pharmacy from "./pharmacy";

// Medication statement model
export default class MedicationStatement {
  // The prescription identifier for this statment.
  public prescriptionIdentifier?:string
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
  public medication: Medication = new Medication();
  // The pharmacy where the medication was filled.
  public pharmacy: Pharmacy = new Pharmacy();
}
