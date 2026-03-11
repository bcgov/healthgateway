export default interface MedicationActiveIngredient {
    activeIngredientCode: number;
    ingredient?: string;
    ingredientFrench?: string;
    ingredientSuppliedInd?: string;
    strength?: string;
    strengthUnit?: string;
    strengthUnitFrench?: string;
    strengthType?: string;
    strengthTypeFrench?: string;
    dosageValue?: string;
    base?: string;
    dosageUnit?: string;
    dosageUnitFrench?: string;
    notes?: string;
}
