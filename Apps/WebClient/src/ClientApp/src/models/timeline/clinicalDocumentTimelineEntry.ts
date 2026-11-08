import { EntryType } from "@/constants/entryType";
import type { ClinicalDocument } from "@/models/clinicalDocument";
import { fromIsoDate } from "@/utils/dateUtil";

import { TimelineEntry, type UserComment } from "./timelineEntry";

/** Port of models/timeline/clinicalDocumentTimelineEntry.ts */
export class ClinicalDocumentTimelineEntry extends TimelineEntry {
    public fileId: string;
    public name: string;
    public documentType: string;
    public facilityName: string;
    public discipline: string;

    private readonly getComments: (entryId: string) => UserComment[] | null;

    public constructor(
        model: ClinicalDocument,
        getComments: (entryId: string) => UserComment[] | null = () => null
    ) {
        super(
            model.id,
            EntryType.ClinicalDocument,
            fromIsoDate(model.serviceDate)
        );
        this.fileId = model.fileId;
        this.name = model.name;
        this.documentType = model.type;
        this.facilityName = model.facilityName;
        this.discipline = model.discipline;
        this.getComments = getComments;
    }

    public get comments(): UserComment[] | null {
        return this.getComments(this.id);
    }

    public containsText(keyword: string): boolean {
        return [
            this.name,
            this.documentType,
            this.discipline,
            this.facilityName,
        ]
            .join(" ")
            .toUpperCase()
            .includes(keyword.toUpperCase());
    }
}
