import { EntryType } from "@/constants/entryType";
import type { ClinicalDocument } from "@/models/clinicalDocument";
import { fromIsoDate } from "@/utils/dateUtil";

import { TimelineEntry } from "./timelineEntry";

/** Port of models/timeline/clinicalDocumentTimelineEntry.ts */
export class ClinicalDocumentTimelineEntry extends TimelineEntry {
    public fileId: string;
    public name: string;
    public documentType: string;
    public facilityName: string;
    public discipline: string;

    public constructor(model: ClinicalDocument) {
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
    }

    protected containsText(keyword: string): boolean {
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
