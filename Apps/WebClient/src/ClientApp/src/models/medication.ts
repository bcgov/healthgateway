/** Port of models/medicationStatement.ts, medicationSummary.ts, pharmacy.ts and medicationActiveIngredient.ts */
export interface MedicationStatement {
    prescriptionIdentifier: string;
    /** ISO date */
    dispensedDate: string;
    practitionerSurname?: string;
    directions?: string;
    medicationSummary: MedicationSummary;
    dispensingPharmacy: Pharmacy;
}

export interface MedicationSummary {
    din: string;
    brandName: string;
    genericName: string;
    quantity?: number;
    manufacturer?: string;
    form?: string;
    strength?: string;
    strengthUnit?: string;
    isPin: boolean;
    pharmacyAssessmentTitle: string;
    prescriptionProvided?: boolean;
    redirectedToHealthCareProvider?: boolean;
    title: string;
    subtitle: string;
    isPharmacistAssessment: boolean;
    activeIngredients: MedicationActiveIngredient[];
}

export interface Pharmacy {
    pharmacyId?: string;
    name?: string;
    addressLine1?: string;
    addressLine2?: string;
    city?: string;
    province?: string;
    postalCode?: string;
    countryCode?: string;
    phoneNumber?: string;
    faxNumber?: string;
}

export interface MedicationActiveIngredient {
    activeIngredientCode: number;
    ingredient?: string;
    strength?: string;
    strengthUnit?: string;
}
