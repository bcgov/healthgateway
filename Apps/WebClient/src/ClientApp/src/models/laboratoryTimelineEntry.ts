import TimelineEntry, { EntryType } from "@/models/timelineEntry";
import { LaboratoryOrder, LaboratoryResult } from "@/models/laboratory";
import { DateWrapper } from "@/models/dateWrapper";
import TimelineFilter from "@/models/timelineFilter";

// The laboratory timeline entry model
export default class LaboratoryTimelineEntry extends TimelineEntry {
    public orderingProviderIds: string | null;
    public orderingProviders: string | null;
    public reportingLab: string | null;
    public location: string | null;
    public labResultOutcome: string | null;
    public displayDate: DateWrapper;
    public reportAvailable: boolean;
    public isStatusFinal: boolean;

    public summaryTitle: string;
    public summaryDescription: string;
    public summaryStatus: string;

    public resultList: LaboratoryResultViewModel[];

    public constructor(model: LaboratoryOrder) {
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
        this.isStatusFinal = this.summaryStatus == "Final";
        this.labResultOutcome = firstResult.labResultOutcome;
        console.log(this.isStatusFinal);
    }

    public keywordApplies(filter: TimelineFilter): boolean {
        let text = this.summaryTitle + this.summaryDescription;
        text = text.toUpperCase();
        return text.includes(filter.keyword.toUpperCase());
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
    public resultDescription: string | null;
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
        this.resultDescription = model.resultDescription;
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
