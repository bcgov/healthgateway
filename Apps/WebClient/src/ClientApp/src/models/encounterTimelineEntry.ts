import Clinic from "@/models/clinic";
import { DateWrapper } from "@/models/dateWrapper";
import Encounter from "@/models/encounter";
import TimelineEntry, { EntryType } from "@/models/timelineEntry";
import { UserComment } from "@/models/userComment";

// The encounter timeline entry model
export default class EncounterTimelineEntry extends TimelineEntry {
    public practitionerName: string;
    public specialtyDescription: string;
    public clinic: ClinicViewModel;

    public constructor(model: Encounter, comments: UserComment[]) {
        super(
            model.id,
            EntryType.Encounter,
            new DateWrapper(model.encounterDate),
            comments
        );
        this.practitionerName =
            model.practitionerName || "Unknown Practitioner";
        this.specialtyDescription = model.specialtyDescription || "";
        this.clinic = new ClinicViewModel(model.clinic);
    }

    public containsText(keyword: string): boolean {
        let text =
            this.practitionerName +
            this.specialtyDescription +
            this.clinic.name;
        text = text.toUpperCase();
        return text.includes(keyword.toUpperCase());
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
