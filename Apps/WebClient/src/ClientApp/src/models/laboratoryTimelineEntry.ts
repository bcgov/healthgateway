import TimelineEntry, { EntryType } from "@/models/timelineEntry";
import { LaboratoryOrder, LaboratoryResult } from "@/models/laboratory";
import { DateWrapper } from "@/models/dateWrapper";

// The laboratory timeline entry model
export default class LaboratoryTimelineEntry extends TimelineEntry {
    public id: string;
    public orderingProviderIds: string | null;
    public orderingProviders: string | null;
    public reportingLab: string | null;
    public location: string | null;
    public displayDate: DateWrapper;
    public reportAvailable: boolean;

    public summaryTitle: string;
    public summaryDescription: string;
    public summaryStatus: string;

    public resultList: LaboratoryResultViewModel[];

    public constructor(model: LaboratoryOrder) {
        super(
            model.id,
            EntryType.Laboratory,
            model.labResults[0].collectedDateTime
        );

        this.id = model.id;
        this.orderingProviderIds = model.orderingProviderIds;
        this.orderingProviders = model.orderingProviders;
        this.reportingLab = model.reportingLab;
        this.location = model.location;
        this.reportAvailable = model.reportAvailable;

        this.resultList = new Array();
        model.labResults.forEach((result) => {
            this.resultList.push(new LaboratoryResultViewModel(result));
        });

        this.sortResults();

        const firstResult = this.resultList[0];
        this.displayDate = firstResult.collectedDateTime;

        this.summaryTitle = firstResult.loincName || "";
        this.summaryDescription = firstResult.testType || "";
        this.summaryStatus = firstResult.testStatus || "";
    }

    public filterApplies(filterText: string, filterTypes: string[]): boolean {
        if (!filterTypes.includes("Laboratory")) {
            return false;
        }

        let text =
            (this.summaryTitle! || "") + (this.summaryDescription! || "");
        text = text.toUpperCase();
        return text.includes(filterText.toUpperCase());
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
    public receivedDateTime: DateWrapper;
    public resultDateTime: DateWrapper;
    public loinc: string | null;
    public loincName: string | null;

    constructor(model: LaboratoryResult) {
        this.id = model.id;
        this.testType = model.testType;
        this.outOfRange = model.outOfRange ? "True" : "False";
        this.collectedDateTime = new DateWrapper(model.collectedDateTime);
        this.testStatus = model.testStatus;
        this.resultDescription = model.resultDescription;
        this.receivedDateTime = new DateWrapper(model.receivedDateTime);
        this.resultDateTime = new DateWrapper(model.resultDateTime);
        this.loinc = model.loinc;
        this.loincName = model.loincName;
    }
}
