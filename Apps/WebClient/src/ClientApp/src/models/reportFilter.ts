import { DateWrapper, StringISODate } from "@/models/dateWrapper";

// Report filter model
export default class ReportFilter {
    public readonly startDate: StringISODate | null;
    public readonly endDate: StringISODate | null;
    public readonly medications: string[];

    /**
     * Note: Should only be called from the report filter builder
     * @param builder The builder to use when constructing a ReportFilter
     */
    public constructor(builder: ReportFilterBuilder) {
        this.endDate = builder.endDate;
        this.startDate = builder.startDate;
        this.medications = builder.medications;
    }

    public allowsDate(dateString: StringISODate): boolean {
        let filterStart = true;
        if (this.startDate !== null) {
            filterStart = new DateWrapper(dateString).isAfterOrSame(
                new DateWrapper(this.startDate)
            );
        }

        let filterEnd = true;
        if (this.endDate !== null) {
            filterEnd = new DateWrapper(dateString).isBeforeOrSame(
                new DateWrapper(this.endDate)
            );
        }
        return filterStart && filterEnd;
    }

    public allowsMedication(medicationName: string): boolean {
        return !this.medications.some((x) => x === medicationName);
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
        if (!this.hasDateFilter()) {
            return "";
        }

        const start = this.startDate
            ? ` from ${this.formatDate(this.startDate)}`
            : "";
        const end = this.endDate
            ? this.formatDate(this.endDate)
            : this.formatDate(new DateWrapper().toISO());
        return `Displaying records${start} up to ${end}`;
    }

    private formatDate(date: string): string {
        return new DateWrapper(date).format();
    }
}

export class ReportFilterBuilder {
    private _startDate: StringISODate | null;
    private _endDate: StringISODate | null;
    private _medications: string[];

    private constructor() {
        this._endDate = null;
        this._startDate = null;
        this._medications = [];
    }

    public get startDate(): StringISODate | null {
        return this._startDate;
    }

    public get endDate(): StringISODate | null {
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

    public withStartDate(
        dateString: StringISODate | null
    ): ReportFilterBuilder {
        this._startDate = dateString;
        return this;
    }

    public withEndDate(dateString: StringISODate | null): ReportFilterBuilder {
        this._endDate = dateString;
        return this;
    }

    public withMedications(medications: string[]): ReportFilterBuilder {
        this._medications = medications;
        return this;
    }

    public withMedication(medication: string): ReportFilterBuilder {
        this._medications.push(medication);
        return this;
    }

    public build(): ReportFilter {
        return new ReportFilter(this);
    }
}
