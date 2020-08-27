import TimelineEntry, { EntryType } from "@/models/timelineEntry";
import Encounter from "@/models/encounter";
import Clinic from "@/models/clinic";

// The encounter timeline entry model
export default class EncounterTimelineEntry extends TimelineEntry {
    public practitionerName: string;
    public specialtyDescription: string;
    public clinic: ClinicViewModel;

    public constructor(model?: Encounter) {
        super(
            model?.id ?? "",
            EntryType.Encounter,
            model?.serviceDateTime ?? new Date()
        );
        this.practitionerName =
            model?.practitionerName || "Unknown Practitioner";
        this.specialtyDescription = model?.specialtyDescription || "";
        this.clinic = new ClinicViewModel(model?.clinic?.clinicId);
        if (model?.clinic) {
            this.clinic.populateFromModel(model.clinic);
        }
    }

    public filterApplies(filterText: string, filterTypes: string[]): boolean {
        if (!filterTypes.includes("Encounter")) {
            return false;
        }

        let text =
            (this.practitionerName! || "") +
            (this.specialtyDescription! || "") +
            (this.clinic?.name || "");
        text = text.toUpperCase();
        return text.includes(filterText.toUpperCase());
    }
}

class ClinicViewModel {
    public id: string;
    public name?: string;
    public address?: string;
    public phoneNumber?: string;

    constructor(id: string | undefined) {
        this.id = id ? id : "";
    }

    public populateFromModel(model: Clinic): void {
        this.name = model.name;
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