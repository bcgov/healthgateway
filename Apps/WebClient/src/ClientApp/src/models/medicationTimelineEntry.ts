import TimelineEntry, { EntryType } from "@/models/timelineEntry";
import Pharmacy from "@/models/pharmacy";
import MedicationStatementHistory from "@/models/medicationStatementHistory";
import MedicationSummary from "@/models//medicationSummary";

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

        this.medication = new MedicationViewModel(model.medicationSummary);
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
    public phoneNumber?: string;
    public faxNumber?: string;

    constructor(id: string | undefined) {
        this.id = id ? id : "";
    }

    public populateFromModel(model: Pharmacy): void {
        this.name = model.name;
        this.phoneNumber = model.phoneNumber;
        this.faxNumber = model.faxNumber;

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
    constructor(model: MedicationSummary) {
        this.din = model.din ? model.din : "";
        this.brandName = model.brandName;
        this.genericName = model.genericName;
        this.quantity = model.quantity;
        this.form = model.form;
        this.strength = model.strength;
        this.strengthUnit = model.strengthUnit;
        this.manufacturer = model.manufacturer;
        this.isPin = model.isPin;
    }
}
