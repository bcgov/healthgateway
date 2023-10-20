import { DateWrapper, IDateWrapper, StringISODate } from "@/models/dateWrapper";

export interface IReportFilter {
    readonly startDate: StringISODate | null;
    readonly endDate: StringISODate | null;
    readonly medications: string[];
    readonly filterText: string;
    allowsDate(date: IDateWrapper): boolean;
    allowsMedication(medicationName: string): boolean;
    hasActiveFilter(): boolean;
    hasMedicationsFilter(): boolean;
    hasDateFilter(): boolean;
}

// Report filter model
export default class ReportFilter implements IReportFilter {
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

    public allowsDate(date: IDateWrapper): boolean {
        let filterStart = true;
        if (this.startDate !== null) {
            filterStart = date.isAfterOrSame(
                DateWrapper.fromIsoDate(this.startDate)
            );
        }

        let filterEnd = true;
        if (this.endDate !== null) {
            filterEnd = date.isBefore(
                DateWrapper.fromIsoDate(this.endDate)
                    .add({ days: 1 })
                    .startOf("day")
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
            ? ` from ${DateWrapper.fromIsoDate(this.startDate).format()}`
            : "";
        const end = this.endDate
            ? DateWrapper.fromIsoDate(this.endDate).format()
            : DateWrapper.today().format();
        return `Displaying records${start} up to ${end}`;
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

    public withStartDate(dateString: StringISODate | null): this {
        this._startDate = dateString;
        return this;
    }

    public withEndDate(dateString: StringISODate | null): this {
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
