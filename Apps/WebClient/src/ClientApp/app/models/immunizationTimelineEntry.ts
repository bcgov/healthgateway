import TimelineEntry, { EntryType } from "@/models/timelineEntry";
import ImmunizationData, { ImmunizationAgent } from "@/models/immunizationData";

// The immunization timeline entry model
export default class ImmunizationTimelineEntry extends TimelineEntry {
  public immunization: ImmunizationViewModel;

  public constructor(model: ImmunizationData) {
    super(model.id, EntryType.Immunization, model.occurrenceDateTime);
    this.immunization = new ImmunizationViewModel(model);
  }

  public filterApplies(filterText: string): boolean {
    var agentNames = this.immunization.agents.map(function(agent) {
      return agent.display;
    });

    var text = (this.immunization.name! || "") + (agentNames.toString() || "");
    text = text.toUpperCase();
    return text.includes(filterText.toUpperCase());
  }
}

class ImmunizationViewModel {
  public name?: string;
  public status?: string;
  public agents: ImmunizationAgentViewModel[];
  public occurrenceDateTime: Date;

  constructor(model: ImmunizationData) {
    this.name = model.name;
    this.status = model.status;
    this.occurrenceDateTime = model.occurrenceDateTime;
    this.agents = [];

    for (let index = 0; index < model.immunizationAgents.length; index++) {
      const agent = model.immunizationAgents[index];

      this.agents.push(new ImmunizationAgentViewModel(agent));
    }
  }
}

class ImmunizationAgentViewModel {
  public code?: string;
  public display?: string;
  constructor(model: ImmunizationAgent) {
    this.code = model.code;
    this.display = model.name;
  }
}
