import { EntryType } from "@/constants/entryType";
import { DateWrapper } from "@/models/dateWrapper";
import {
    Covid19LaboratoryOrder,
    Covid19LaboratoryTest,
} from "@/models/laboratory";
import TimelineEntry from "@/models/timeline/timelineEntry";
import { UserComment } from "@/models/userComment";
import DateSortUtility from "@/utility/dateSortUtility";

// The COVID-19 test result timeline entry model
export default class Covid19TestResultTimelineEntry extends TimelineEntry {
    public orderingProviderIds: string | null;
    public orderingProviders: string | null;
    public reportingLab: string | null;
    public location: string | null;
    public labResultOutcome: string;
    public displayDate: DateWrapper;
    public reportAvailable: boolean;
    public resultReady: boolean;

    public summaryTitle: string;
    public summaryDescription: string;
    public summaryStatus: string;

    public tests: Covid19TestData[];

    private readonly getComments: (entyId: string) => UserComment[] | null;

    public constructor(
        model: Covid19LaboratoryOrder,
        getComments: (entyId: string) => UserComment[] | null
    ) {
        super(
            model.id,
            EntryType.Covid19TestResult,
            DateWrapper.fromIso(model.labResults[0].collectedDateTime)
        );

        this.orderingProviderIds = model.orderingProviderIds;
        this.orderingProviders = model.orderingProviders;
        this.reportingLab = model.reportingLab;
        this.location = model.location;
        this.reportAvailable = model.reportAvailable;

        this.tests = [];
        model.labResults.forEach((test) =>
            this.tests.push(new Covid19TestData(test))
        );

        this.sortResults();

        const firstResult = this.tests[0];
        this.displayDate = firstResult.collectedDateTime;

        this.summaryTitle = firstResult.loincName ?? "";
        this.summaryDescription = firstResult.testType ?? "";
        this.summaryStatus = firstResult.testStatus ?? "";
        this.resultReady = firstResult.resultReady;
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

    private sortResults(): void {
        this.tests.sort((a, b) =>
            DateSortUtility.descending(a.collectedDateTime, b.collectedDateTime)
        );
    }
}

export class Covid19TestData {
    public id: string;
    public testType: string;
    public outOfRange: string;
    public collectedDateTime: DateWrapper;
    public testStatus: string;
    public resultReady: boolean;
    public resultDescription: string[];
    public resultLink: string;
    public labResultOutcome: string;
    public filteredLabResultOutcome: string;
    public receivedDateTime: DateWrapper;
    public resultDateTime: DateWrapper;
    public loinc: string;
    public loincName: string;

    constructor(model: Covid19LaboratoryTest) {
        this.id = model.id;
        this.testType = model.testType;
        this.outOfRange = model.outOfRange ? "True" : "False";
        this.collectedDateTime = DateWrapper.fromIso(model.collectedDateTime);
        this.testStatus = model.testStatus;
        this.resultReady = model.resultReady;
        this.resultDescription = model.resultDescription;
        this.resultLink = model.resultLink;
        this.labResultOutcome = model.labResultOutcome;
        this.filteredLabResultOutcome = model.filteredLabResultOutcome;
        this.receivedDateTime = DateWrapper.fromIso(model.receivedDateTime);
        this.resultDateTime = DateWrapper.fromIso(model.resultDateTime);
        this.loinc = model.loinc;
        this.loincName = model.loincName;
    }
}
