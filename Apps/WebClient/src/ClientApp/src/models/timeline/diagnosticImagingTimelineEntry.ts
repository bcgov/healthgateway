import { EntryType } from "@/constants/entryType";
import type { DiagnosticImagingExam } from "@/models/patientData";
import { fromIsoDate } from "@/utils/dateUtil";

import { TimelineEntry, type UserComment } from "./timelineEntry";

/** Port of models/timeline/diagnosticImagingTimelineEntry.ts */
export class DiagnosticImagingTimelineEntry extends TimelineEntry {
    public title: string;
    public procedureDescription: string;
    public bodyPart: string;
    public modality: string;
    public organization: string;
    public healthAuthority: string;
    public examStatus: string;
    public fileId: string | undefined;
    public examDate: string;

    private readonly getComments: (entryId: string) => UserComment[] | null;

    public constructor(
        model: DiagnosticImagingExam,
        getComments: (entryId: string) => UserComment[] | null = () => null
    ) {
        super(
            model.id ?? `diagnosticImaging-${model.examDate}`,
            EntryType.DiagnosticImaging,
            fromIsoDate(model.examDate)
        );
        this.title = model.modality || "N/A";
        if (model.isUpdated) this.title = `${this.title} - Recently Updated`;
        this.procedureDescription = model.procedureDescription || "N/A";
        this.bodyPart = model.bodyPart || "N/A";
        this.modality = model.modality || "N/A";
        this.organization = model.organization || "N/A";
        this.healthAuthority = model.healthAuthority || "N/A";
        this.examStatus = model.examStatus || "N/A";
        this.fileId = model.fileId;
        this.examDate = model.examDate;
        this.getComments = getComments;
    }

    public get comments(): UserComment[] | null {
        return this.getComments(this.id);
    }

    protected containsText(keyword: string): boolean {
        return [
            this.procedureDescription,
            this.bodyPart,
            this.modality,
            this.organization,
            this.healthAuthority,
        ]
            .join(" ")
            .toUpperCase()
            .includes(keyword.toUpperCase());
    }
}
