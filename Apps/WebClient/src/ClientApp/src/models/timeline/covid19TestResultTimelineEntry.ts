import type { DateTime } from "luxon";

import { EntryType } from "@/constants/entryType";
import type {
    Covid19LaboratoryOrder,
    Covid19LaboratoryTest,
} from "@/models/laboratory";
import { descending, fromIso } from "@/utils/dateUtil";

import { TimelineEntry, type UserComment } from "./timelineEntry";

export class Covid19TestData {
    public id: string;
    public testType: string;
    public outOfRange: string;
    public collectedDateTime: DateTime;
    public testStatus: string;
    public resultReady: boolean;
    public resultDescription: string[];
    public resultLink: string;
    public labResultOutcome: string;
    public filteredLabResultOutcome: string;
    public receivedDateTime: DateTime;
    public resultDateTime: DateTime;
    public loinc: string;
    public loincName: string;

    constructor(model: Covid19LaboratoryTest) {
        this.id = model.id;
        this.testType = model.testType;
        this.outOfRange = model.outOfRange ? "True" : "False";
        this.collectedDateTime = fromIso(model.collectedDateTime);
        this.testStatus = model.testStatus;
        this.resultReady = model.resultReady;
        this.resultDescription = model.resultDescription;
        this.resultLink = model.resultLink;
        this.labResultOutcome = model.labResultOutcome;
        this.filteredLabResultOutcome = model.filteredLabResultOutcome;
        this.receivedDateTime = fromIso(model.receivedDateTime);
        this.resultDateTime = fromIso(model.resultDateTime);
        this.loinc = model.loinc;
        this.loincName = model.loincName;
    }
}

/** Port of models/timeline/covid19TestResultTimelineEntry.ts */
export class Covid19TestResultTimelineEntry extends TimelineEntry {
    public orderingProviderIds: string | null;
    public orderingProviders: string | null;
    public reportingLab: string | null;
    public location: string | null;
    public labResultOutcome: string;
    public displayDate: DateTime;
    public reportAvailable: boolean;
    public resultReady: boolean;
    public summaryTitle: string;
    public summaryDescription: string;
    public summaryStatus: string;
    public tests: Covid19TestData[];

    private readonly getComments: (entryId: string) => UserComment[] | null;

    public constructor(
        model: Covid19LaboratoryOrder,
        getComments: (entryId: string) => UserComment[] | null = () => null
    ) {
        super(
            model.id,
            EntryType.Covid19TestResult,
            fromIso(model.labResults[0].collectedDateTime)
        );
        this.orderingProviderIds = model.orderingProviderIds;
        this.orderingProviders = model.orderingProviders;
        this.reportingLab = model.reportingLab;
        this.location = model.location;
        this.reportAvailable = model.reportAvailable;
        this.tests = model.labResults.map((test) => new Covid19TestData(test));
        this.tests.sort((a, b) =>
            descending(a.collectedDateTime, b.collectedDateTime)
        );
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
        return (this.summaryTitle + this.summaryDescription)
            .toUpperCase()
            .includes(keyword.toUpperCase());
    }
}
