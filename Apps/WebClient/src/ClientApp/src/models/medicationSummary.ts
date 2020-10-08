// Medication model
export default interface MedicationSummary {
    // Drug Identification Number for the medication.
    din: string;
    // Brand name of the medication.
    brandName: string;
    // Common or generic name of the medication.
    genericName: string;
    // Max quantity for the medication statement.
    maxDailyDosage?: number;
    // Quantity for the medication statement.
    quantity?: number;
    // Drug medication discontinued date, if applicable.
    drugDiscontinuedDate?: Date;
    // Manufacturer
    manufacturer?: string;
    // Form
    form?: string;
    // Strength
    strength?: string;
    // Strength Unit
    strengthUnit?: string;
    // Is Provincial Drug
    isPin: boolean;
}
