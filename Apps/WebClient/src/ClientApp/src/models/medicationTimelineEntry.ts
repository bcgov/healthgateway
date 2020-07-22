import TimelineEntry, { EntryType } from "@/models/timelineEntry";
import Pharmacy from "./pharmacy";
import MedicationSumary from "./medicationSumary";
import MedicationResult from "./medicationResult";
import MedicationStatementHistory from "./medicationStatementHistory";

// The medication timeline entry model
export default class MedicationTimelineEntry extends TimelineEntry {
    public pharmacy: PharmacyViewModel;
    public medication: MedicationViewModel;
    public directions: string;
    public practitionerSurname: string;
    public prescriptionIdentifier: string;

    public constructor(model: MedicationStatementHistory) {
        super(
            model.prescriptionIdentifier,
            EntryType.Medication,
            model.dispensedDate
        );

        this.medication = new MedicationViewModel(model.medicationSumary);
        this.pharmacy = new PharmacyViewModel(
            model.dispensingPharmacy?.pharmacyId
        );

        if (model.dispensingPharmacy) {
            this.pharmacy.populateFromModel(model.dispensingPharmacy);
        }

        this.practitionerSurname = model.practitionerSurname || "N/A";
        this.prescriptionIdentifier = model.prescriptionIdentifier || "N/A";
        this.directions = model.directions || "N/A";
    }

    public filterApplies(filterText: string, filterTypes: string[]): boolean {
        if (!filterTypes.includes("Medication")) {
            return false;
        }

        let text =
            (this.practitionerSurname! || "") +
            (this.medication.brandName! || "") +
            (this.medication.genericName! || "");
        text = text.toUpperCase();
        return text.includes(filterText.toUpperCase());
    }
}

class PharmacyViewModel {
    public id: string;
    public name?: string;
    public address?: string;
    public phoneType?: string;
    public phoneNumber?: string;

    constructor(id: string | undefined) {
        this.id = id ? id : "";
    }

    public populateFromModel(model: Pharmacy): void {
        this.name = model.name;
        this.phoneType = model.phoneType;
        this.phoneNumber = model.phoneNumber;

        this.address =
            model.addressLine1 +
            " " +
            model.addressLine2 +
            ", " +
            model.city +
            " " +
            model.province;
    }
}

class MedicationViewModel {
    public din: string;
    public brandName: string;
    public genericName?: string;
    public quantity?: number;
    public form?: string;
    public strength?: string;
    public strengthUnit?: string;
    public manufacturer?: string;
    public isPin!: boolean;
    constructor(model: MedicationSumary) {
        this.din = model.din ? model.din : "";
        this.brandName = model.brandName;
        this.genericName = model.genericName;
        this.quantity = model.quantity;
    }

    public populateFromModel(model: MedicationResult): void {
        if (model.federalData) {
            const federalModel = model.federalData;

            this.form = federalModel.drugProduct!.form!.pharmaceuticalForm
                ? federalModel.drugProduct!.form!.pharmaceuticalForm
                : "";

            this.strength = federalModel.drugProduct!.activeIngredient!.strength
                ? federalModel.drugProduct!.activeIngredient!.strength
                : "";

            this.strengthUnit = federalModel.drugProduct!.activeIngredient!
                .strengthUnit
                ? federalModel.drugProduct!.activeIngredient!.strengthUnit
                : "";

            this.manufacturer = federalModel.drugProduct!.company!.companyName
                ? federalModel.drugProduct!.company!.companyName
                : "";
        } else if (model.provincialData) {
            const provincialModel = model.provincialData;
            this.isPin = true;
            this.form = provincialModel.pharmaCareDrug
                ? provincialModel.pharmaCareDrug.dosageForm
                : "";
        }
    }
}
