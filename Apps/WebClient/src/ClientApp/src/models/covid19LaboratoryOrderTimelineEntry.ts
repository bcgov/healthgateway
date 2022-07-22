import { EntryType } from "@/constants/entryType";
import { DateWrapper } from "@/models/dateWrapper";
import {
    Covid19LaboratoryOrder,
    Covid19LaboratoryTest,
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
    public resultReady: boolean;

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
        model.labResults.forEach((test) =>
            this.tests.push(new Covid19LaboratoryTestViewModel(test))
        );

        this.sortResults();

        const firstResult = this.tests[0];
        this.displayDate = firstResult.collectedDateTime;

        this.summaryTitle = firstResult.loincName || "";
        this.summaryDescription = firstResult.testType || "";
        this.summaryStatus = firstResult.testStatus || "";
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
        this.tests.sort((a, b) => {
            if (a.collectedDateTime.isBefore(b.collectedDateTime)) {
                return 1;
            }
            if (a.collectedDateTime.isAfter(b.collectedDateTime)) {
                return -1;
            }
            return 0;
        });
    }
}

export class Covid19LaboratoryTestViewModel {
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
        this.collectedDateTime = new DateWrapper(model.collectedDateTime, {
            hasTime: true,
        });
        this.testStatus = model.testStatus;
        this.resultReady = model.resultReady;
        this.resultDescription = model.resultDescription;
        this.resultLink = model.resultLink;
        this.labResultOutcome = model.labResultOutcome;
        this.filteredLabResultOutcome = model.filteredLabResultOutcome;
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
