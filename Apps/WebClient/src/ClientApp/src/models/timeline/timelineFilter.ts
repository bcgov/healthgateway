import type { EntryType } from "@/constants/entryType";

/** Port of models/timeline/timelineFilter.ts */
export class TimelineFilter {
    public readonly keyword: string;
    public readonly startDate: string;
    public readonly endDate: string;
    public readonly entryTypes: Set<EntryType>;

    /** Only the builder constructs a TimelineFilter. */
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
    private _keyword = "";
    private _entryTypes = new Set<EntryType>();
    private _startDate = "";
    private _endDate = "";

    private constructor() {}

    public get keyword(): string {
        return this._keyword;
    }

    public get entryTypes(): Set<EntryType> {
        return this._entryTypes;
    }

    public get startDate(): string {
        return this._startDate;
    }

    public get endDate(): string {
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

    public withStartDate(dateString: string): this {
        this._startDate = dateString;
        return this;
    }

    public withEndDate(dateString: string): this {
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
