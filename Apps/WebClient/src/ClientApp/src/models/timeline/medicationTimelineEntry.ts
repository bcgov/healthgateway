import { EntryType } from "@/constants/entryType";
import type {
    MedicationActiveIngredient,
    MedicationStatement,
    MedicationSummary,
    Pharmacy,
} from "@/models/medication";
import { fromIsoDate } from "@/utils/dateUtil";

import { TimelineEntry, type UserComment } from "./timelineEntry";

class PharmacyViewModel {
    public id: string;
    public name?: string;
    public address?: string;
    public phoneNumber?: string;
    public faxNumber?: string;

    constructor(id: string | undefined) {
        this.id = id ?? "";
    }

    public populateFromModel(model: Pharmacy): void {
        this.name = model.name;
        this.phoneNumber = model.phoneNumber;
        this.faxNumber = model.faxNumber;
        // string concatenation as the web client does (an absent part reads "undefined")
        this.address =
            String(model.addressLine1) +
            " " +
            String(model.addressLine2) +
            ", " +
            String(model.city) +
            " " +
            String(model.province);
    }
}

class MedicationViewModel {
    public din: string;
    public brandName: string;
    public genericName?: string;
    public quantity?: number;
    public form?: string;
    public manufacturer?: string;
    public isPin: boolean;
    public title: string;
    public subtitle: string;
    public isPharmacistAssessment: boolean;
    public activeIngredients: MedicationActiveIngredient[];

    constructor(model: MedicationSummary) {
        this.din = model.din ?? "";
        this.brandName = model.brandName;
        this.genericName = model.genericName;
        this.quantity = model.quantity;
        this.form = model.form;
        this.manufacturer = model.manufacturer;
        this.isPin = model.isPin;
        this.title = model.title;
        this.subtitle = model.subtitle;
        this.isPharmacistAssessment = model.isPharmacistAssessment;
        this.activeIngredients = model.activeIngredients ?? [];
    }
}

/** Port of models/timeline/medicationTimelineEntry.ts */
export class MedicationTimelineEntry extends TimelineEntry {
    public pharmacy: PharmacyViewModel;
    public medication: MedicationViewModel;
    public title: string;
    public subtitle?: string;
    public directions: string;
    public practitionerSurname: string;
    public prescriptionIdentifier: string;
    public isPharmacistAssessment = false;
    public prescriptionProvided?: boolean;
    public redirectedToHealthCareProvider?: boolean;

    private readonly getComments: (entryId: string) => UserComment[] | null;

    public constructor(
        model: MedicationStatement,
        getComments: (entryId: string) => UserComment[] | null = () => null
    ) {
        super(
            model.prescriptionIdentifier,
            EntryType.Medication,
            fromIsoDate(model.dispensedDate)
        );
        const summary = model.medicationSummary;
        this.medication = new MedicationViewModel(summary);
        this.title = this.medication.title;
        this.subtitle = this.medication.subtitle;
        this.pharmacy = new PharmacyViewModel(
            model.dispensingPharmacy?.pharmacyId
        );
        if (summary.isPharmacistAssessment) {
            this.isPharmacistAssessment = true;
            this.prescriptionProvided = summary.prescriptionProvided;
            this.redirectedToHealthCareProvider =
                summary.redirectedToHealthCareProvider;
        }
        if (model.dispensingPharmacy)
            this.pharmacy.populateFromModel(model.dispensingPharmacy);
        this.practitionerSurname = model.practitionerSurname || "N/A";
        this.prescriptionIdentifier = model.prescriptionIdentifier || "N/A";
        this.directions = model.directions || "N/A";
        this.getComments = getComments;
    }

    public get comments(): UserComment[] | null {
        return this.getComments(this.id);
    }

    public containsText(keyword: string): boolean {
        const text = (
            (this.practitionerSurname ?? "") +
            (this.title ?? "") +
            (this.subtitle ?? "")
        ).toUpperCase();
        return text.includes(keyword.toUpperCase());
    }
}
