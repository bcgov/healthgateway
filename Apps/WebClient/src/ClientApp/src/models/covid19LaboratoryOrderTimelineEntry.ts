import { EntryType } from "@/constants/entryType";
import { DateWrapper } from "@/models/dateWrapper";
import {
    Covid19LaboratoryOrder,
    Covid19LaboratoryTest,
    LaboratoryUtil,
} from "@/models/laboratory";
import TimelineEntry from "@/models/timelineEntry";
import { UserComment } from "@/models/userComment";

// The COVID-19 laboratory order timeline entry model
export default class Covid19LaboratoryOrderTimelineEntry extends TimelineEntry {
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

    public tests: Covid19LaboratoryTestViewModel[];

    private getComments: (entyId: string) => UserComment[] | null;

    public constructor(
        model: Covid19LaboratoryOrder,
        getComments: (entyId: string) => UserComment[] | null
    ) {
        super(
            model.id,
            EntryType.Covid19LaboratoryOrder,
            new DateWrapper(model.labResults[0].collectedDateTime, {
                hasTime: true,
            })
        );

        this.orderingProviderIds = model.orderingProviderIds;
        this.orderingProviders = model.orderingProviders;
        this.reportingLab = model.reportingLab;
        this.location = model.location;
        this.reportAvailable = model.reportAvailable;

        this.tests = [];
        model.labResults.forEach((test) => {
            this.tests.push(new Covid19LaboratoryTestViewModel(test));
        });

        this.sortResults();

        const firstResult = this.tests[0];
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
        this.tests.sort((a, b) =>
            a.collectedDateTime > b.collectedDateTime
                ? -1
                : a.collectedDateTime < b.collectedDateTime
                ? 1
                : 0
        );
    }
}

export class Covid19LaboratoryTestViewModel {
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

    constructor(model: Covid19LaboratoryTest) {
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
