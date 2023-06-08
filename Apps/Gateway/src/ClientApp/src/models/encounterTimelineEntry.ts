import { Duration } from "luxon";

import { EntryType } from "@/constants/entryType";
import Clinic from "@/models/clinic";
import { DateWrapper } from "@/models/dateWrapper";
import { Encounter } from "@/models/encounter";
import TimelineEntry from "@/models/timelineEntry";
import { UserComment } from "@/models/userComment";

// The encounter timeline entry model
export default class EncounterTimelineEntry extends TimelineEntry {
    public practitionerName: string;
    public specialtyDescription: string;
    public clinic: ClinicViewModel;
    private getComments: (entyId: string) => UserComment[] | null;

    public constructor(
        model: Encounter,
        getComments: (entyId: string) => UserComment[] | null
    ) {
        super(
            model.id,
            EntryType.HealthVisit,
            new DateWrapper(model.encounterDate)
        );
        this.practitionerName =
            model.practitionerName || "Unknown Practitioner";
        this.specialtyDescription = model.specialtyDescription || "";
        this.clinic = new ClinicViewModel(model.clinic);
        this.getComments = getComments;
    }

    public get comments(): UserComment[] | null {
        return this.getComments(this.id);
    }

    public containsText(keyword: string): boolean {
        let text =
            this.practitionerName +
            this.specialtyDescription +
            this.clinic.name;
        text = text.toUpperCase();
        return text.includes(keyword.toUpperCase());
    }

    public showRollOffWarning(): boolean {
        const duration = Duration.fromObject({ years: 6 });
        const warningDate = new DateWrapper().subtract(duration);
        return this.date.isBeforeOrSame(warningDate);
    }
}

class ClinicViewModel {
    public id: string;
    public name: string;

    constructor(model: Clinic) {
        this.id = model.clinicId || "";
        this.name = model.name;
    }
}
