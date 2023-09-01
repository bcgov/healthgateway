import { EntryType } from "@/constants/entryType";
import { DateWrapper, StringISODate } from "@/models/dateWrapper";
import { BcCancerScreening } from "@/models/patientDataResponse";
import TimelineEntry from "@/models/timeline/timelineEntry";
import { UserComment } from "@/models/userComment";
export default class BcCancerScreeningTimelineEntry extends TimelineEntry {
    public title: string;
    public documentType: string;
    public fileId: string;
    public resultDate: StringISODate;
    public programName: string;

    private getComments: (entryId: string) => UserComment[] | null;

    public constructor(
        model: BcCancerScreening,
        getComments: (entryId: string) => UserComment[] | null
    ) {
        super(
            model.id ?? `cancerScreening-${model.resultDateTime}`,
            EntryType.BcCancerScreening,
            new DateWrapper(model.resultDateTime, { isUtc: true })
        );

        this.title = "BC Cancer Result";
        this.documentType = "Screening results";
        this.programName = model.programName;
        this.fileId = model.fileId;
        this.resultDate = model.resultDateTime;
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
