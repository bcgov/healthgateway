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
    public showRollOffWarning: boolean;
    private getComments: (entyId: string) => UserComment[] | null;

    public constructor(
        model: Encounter,
        getComments: (entyId: string) => UserComment[] | null
    ) {
        super(
            model.id,
            EntryType.HealthVisit,
            DateWrapper.fromIsoDate(model.encounterDate)
        );
        this.practitionerName =
            model.practitionerName || "Unknown Practitioner";
        this.specialtyDescription = model.specialtyDescription ?? "";
        this.clinic = new ClinicViewModel(model.clinic);

        const duration = Duration.fromObject({ years: 6 });
        const warningDate = DateWrapper.today().subtract(duration);
        this.showRollOffWarning = this.date.isBeforeOrSame(warningDate);

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
}

class ClinicViewModel {
    public id: string;
    public name: string;

    constructor(model: Clinic) {
        this.id = model.clinicId ?? "";
        this.name = model.name;
    }
}
