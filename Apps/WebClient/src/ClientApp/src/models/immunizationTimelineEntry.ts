import TimelineEntry, { EntryType } from "@/models/timelineEntry";
import ImmunizationModel from "@/models/immunizationModel";
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

class ImmunizationViewModel {
    public id: string;
    public isSelfReported: boolean;
    public location: string;
    public name: string;
    public dateOfImmunization: DateWrapper;
    public providerOrClinic: string;
    public productName: string;
    public lotNumber: string;

    constructor(model: ImmunizationModel) {
        this.id = model.id;
        this.isSelfReported = model.isSelfReported;
        this.location = model.location;
        this.name = model.name;
        this.dateOfImmunization = new DateWrapper(model.dateOfImmunization);
        this.providerOrClinic = model.providerOrClinic;
        this.productName = model.immunizationAgents[0].productName;
        this.lotNumber = model.immunizationAgents[0].lotNumber;
    }
}
