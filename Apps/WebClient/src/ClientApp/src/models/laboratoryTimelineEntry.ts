import { DateWrapper } from "@/models/dateWrapper";
import {
    LaboratoryOrder,
    LaboratoryResult,
    LaboratoryUtil,
} from "@/models/laboratory";
import TimelineEntry, { EntryType } from "@/models/timelineEntry";
import { UserComment } from "@/models/userComment";

// The laboratory timeline entry model
export default class LaboratoryTimelineEntry extends TimelineEntry {
    public orderingProviderIds: string | null;
    public orderingProviders: string | null;
    public reportingLab: string | null;
    public location: string | null;
    public labResultOutcome: string | null;
    public displayDate: DateWrapper;
    public reportAvailable: boolean;
    public isTestResultReady: boolean;

    public summaryTitle: string;
    public summaryDescription: string;
    public summaryStatus: string;

    public resultList: LaboratoryResultViewModel[];

    private getComments: (entyId: string) => UserComment[] | null;

    public constructor(
        model: LaboratoryOrder,
        getComments: (entyId: string) => UserComment[] | null
    ) {
        super(
            model.id,
            EntryType.Laboratory,
            new DateWrapper(model.labResults[0].collectedDateTime, {
                hasTime: true,
            })
        );

        this.orderingProviderIds = model.orderingProviderIds;
        this.orderingProviders = model.orderingProviders;
        this.reportingLab = model.reportingLab;
        this.location = model.location;
        this.reportAvailable = model.reportAvailable;

        this.resultList = [];
        model.labResults.forEach((result) => {
            this.resultList.push(new LaboratoryResultViewModel(result));
        });

        this.sortResults();

        const firstResult = this.resultList[0];
        this.displayDate = firstResult.collectedDateTime;

        this.summaryTitle = firstResult.loincName || "";
        this.summaryDescription = firstResult.testType || "";
        this.summaryStatus = firstResult.testStatus || "";
        this.isTestResultReady = LaboratoryUtil.isTestResultReady(
            this.summaryStatus
        );
        this.labResultOutcome = firstResult.labResultOutcome;

        this.getComments = getComments;
    }

    public get comments(): UserComment[] | null {
        return this.getComments(this.id);
    }

    public containsText(keyword: string): boolean {
        let text = this.summaryTitle + this.summaryDescription;
        text = text.toUpperCase();
        return text.includes(keyword.toUpperCase());
    }

    private sortResults() {
        this.resultList.sort((a, b) =>
            a.collectedDateTime > b.collectedDateTime
                ? -1
                : a.collectedDateTime < b.collectedDateTime
                ? 1
                : 0
        );
    }
}

export class LaboratoryResultViewModel {
    public id: string;
    public testType: string | null;
    public outOfRange: string;
    public collectedDateTime: DateWrapper;
    public testStatus: string | null;
    public isTestResultReady: boolean;
    public resultDescription: string[];
    public resultLink: string | null;
    public labResultOutcome: string | null;
    public receivedDateTime: DateWrapper;
    public resultDateTime: DateWrapper;
    public loinc: string | null;
    public loincName: string | null;

    constructor(model: LaboratoryResult) {
        this.id = model.id;
        this.testType = model.testType;
        this.outOfRange = model.outOfRange ? "True" : "False";
        this.collectedDateTime = new DateWrapper(model.collectedDateTime, {
            hasTime: true,
        });
        this.testStatus = model.testStatus;
        this.isTestResultReady = LaboratoryUtil.isTestResultReady(
            this.testStatus
        );
        this.resultDescription = model.resultDescription;
        this.resultLink = model.resultLink;
        this.labResultOutcome = model.labResultOutcome;
        this.receivedDateTime = new DateWrapper(model.receivedDateTime, {
            hasTime: true,
        });
        this.resultDateTime = new DateWrapper(model.resultDateTime, {
            hasTime: true,
        });
        this.loinc = model.loinc;
        this.loincName = model.loincName;
    }
}
