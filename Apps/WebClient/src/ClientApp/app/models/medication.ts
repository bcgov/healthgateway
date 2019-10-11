// Medication model
export default class Medication {
  // Drug Identification Number for the medication.
  public DIN?: string;
  /// The medication form (tablet/drop/etc).
  public form?:string;
  // Brand name of the medication.
  public brandName?: string;
  // Common or generic name of the medication.
  public genericName?: string;
  // Quantity for the medication statement.
  public quantity?: number;
  // Medication dosage.
  public dosage?: number;
  // The medication dose if complex (50MCG-5/MLDROPS)
  public complexDose?:string;
  // Medication dosage unit (mg/ml/etc).
  public dosageUnit?:string;
  // The medication manufacturer.
  public manufacturer?:string;
  // Drug medication discontinued date, if applicable.
  public drugDiscontinuedDate?: Date;
}
