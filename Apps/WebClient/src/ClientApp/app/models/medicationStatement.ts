// Medication statement model
export default class MedicationStatement {
  // Drug Identification Number for the medication statement.
  public DIN?: string;
  // Brand name of the medication statement.
  public brandName?: string;
  // Common or generic name of the medication statement.
  public genericName?: string;
  // Quantity for the medication statement.
  public quantity?: number;
  // Medication statement dosage.
  public dosage?: number;
  // Medication statement prescription status.
  public prescriptionStatus?: string;
  // Date the medication statement was dispensed.
  public dispensedDate?: Date;
  // Surname of the Practitioner who issued the medication statement.
  public practitionerSurname?: string;
  // Drug medication discontinued date, if applicable.
  public drugDiscontinuedDate?: Date;
  // Medication statement directions.
  public directions?: string;
  // Date the medication statement was entered.
  public dateEntered?: Date;
}
