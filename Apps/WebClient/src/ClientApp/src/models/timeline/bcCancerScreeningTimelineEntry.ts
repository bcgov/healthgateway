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
    public programName!: string;

    private readonly getComments: (entryId: string) => UserComment[] | null;
    private readonly normalizedProgramName: string;

    private static readonly programResultTitleMap: Record<string, string> = {
        "cervical cancer": "BC Cervical Cancer Screening Result Letter",
        "breast cancer": "BC Breast Cancer Screening Result Letter",
        "colon cancer": "BC Colon Cancer Screening Result Letter",
        "lung cancer": "BC Lung Cancer Screening Result Letter",
    };

    private static readonly programReminderTitleMap: Record<string, string> = {
        "cervical cancer": "BC Cervical Cancer Screening Reminder Letter",
        "breast cancer": "BC Breast Cancer Screening Reminder Letter",
        "colon cancer": "BC Colon Cancer Screening Reminder Letter",
        "lung cancer": "BC Lung Cancer Screening Reminder Letter",
    };

    public constructor(
        model: BcCancerScreening,
        getComments: (entryId: string) => UserComment[] | null
    ) {
        const programDisplayNameMap: Record<string, string> = {
            "cervical cancer": "Cervix",
            "breast cancer": "Breast",
            "colon cancer": "Colon",
            "lung cancer": "Lung",
        };

        const isResult = model.eventType === BcCancerScreeningType.Result;
        const dateTime = isResult ? model.resultDateTime : model.eventDateTime;
        super(
            model.id ?? `cancerScreening-${dateTime}`,
            EntryType.BcCancerScreening,
            DateWrapper.fromIso(dateTime)
        );
        this.isResult = isResult;
        this.fileId = model.fileId;
        this.programName = model.programName?.trim() || "Unknown";
        // Normalize incoming value: trim, lowercase, single space
        this.normalizedProgramName = this.programName
            .trim()
            .toLowerCase()
            .replace(/\s+/g, " ");
        const program =
            programDisplayNameMap[this.normalizedProgramName] ??
            this.programName;
        this.subtitle = `Program: ${program} Screening`;
        this.screeningType = model.eventType;
        this.setEntryProperties();
        this.getComments = getComments;
    }

    private setEntryProperties(): void {
        if (this.screeningType === BcCancerScreeningType.Result) {
            this.title =
                BcCancerScreeningTimelineEntry.programResultTitleMap[
                    this.normalizedProgramName
                ] ?? this.programName;
            this.callToActionText = "View Letter";
            this.documentType = "Screening results";
            this.fileName = "bc_cancer_result";
            this.eventText = "BC Cancer Result PDF";
        } else {
            this.title =
                BcCancerScreeningTimelineEntry.programReminderTitleMap[
                    this.normalizedProgramName
                ] ?? this.programName;
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
