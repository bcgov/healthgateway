import TimelineEntry, { EntryType } from "@/models/timelineEntry";
import ImmunizationModel from "@/models/immunizationModel";
<<<<<<< HEAD
import { DateWrapper } from "@/models/dateWrapper";
=======
>>>>>>> master

// The immunization timeline entry model
export default class ImmunizationTimelineEntry extends TimelineEntry {
    public immunization: ImmunizationViewModel;

    public constructor(model: ImmunizationModel) {
        super(model.id, EntryType.Immunization, model.dateOfImmunization);
        this.immunization = new ImmunizationViewModel(model);
    }

    public filterApplies(filterText: string, filterTypes: string[]): boolean {
        if (!filterTypes.includes("Immunization")) {
            return false;
        }

        let text =
            (this.immunization.name! || "") +
            (this.immunization.location! || "");
        text = text.toUpperCase();
        return text.includes(filterText.toUpperCase());
    }
}

class ImmunizationViewModel {
    public id: string;
    public isSelfReported: boolean;
    public location: string;
    public name: string;
<<<<<<< HEAD
    public dateOfImmunization: DateWrapper;
=======
    public dateOfImmunization: Date;
>>>>>>> master

    constructor(model: ImmunizationModel) {
        this.id = model.id;
        this.isSelfReported = model.isSelfReported;
        this.location = model.location;
        this.name = model.name;
<<<<<<< HEAD
        this.dateOfImmunization = new DateWrapper(model.dateOfImmunization);
=======
        this.dateOfImmunization = model.dateOfImmunization;
>>>>>>> master
    }
}
