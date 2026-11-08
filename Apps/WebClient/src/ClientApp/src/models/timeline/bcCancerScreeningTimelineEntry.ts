import { EntryType } from "@/constants/entryType";
import { BcCancerDestination, isBcCancerProgram } from "@/constants/tracking";
import {
    type BcCancerScreening,
    BcCancerScreeningType,
} from "@/models/patientData";
import { fromIso } from "@/utils/dateUtil";

import { TimelineEntry, type UserComment } from "./timelineEntry";

const programResultTitleMap: Record<string, string> = {
    "cervical cancer": "Cervix Screening Result Letter",
    "breast cancer": "Breast Screening Result Letter",
    "colon cancer": "Colon Screening Result Letter",
    "lung cancer": "Lung Screening Result Letter",
};

const programReminderTitleMap: Record<string, string> = {
    "cervical cancer": "Cervix Screening Reminder Letter",
    "breast cancer": "Breast Screening Reminder Letter",
    "colon cancer": "Colon Screening Reminder Letter",
    "lung cancer": "Lung Screening Reminder Letter",
};

const programDisplayNameMap: Record<string, string> = {
    "cervical cancer": "Cervix",
    "breast cancer": "Breast",
    "colon cancer": "Colon",
    "lung cancer": "Lung",
};

/** Port of models/timeline/bcCancerScreeningTimelineEntry.ts */
export class BcCancerScreeningTimelineEntry extends TimelineEntry {
    public title!: string;
    public documentType!: string;
    public fileId: string;
    public subtitle: string;
    public callToActionText!: string;
    public screeningType: BcCancerScreeningType;
    public isResult: boolean;
    public fileName!: string;
    public eventText!: string;
    public programName: string;
    public bcCancerDestination: string;

    private readonly getComments: (entryId: string) => UserComment[] | null;
    private readonly programKey: string;

    public constructor(
        model: BcCancerScreening,
        getComments: (entryId: string) => UserComment[] | null = () => null
    ) {
        const isResult = model.eventType === BcCancerScreeningType.Result;
        const dateTime = isResult ? model.resultDateTime : model.eventDateTime;
        super(
            model.id ?? `cancerScreening-${dateTime}`,
            EntryType.BcCancerScreening,
            fromIso(dateTime)
        );
        this.isResult = isResult;
        this.fileId = model.fileId;
        this.programName = model.programName?.trim() || "Unknown";
        this.programKey = this.programName
            .trim()
            .toLowerCase()
            .replaceAll(/\s+/g, " ");
        const program =
            programDisplayNameMap[this.programKey] ?? this.programName;
        this.subtitle = `Program: ${program} Screening`;
        this.screeningType = model.eventType;
        this.setEntryProperties();
        this.getComments = getComments;
        this.bcCancerDestination = isBcCancerProgram(program)
            ? BcCancerDestination[program]
            : "";
    }

    private setEntryProperties(): void {
        if (this.screeningType === BcCancerScreeningType.Result) {
            this.title =
                programResultTitleMap[this.programKey] ??
                `${this.programName} Screening Result Letter`;
            this.callToActionText = "View Letter";
            this.documentType = "Screening results";
            this.fileName = "bc_cancer_result";
            this.eventText = "BC Cancer Result PDF";
        } else {
            this.title =
                programReminderTitleMap[this.programKey] ??
                `${this.programName} Screening Reminder Letter`;
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
        return [this.title, this.documentType]
            .join(" ")
            .toUpperCase()
            .includes(keyword.toUpperCase());
    }
}
