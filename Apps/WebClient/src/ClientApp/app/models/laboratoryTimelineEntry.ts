import TimelineEntry, { EntryType } from "@/models/timelineEntry";
import { LaboratoryReport, LaboratoryResult } from "./laboratory";

// The laboratory timeline entry model
export default class LaboratoryTimelineEntry extends TimelineEntry {
  public id: string;
  public orderingProviderIds: string | null;
  public orderingProviders: string | null;
  public reportingLab: string | null;
  public location: string | null;
  public displayDate: Date;

  public summaryTestType: string;
  public summaryDescription: string;
  public summaryStatus: string;

  public resultList: LaboratoryResultViewModel[];

  public constructor(model: LaboratoryReport) {
    super(
      model.id,
      EntryType.Laboratory,
      model.labResults[0].collectionDateTime
    );

    this.id = model.id;
    this.orderingProviderIds = model.orderingProviderIds;
    this.orderingProviders = model.orderingProviders;
    this.reportingLab = model.reportingLab;
    this.location = model.location;

    this.resultList = new Array();
    model.labResults.forEach(result => {
      this.resultList.push(new LaboratoryResultViewModel(result));
    });

    this.sortResults();

    let firstResult = this.resultList[0];
    this.displayDate = firstResult.collectionDateTime;

    this.summaryTestType = firstResult.testType || "";
    this.summaryDescription = firstResult.loincName || "";
    this.summaryStatus = firstResult.testStatus || "";
  }

  public filterApplies(filterText: string, filterTypes: string[]): boolean {
    if (!filterTypes.includes("Laboratory")) {
      return false;
    }

    var text = (this.summaryTestType! || "") + (this.summaryDescription! || "");
    text = text.toUpperCase();
    return text.includes(filterText.toUpperCase());
  }

  private sortResults() {
    this.resultList.sort((a, b) =>
      a.receivedDateTime > b.receivedDateTime
        ? -1
        : a.receivedDateTime < b.receivedDateTime
        ? 1
        : 0
    );
  }
}

export class LaboratoryResultViewModel {
  public id: string;
  public testType: string | null;
  public outOfRange: boolean;
  public collectionDateTime: Date;
  public testStatus: string | null;
  public resultDescription: string | null;
  public receivedDateTime: Date;
  public resultDateTime: Date;
  public loinc: string | null;
  public loincName: string | null;

  constructor(model: LaboratoryResult) {
    this.id = model.id;
    this.testType = model.testType;
    this.outOfRange = model.outOfRange;
    this.collectionDateTime = model.collectionDateTime;
    this.testStatus = model.testStatus;
    this.resultDescription = model.resultDescription;
    this.receivedDateTime = model.receivedDateTime;
    this.resultDateTime = model.resultDateTime;
    this.loinc = model.loinc;
    this.loincName = model.loincName;
  }
}
