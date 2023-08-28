// create public class DiagnosticImagingTimelineEntry which extends TimelineEntry
import { EntryType } from "@/constants/entryType";
import { DateWrapper, StringISODate } from "@/models/dateWrapper";
import { CancerScreeningExam } from "@/models/patientDataResponse";
import TimelineEntry from "@/models/timeline/timelineEntry";
import { UserComment } from "@/models/userComment";

export default class BcCancerScreeningResultTimelineEntry extends TimelineEntry {
    public title: string;
    public documentType: string;
    public fileId: string;
    public resultDate: StringISODate;
    public programName: string;

    private getComments: (entryId: string) => UserComment[] | null;

    public constructor(
        model: CancerScreeningExam,
        getComments: (entryId: string) => UserComment[] | null
    ) {
        super(
            model.id ?? `cancerScreening-${model.resultTimestamp}`,
            EntryType.CancerScreening,
            new DateWrapper(model.resultTimestamp)
        );

        this.title = "BC Cancer";
        this.documentType = "Screening results";
        this.programName = model.programName;
        this.fileId = model.fileId;
        this.resultDate = model.resultTimestamp;
        this.getComments = getComments;
    }

    public get comments(): UserComment[] | null {
        return this.getComments(this.id);
    }

    protected containsText(keyword: string): boolean {
        const fields = [this.title, this.documentType];
        const text = fields.join(" ").toUpperCase();
        return text.includes(keyword.toUpperCase());
    }
}
