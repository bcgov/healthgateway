import type { DateTime } from "luxon";

import { EntryType } from "@/constants/entryType";
import type { HospitalVisit } from "@/models/encounter";
import { fromIso } from "@/utils/dateUtil";

import { TimelineEntry } from "./timelineEntry";

/** Port of models/timeline/hospitalVisitTimelineEntry.ts */
export class HospitalVisitTimelineEntry extends TimelineEntry {
    public healthService: string;
    public visitType: string;
    public outpatient: boolean;
    public facility: string;
    public provider: string;
    public healthAuthority?: string;
    public admitDateTime: DateTime;
    public endDateTime?: DateTime;

    public constructor(model: HospitalVisit) {
        super(
            model.encounterId,
            EntryType.HospitalVisit,
            fromIso(model.admitDateTime)
        );
        this.healthService = model.healthService;
        this.visitType = model.visitType;
        this.outpatient = model.visitType === "Outpatient Visit";
        this.facility = model.facility;
        this.healthAuthority = model.healthAuthority;
        this.admitDateTime = fromIso(model.admitDateTime);
        this.endDateTime = model.endDateTime
            ? fromIso(model.endDateTime)
            : undefined;
        this.provider = model.provider;
    }

    protected containsText(keyword: string): boolean {
        return [
            this.facility,
            this.healthService,
            this.visitType,
            this.healthAuthority,
            this.provider,
        ]
            .join(" ")
            .toUpperCase()
            .includes(keyword.toUpperCase());
    }
}
