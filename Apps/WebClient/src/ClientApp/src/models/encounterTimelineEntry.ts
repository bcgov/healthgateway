import TimelineEntry, { EntryType } from "@/models/timelineEntry";
import Encounter from "@/models/encounter";
import Clinic from "@/models/clinic";

// The encounter timeline entry model
export default class EncounterTimelineEntry extends TimelineEntry {
    public practitionerName: string;
    public specialtyDescription: string;
    public clinic: ClinicViewModel;

    public constructor(model: Encounter) {
        super(model.id, EntryType.Encounter, model.encounterDate);
        this.practitionerName =
            model.practitionerName || "Unknown Practitioner";
        this.specialtyDescription = model.specialtyDescription || "";
        this.clinic = new ClinicViewModel(model.clinic);
    }

    public filterApplies(filterText: string, filterTypes: string[]): boolean {
        if (!filterTypes.includes("Encounter")) {
            return false;
        }

        let text =
            this.practitionerName +
            this.specialtyDescription +
            this.clinic.name;
        text = text.toUpperCase();
        return text.includes(filterText.toUpperCase());
    }
}

class ClinicViewModel {
    public id: string;
    public name: string;
    public address: string;
    public phoneNumber: string;

    constructor(model: Clinic) {
        this.id = model.clinicId || "";
        this.name = model.name;
        this.phoneNumber = model.phoneNumber || "";

        const addressArray = [
            model.addressLine1,
            model.addressLine2,
            model.addressLine3,
            model.addressLine4,
        ];
        this.address =
            addressArray.filter((val) => val.length > 0).join(" ") +
            ", " +
            (model.city || "") +
            " " +
            (model.province || "") +
            ", " +
            (model.postalCode || "");
    }
}
