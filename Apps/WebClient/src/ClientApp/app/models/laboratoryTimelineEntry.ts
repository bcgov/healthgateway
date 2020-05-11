import TimelineEntry, { EntryType } from "@/models/timelineEntry";
import { LaboratoryReport } from "./laboratory";

// The laboratory timeline entry model
export default class LaboratoryTimelineEntry extends TimelineEntry {
  public id: string;
  public phn: string | null;
  public orderingProviderIds: string | null;
  public orderingProviders: string | null;
  public reportingLab: string | null;
  public location: string | null;
  public ormOrOru: string | null;
  public messageDateTime: Date | null;
  public messageId: string | null;
  public additionalData: string | null;

  public labResultId: string;
  public testType: string | null;
  public outOfRange: boolean;
  public collectedDateTime: Date;
  public testStatus: string | null;
  public resultDescription: string | null;
  public receivedDateTime: Date;
  public resultDateTime: Date;
  public loinc: string | null;
  public loincName: string | null;

  public constructor(model: LaboratoryReport) {
    super(model.id, EntryType.Laboratory, model.messageDateTime);

    this.id = model.id;
    this.phn = model.phn;
    this.orderingProviderIds = model.orderingProviderIds;
    this.orderingProviders = model.orderingProviders;
    this.reportingLab = model.reportingLab;
    this.location = model.location;
    this.ormOrOru = model.ormOrOru;
    this.messageDateTime = model.messageDateTime;
    this.messageId = model.messageId;
    this.additionalData = model.additionalData;

    var labResult = model.labResults[0];
    this.labResultId = labResult.id;
    this.testType = labResult.testType;
    this.outOfRange = labResult.outOfRange;
    this.collectedDateTime = labResult.collectedDateTime;
    this.testStatus = labResult.testStatus;
    this.resultDescription = labResult.resultDescription;
    this.receivedDateTime = labResult.receivedDateTime;
    this.resultDateTime = labResult.resultDateTime;
    this.loinc = labResult.loinc;
    this.loincName = labResult.loincName;
  }

  public filterApplies(filterText: string, filterTypes: string[]): boolean {
    if (!filterTypes.includes("Laboratory")) {
      return false;
    }

    var text =
      (this.testType! || "") +
      (this.resultDescription! || "") +
      (this.loincName! || "");
    text = text.toUpperCase();
    return text.includes(filterText.toUpperCase());
  }
}
