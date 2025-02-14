// create public class DiagnosticImagingTimelineEntry which extends TimelineEntry
import { EntryType } from "@/constants/entryType";
import { DateWrapper, StringISODate } from "@/models/dateWrapper";
import { DiagnosticImagingExam } from "@/models/patientDataResponse";
import TimelineEntry from "@/models/timeline/timelineEntry";
import { UserComment } from "@/models/userComment";

export default class DiagnosticImagingTimelineEntry extends TimelineEntry {
    public title: string;
    public procedureDescription: string;
    public bodyPart: string;
    public modality: string;
    public organization: string;
    public healthAuthority: string;
    public examStatus: string;
    public fileId: string | undefined;
    public examDate: StringISODate;

    private readonly getComments: (entryId: string) => UserComment[] | null;

    public constructor(
        model: DiagnosticImagingExam,
        getComments: (entryId: string) => UserComment[] | null
    ) {
        super(
            model.id ?? `diagnosticImaging-${model.examDate}`,
            EntryType.DiagnosticImaging,
            DateWrapper.fromIsoDate(model.examDate)
        );

        this.title = model.modality || "N/A";
        if (model.isUpdated) {
            this.title = `${this.title} - Recently Updated`;
        }

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
        const fields = [
            this.procedureDescription,
            this.bodyPart,
            this.modality,
            this.organization,
            this.healthAuthority,
        ];
        const text = fields.join(" ").toUpperCase();
        return text.includes(keyword.toUpperCase());
    }
}
