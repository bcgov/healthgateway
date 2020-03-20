import MedicationSumary from "./medicationSumary";

// Medication statement model
export default class ODRMedicationStatement {
  // The medication statement identifier.
  public recordId?: string;
  // The prescription identifier for this statement.
  public rxNumber?: string;
  // Medication statement prescription status.
  public rxStatus?: string;
  // Medication statement quantity.
  public quantity?: number;
  // Medication statement refills.
  public refills?: number;
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
}
