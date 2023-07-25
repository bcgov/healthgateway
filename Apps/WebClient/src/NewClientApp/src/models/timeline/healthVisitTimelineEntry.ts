import { Duration } from "luxon";

import { EntryType } from "@/constants/entryType";
import { DateWrapper } from "@/models/dateWrapper";
import { Clinic, Encounter } from "@/models/encounter";
import TimelineEntry from "@/models/timeline/timelineEntry";
import { UserComment } from "@/models/userComment";

// The health visit timeline entry model
export default class HealthVisitTimelineEntry extends TimelineEntry {
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
