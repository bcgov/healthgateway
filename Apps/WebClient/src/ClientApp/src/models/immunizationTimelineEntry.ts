import TimelineEntry, { EntryType } from "@/models/timelineEntry";
import ImmunizationData, { ImmunizationAgent } from "@/models/immunizationData";

// The immunization timeline entry model
export default class ImmunizationTimelineEntry extends TimelineEntry {
    public immunization: ImmunizationViewModel;

    public constructor(model: ImmunizationData) {
        super(model.id, EntryType.Immunization, model.occurrenceDateTime);
        this.immunization = new ImmunizationViewModel(model);
    }

    public filterApplies(filterText: string, filterTypes: string[]): boolean {
        if (!filterTypes.includes("Immunization")) {
            return false;
        }

        let text =
            (this.immunization.name! || "") +
            (this.immunization.agents.toString() || "");
        text = text.toUpperCase();
        return text.includes(filterText.toUpperCase());
    }
}

class ImmunizationViewModel {
    public name?: string;
    public status?: string;
    public agents: string;
    public occurrenceDateTime: Date;

    constructor(model: ImmunizationData) {
        this.name = model.name;
        this.status = model.status;
        this.occurrenceDateTime = model.occurrenceDateTime;
        this.agents = model.immunizationAgents
            .map(function (agent) {
                return agent.name;
            })
            .join(",");
    }
}
