import TimelineEntry, { EntryType } from "@/models/timelineEntry";
import ImmunizationData from "@/models/immunizationData";

// The immunization timeline entry model
export default class ImmunizationTimelineEntry extends TimelineEntry {
  public immunization: ImmunizationViewModel;

  public constructor(model: ImmunizationData) {
    super(model.id, EntryType.Immunization, model.occurrenceDateTime);
    this.immunization = new ImmunizationViewModel(model);
  }

  public filterApplies(filterText: string): boolean {
    var text =
      (this.immunization.name! || "") +
      (this.immunization.status! || "") +
      (this.immunization.agentCode! || "") +
      (this.immunization.agentDisplay! || "");
    text = text.toUpperCase();
    return text.includes(filterText.toUpperCase());
  }
}

class ImmunizationViewModel {
  public name?: string;
  public status?: string;
  public agentCode?: string;
  public agentDisplay?: string;
  public occurrenceDateTime: Date;

  constructor(model: ImmunizationData) {
    this.name = model.name;
    this.status = model.status;
    this.agentCode = model.immunizationAgentCode;
    this.agentDisplay = model.immunizationAgentDisplay;
    this.occurrenceDateTime = model.occurrenceDateTime;
  }
}
