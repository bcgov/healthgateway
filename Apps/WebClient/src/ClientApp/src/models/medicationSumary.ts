// Medication model
export default class MedicationSumary {
  // Drug Identification Number for the medication.
  public din?: string;
  // Brand name of the medication.
  public brandName?: string;
  // Common or generic name of the medication.
  public genericName?: string;
  // Max quantity for the medication statement.
  public maxDailyDosage?: number;
  // Quantity for the medication statement.
  public quantity?: number;
  // Drug medication discontinued date, if applicable.
  public drugDiscontinuedDate?: Date;
}
