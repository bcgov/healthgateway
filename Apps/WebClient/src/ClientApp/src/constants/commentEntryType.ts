export const enum CommentEntryType {
    // The code represeting no entry type set.
    None = "NA",
    // The code representing Special Authority Requests.
    MedicationRequest = "SAR",
    // The code representing Medication.
    Medication = "Med",
    // The code representing Immunization.
    Immunization = "Imm",
    // The code representing COVID-19 Laboratory Orders.
    Covid19LaboratoryOrder = "Lab",
    // The code representing all Laboratory Orders.
    LaboratoryOrder = "ALO",
    // The code representing Encounter.
    Encounter = "Enc",
}
