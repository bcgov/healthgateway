import TimelineEntry, { EntryType } from "@/models/timelineEntry";
import ImmunizationModel, {
    ImmunizationAgent,
} from "@/models/immunizationModel";
import { DateWrapper } from "@/models/dateWrapper";
import TimelineFilter from "@/models/timelineFilter";

// The immunization timeline entry model
export default class ImmunizationTimelineEntry extends TimelineEntry {
    public immunization: ImmunizationViewModel;

    public constructor(model: ImmunizationModel) {
        super(
            model.id,
            EntryType.Immunization,
            new DateWrapper(model.dateOfImmunization)
        );
        this.immunization = new ImmunizationViewModel(model);
    }

    public keywordApplies(filter: TimelineFilter): boolean {
        let text =
            (this.immunization.name || "") + (this.immunization.location || "");
        text = text.toUpperCase();
        return text.includes(filter.keyword.toUpperCase());
    }
}

class ImmunizationAgentViewModel {
    public code: string;
    public name: string;
    public lotNumber: string;
    public productName: string;

    constructor(model: ImmunizationAgent) {
        this.code = model.code;
        this.name = model.name;
        this.lotNumber = model.lotNumber === "" ? "N/A" : model.lotNumber;
        this.productName = model.productName === "" ? "N/A" : model.productName;
    }
}

class ImmunizationViewModel {
    public id: string;
    public isSelfReported: boolean;
    public location: string;
    public name: string;
    public status: string;
    public dateOfImmunization: DateWrapper;
    public providerOrClinic: string;
    public immunizationAgents: ImmunizationAgentViewModel[];

    constructor(model: ImmunizationModel) {
        this.id = model.id;
        this.isSelfReported = model.isSelfReported;
        this.location = model.location;
        this.name = model.name;
        this.status = model.status;
        this.dateOfImmunization = new DateWrapper(model.dateOfImmunization);
        this.providerOrClinic =
            model.providerOrClinic === "" ? "N/A" : model.providerOrClinic;
        this.immunizationAgents = [];
        model.immunizationAgents.forEach((agent) => {
            this.immunizationAgents.push(new ImmunizationAgentViewModel(agent));
        });
    }
}
