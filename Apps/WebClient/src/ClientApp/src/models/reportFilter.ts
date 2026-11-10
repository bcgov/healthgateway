import { DateTime } from "luxon";

import { formatDate, fromIsoDate, today } from "@/utils/dateUtil";

/** Port of models/reportFilter.ts */
export interface IReportFilter {
    readonly startDate: string | null;
    readonly endDate: string | null;
    readonly medications: string[];
    readonly filterText: string;
    allowsDate(date: DateTime): boolean;
    allowsMedication(medicationName: string): boolean;
    hasActiveFilter(): boolean;
    hasMedicationsFilter(): boolean;
    hasDateFilter(): boolean;
}

export class ReportFilter implements IReportFilter {
    public readonly startDate: string | null;
    public readonly endDate: string | null;
    public readonly medications: string[];

    /** Only the builder constructs a ReportFilter. */
    public constructor(builder: ReportFilterBuilder) {
        this.endDate = builder.endDate;
        this.startDate = builder.startDate;
        this.medications = builder.medications;
    }

    public allowsDate(date: DateTime): boolean {
        let filterStart = true;
        if (this.startDate !== null)
            filterStart = date >= fromIsoDate(this.startDate);

        let filterEnd = true;
        if (this.endDate !== null)
            filterEnd =
                date <
                fromIsoDate(this.endDate).plus({ days: 1 }).startOf("day");
        return filterStart && filterEnd;
    }

    public allowsMedication(medicationName: string): boolean {
        return !this.medications.includes(medicationName);
    }

    public hasActiveFilter(): boolean {
        return this.hasDateFilter() || this.hasMedicationsFilter();
    }

    public hasMedicationsFilter(): boolean {
        return this.medications.length > 0;
    }

    public hasDateFilter(): boolean {
        return !!this.endDate || !!this.startDate;
    }

    public get filterText(): string {
        if (!this.hasDateFilter()) return "";
        const start = this.startDate
            ? ` from ${formatDate(fromIsoDate(this.startDate))}`
            : "";
        const end = this.endDate
            ? formatDate(fromIsoDate(this.endDate))
            : formatDate(today());
        return `Displaying records${start} up to ${end}`;
    }
}

export class ReportFilterBuilder {
    private _startDate: string | null = null;
    private _endDate: string | null = null;
    private _medications: string[] = [];

    private constructor() {}

    public get startDate(): string | null {
        return this._startDate;
    }

    public get endDate(): string | null {
        return this._endDate;
    }

    public get medications(): string[] {
        return this._medications;
    }

    public static create(): ReportFilterBuilder {
        return new ReportFilterBuilder();
    }

    public static createFromFilter(filter: ReportFilter): ReportFilterBuilder {
        const builder = new ReportFilterBuilder();
        builder._startDate = filter.startDate;
        builder._endDate = filter.endDate;
        builder._medications = filter.medications;
        return builder;
    }

    public static buildEmpty(): ReportFilter {
        return new ReportFilter(new ReportFilterBuilder());
    }

    public withStartDate(dateString: string | null): this {
        this._startDate = dateString;
        return this;
    }

    public withEndDate(dateString: string | null): this {
        this._endDate = dateString;
        return this;
    }

    public withMedications(medications: string[]): this {
        this._medications = medications;
        return this;
    }

    public withMedication(medication: string): this {
        this._medications.push(medication);
        return this;
    }

    public build(): ReportFilter {
        return new ReportFilter(this);
    }
}
