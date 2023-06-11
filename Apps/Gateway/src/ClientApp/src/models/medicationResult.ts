export default class MedicationResult {
    public din?: string;
    public federalData?: FederalDrugSource;
    public provincialData?: ProvincialDrugSource;
}

class FederalDrugSource {
    public updateDateTime?: Date;
    public drugProduct?: DrugProduct;
}

class DrugProduct {
    public accessionNumber?: string;
    public aiGroupNumber?: string;
    public brandName?: string;
    public brandNameFrench?: string;
    public descriptor?: string;
    public descriptorFrench?: string;
    public drugClass?: string;
    public drugClassFrench?: string;
    public drugCode?: string;
    public drugIdentificationNumber?: string;
    public fileDownload?: string;
    public fileDownloadId?: string;
    public id?: string;
    public lastUpdate?: string;
    public numberOfAis?: string;
    public pediatricFlag?: string;
    public productCategorization?: string;

    public activeIngredient?: ActiveIngredient;
    public form?: Form;
    public company?: Company;
}

class ActiveIngredient {
    public activeIngredientCode?: string;
    public base?: string;
    public dosageUnit?: string;
    public dosageUnitFrench?: string;
    public dosageValue?: string;
    public drugProduct?: string;
    public id?: string;
    public ingredient?: string;
    public ingredientFrench?: string;
    public ingredientSuppliedInd?: string;
    public notes?: string;
    public strength?: string;
    public strengthType?: string;
    public strengthTypeFrench?: string;
    public strengthUnit?: string;
    public strengthUnitFrench?: string;
}

class Form {
    public drugProduct?: string;
    public id?: string;
    public pharmaceuticalForm?: string;
    public pharmaceuticalFormCode?: string;
    public pharmaceuticalFormFrench?: string;
}

class Company {
    public addressBillingFlag?: string;
    public addressMailingFlag?: string;
    public addressNotificationFlag?: string;
    public addressOther?: string;
    public cityName?: string;
    public companyCode?: string;
    public companyName?: string;
    public companyType?: string;
    public country?: string;
    public countryFrench?: string;
    public drugProduct?: string;
    public id?: string;
    public manufacturerCode?: string;
    public postOfficeBox?: string;
    public postalCode?: string;
    public province?: string;
    public provinceFrench?: string;
    public streetName?: string;
    public suiteNumber?: string;
}

class ProvincialDrugSource {
    public updateDateTime?: Date;
    public pharmaCareDrug?: PharmaCareDrug;
}

class PharmaCareDrug {
    public benefitGroupList?: string;
    public brandName?: string;
    public cfrCode?: string;
    public dinPin?: string;
    public dosageForm?: string;
    public effectiveDate?: string;
    public endDate?: string;
    public fileDownload?: string;
    public fileDownloadId?: string;
    public formularyListDate?: string;
    public genericName?: string;
    public id?: string;
    public lcaIndicator?: string;
    public lcaPrice?: number;
    public limitedUseFlag?: string;
    public manufacturer?: string;
    public maximumDaysSupply?: number;
    public maximumPrice?: number;
    public payGenericIndicator?: string;
    public pharmaCarePlanDescription?: string;
    public plan?: string;
    public quantityLimit?: number;
    public rdpCategory?: string;
    public rdpExcludedPlans?: string;
    public rdpPrice?: string;
    public rdpSubCategory?: string;
    public trialFlag?: string;
}
