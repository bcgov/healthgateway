import TimelineEntry, { EntryType } from "@/models/timelineEntry";
import Encounter from "@/models/encounter";
import Clinic from "@/models/clinic";

// The encounter timeline entry model
export default class EncounterTimelineEntry extends TimelineEntry {
    public practitionerName: string;
    public specialtyDescription: string;
    public clinic: Clinic;

    public constructor(model?: Encounter) {
        super(
            model?.id ?? "",
            EntryType.Encounter,
            model?.serviceDateTime ?? new Date()
        );
        this.practitionerName =
            model?.practitionerName || "Unknown Practitioner";
        this.specialtyDescription = model?.specialtyDescription || "";
        this.clinic = model?.clinic;
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
