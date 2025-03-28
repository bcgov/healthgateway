import { EntryType } from "@/constants/entryType";
import { DateWrapper } from "@/models/dateWrapper";
import {
    BcCancerScreening,
    BcCancerScreeningType,
} from "@/models/patientDataResponse";
import TimelineEntry from "@/models/timeline/timelineEntry";
import { UserComment } from "@/models/userComment";
export default class BcCancerScreeningTimelineEntry extends TimelineEntry {
    public title!: string;
    public documentType!: string;
    public fileId: string;
    public subtitle: string;
    public callToActionText!: string;
    public screeningType: BcCancerScreeningType;
    public isResult!: boolean;
    public fileName!: string;
    public eventText!: string;

    private readonly getComments: (entryId: string) => UserComment[] | null;

    public constructor(
        model: BcCancerScreening,
        getComments: (entryId: string) => UserComment[] | null
    ) {
        const isResult = model.eventType === BcCancerScreeningType.Result;
        const dateTime = isResult ? model.resultDateTime : model.eventDateTime;
        super(
            model.id ?? `cancerScreening-${dateTime}`,
            EntryType.BcCancerScreening,
            DateWrapper.fromIso(dateTime)
        );
        this.isResult = isResult;
        this.fileId = model.fileId;
        this.subtitle = `Program: Cervix Screening`;
        this.screeningType = model.eventType;
        this.setEntryProperties();
        this.getComments = getComments;
    }

    private setEntryProperties(): void {
        if (this.screeningType === BcCancerScreeningType.Result) {
            this.title = "BC Cancer Screening Result Letter";
            this.callToActionText = "View Letter";
            this.documentType = "Screening results";
            this.fileName = "bc_cancer_result";
            this.eventText = "BC Cancer Result PDF";
        } else {
            this.title = "BC Cancer Screening Reminder Letter";
            this.callToActionText = "View Letter";
            this.documentType = "Screening letter";
            this.fileName = "bc_cancer_screening";
            this.eventText = "BC Cancer Screening PDF";
        }
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
