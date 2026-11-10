import { EntryType } from "@/constants/entryType";
import type { Clinic, Encounter } from "@/models/encounter";
import { fromIsoDate, today } from "@/utils/dateUtil";

import { TimelineEntry, type UserComment } from "./timelineEntry";

class ClinicViewModel {
    public id: string;
    public name: string;

    constructor(model: Clinic) {
        this.id = model.clinicId ?? "";
        this.name = model.name;
    }
}

/** Port of models/timeline/healthVisitTimelineEntry.ts */
export class HealthVisitTimelineEntry extends TimelineEntry {
    public practitionerName: string;
    public specialtyDescription: string;
    public clinic: ClinicViewModel;
    /** visits 6 or more years old roll off at the next calendar year */
    public showRollOffWarning: boolean;

    private readonly getComments: (entryId: string) => UserComment[] | null;

    public constructor(
        model: Encounter,
        getComments: (entryId: string) => UserComment[] | null = () => null
    ) {
        super(
            model.id,
            EntryType.HealthVisit,
            fromIsoDate(model.encounterDate)
        );
        this.practitionerName =
            model.practitionerName || "Unknown Practitioner";
        this.specialtyDescription = model.specialtyDescription ?? "";
        this.clinic = new ClinicViewModel(model.clinic);
        this.showRollOffWarning = this.date <= today().minus({ years: 6 });
        this.getComments = getComments;
    }

    public get comments(): UserComment[] | null {
        return this.getComments(this.id);
    }

    public containsText(keyword: string): boolean {
        return (
            this.practitionerName +
            this.specialtyDescription +
            this.clinic.name
        )
            .toUpperCase()
            .includes(keyword.toUpperCase());
    }
}
