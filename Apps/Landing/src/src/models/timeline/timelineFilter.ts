import { EntryType } from "@/constants/entryType";
import { StringISODate } from "@/models/dateWrapper";

// Timeline filter model
export default class TimelineFilter {
    public readonly keyword: string;
    public readonly startDate: StringISODate;
    public readonly endDate: StringISODate;
    public readonly entryTypes: Set<EntryType>;

    /**
     * Note: Should only be called from the timeline filter builder
     * @param builder The builder to use when constructing a TimelineFilter
     */
    public constructor(builder: TimelineFilterBuilder) {
        this.keyword = builder.keyword;
        this.entryTypes = builder.entryTypes;
        this.endDate = builder.endDate;
        this.startDate = builder.startDate;
    }

    public hasActiveFilter(): boolean {
        return (
            this.keyword.length > 0 ||
            !!this.endDate ||
            !!this.startDate ||
            this.entryTypes.size > 0
        );
    }
}

export class TimelineFilterBuilder {
    private _keyword: string;
    private _entryTypes: Set<EntryType>;
    private _startDate: StringISODate;
    private _endDate: StringISODate;

    private constructor() {
        this._keyword = "";
        this._entryTypes = new Set();
        this._endDate = "";
        this._startDate = "";
    }

    public get keyword(): string {
        return this._keyword;
    }

    public get entryTypes(): Set<EntryType> {
        return this._entryTypes;
    }

    public get startDate(): StringISODate {
        return this._startDate;
    }

    public get endDate(): StringISODate {
        return this._endDate;
    }

    public static create(): TimelineFilterBuilder {
        return new TimelineFilterBuilder();
    }

    public static createFromFilter(
        filter: TimelineFilter
    ): TimelineFilterBuilder {
        const builder = new TimelineFilterBuilder();
        builder._keyword = filter.keyword;
        builder._startDate = filter.startDate;
        builder._endDate = filter.endDate;
        builder._entryTypes = filter.entryTypes;
        return builder;
    }

    public static buildEmpty(): TimelineFilter {
        return new TimelineFilter(new TimelineFilterBuilder());
    }

    public withKeyword(keyword: string): this {
        this._keyword = keyword;
        return this;
    }

    public withStartDate(dateString: StringISODate): this {
        this._startDate = dateString;
        return this;
    }

    public withEndDate(dateString: StringISODate): this {
        this._endDate = dateString;
        return this;
    }

    public withEntryTypes(entryTypes: EntryType[]): this {
        this._entryTypes = new Set<EntryType>(entryTypes);
        return this;
    }

    public withEntryType(entryType: EntryType): this {
        this._entryTypes.add(entryType);
        return this;
    }

    public build(): TimelineFilter {
        return new TimelineFilter(this);
    }
}
