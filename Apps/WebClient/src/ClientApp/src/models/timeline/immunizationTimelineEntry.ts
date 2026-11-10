import type { DateTime } from "luxon";

import { EntryType } from "@/constants/entryType";
import type {
    Forecast,
    ImmunizationAgent,
    ImmunizationEvent,
} from "@/models/immunization";
import { formatDate, fromIso, fromIsoDate } from "@/utils/dateUtil";

import { TimelineEntry, type UserComment } from "./timelineEntry";

/** DateTimeFormat.formatDateString */
const formatDateString = "yyyy-MM-dd";

class ImmunizationAgentViewModel {
    public code: string;
    public name: string;
    public lotNumber: string | null;
    public productName: string | null;

    constructor(model: ImmunizationAgent) {
        this.code = model.code;
        this.name = model.name;
        this.lotNumber = model.lotNumber || null;
        this.productName = model.productName || null;
    }
}

class ForecastViewModel {
    public displayName: string;
    public dueDate: string;
    public status: string;

    constructor(model: Forecast) {
        this.displayName = model.displayName;
        this.dueDate = formatDate(fromIsoDate(model.dueDate), formatDateString);
        this.status = model.status;
    }
}

class ImmunizationViewModel {
    public id: string;
    public name: string;
    public status: string;
    public dateOfImmunization: DateTime;
    public providerOrClinic: string | null;
    public immunizationAgents: ImmunizationAgentViewModel[];
    public forecast?: ForecastViewModel;
    public searchableAgentsText: string;

    constructor(model: ImmunizationEvent) {
        this.id = model.id;
        this.name = model.immunization.name;
        this.status = model.status;
        this.dateOfImmunization = fromIsoDate(model.dateOfImmunization);
        this.providerOrClinic = model.providerOrClinic || null;
        this.immunizationAgents = model.immunization.immunizationAgents.map(
            (agent) => new ImmunizationAgentViewModel(agent)
        );
        if (model.forecast)
            this.forecast = new ForecastViewModel(model.forecast);
        this.searchableAgentsText = this.immunizationAgents.reduce(
            (accumulator, current) =>
                accumulator +
                (current.lotNumber ?? "") +
                (current.name ?? "") +
                (current.productName ?? ""),
            ""
        );
    }
}

/** Port of models/timeline/immunizationTimelineEntry.ts */
export class ImmunizationTimelineEntry extends TimelineEntry {
    public immunization: ImmunizationViewModel;

    public constructor(model: ImmunizationEvent) {
        super(
            model.id,
            EntryType.Immunization,
            fromIso(model.dateOfImmunization)
        );
        this.immunization = new ImmunizationViewModel(model);
    }

    public get comments(): UserComment[] | null {
        return null;
    }

    public containsText(keyword: string): boolean {
        const text = (
            (this.immunization.name ?? "") +
            (this.immunization.searchableAgentsText ?? "") +
            (this.immunization.providerOrClinic ?? "")
        ).toUpperCase();
        return text.includes(keyword.toUpperCase());
    }
}
