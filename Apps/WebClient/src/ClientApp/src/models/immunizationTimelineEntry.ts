import DateTimeFormat from "@/constants/dateTimeFormat";
import { EntryType } from "@/constants/entryType";
import { DateWrapper } from "@/models/dateWrapper";
import {
    Forecast,
    ImmunizationAgent,
    ImmunizationEvent,
} from "@/models/immunizationModel";
import TimelineEntry from "@/models/timelineEntry";
import { UserComment } from "@/models/userComment";

// The immunization timeline entry model
export default class ImmunizationTimelineEntry extends TimelineEntry {
    public immunization: ImmunizationViewModel;

    public constructor(model: ImmunizationEvent) {
        super(
            model.id,
            EntryType.Immunization,
            new DateWrapper(model.dateOfImmunization)
        );
        this.immunization = new ImmunizationViewModel(model);
    }

    public get comments(): UserComment[] | null {
        return null;
    }

    public containsText(keyword: string): boolean {
        let text =
            (this.immunization.name || "") +
            (this.immunization.searchableAgentsText || "") +
            (this.immunization.providerOrClinic || "");
        text = text.toUpperCase();
        return text.includes(keyword.toUpperCase());
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

class ForecastViewModel {
    public displayName: string;
    public dueDate: string;
    public status: string;

    constructor(model: Forecast) {
        this.displayName = model.displayName;
        if (model.dueDate) {
            this.dueDate = DateWrapper.format(
                model.dueDate,
                DateTimeFormat.formatDateString
            );
        } else {
            this.dueDate = "";
        }

        this.status = model.status;
    }
}

class ImmunizationViewModel {
    public id: string;
    public isSelfReported: boolean;
    public location: string;
    public name: string;
    public status: string;
    public valid: boolean;
    public dateOfImmunization: DateWrapper;
    public providerOrClinic: string;
    public immunizationAgents: ImmunizationAgentViewModel[];
    public forecast?: ForecastViewModel;
    public targetedDisease: string;

    public searchableAgentsText: string;

    constructor(model: ImmunizationEvent) {
        this.id = model.id;
        this.isSelfReported = model.isSelfReported;
        this.location = model.location;
        this.name = model.immunization.name;
        this.status = model.status;
        this.valid = model.valid;
        this.dateOfImmunization = new DateWrapper(model.dateOfImmunization);
        this.providerOrClinic =
            model.providerOrClinic === "" ? "N/A" : model.providerOrClinic;
        this.immunizationAgents = [];
        model.immunization.immunizationAgents.forEach((agent) => {
            this.immunizationAgents.push(new ImmunizationAgentViewModel(agent));
        });
        if (model.forecast) {
            this.forecast = new ForecastViewModel(model.forecast);
        }

        this.targetedDisease = model.targetedDisease;

        this.searchableAgentsText = this.immunizationAgents.reduce(
            (accumulator: string, current: ImmunizationAgentViewModel) =>
                accumulator +
                (current.lotNumber || "") +
                (current.name || "") +
                (current.productName || ""),
            ""
        );
    }
}
