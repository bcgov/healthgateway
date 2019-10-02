// Medication prescription model
export default class Prescription {
  // Drug Identification Number for the prescribed medication.
  public DIN!: string;
  // Brand name of the prescribed medication.
  public brandName!: string;
  // Common or generic name of the medication prescribed.
  public genericName!: string;
  // Quantity for the prescription.
  public quantity!: number;
  // Prescription dosage.
  public dosage!: number;
  // Prescription status.
  public prescriptionStatus!: string;
  // Date the prescription was dispensed.
  public dispensedDate!: Date;
  // Surname of the Practitioner who issued the prescription.
  public practitionerSurname!: string;
  // Drug prescription discontinued date, if applicable.
  public drugDiscontinuedDate!: Date;
  // Prescriptions directions.
  public directions!: string;
  // Date the prescription was entered.
  public dateEntered!: Date;
}
