import { StringISODate } from "@/models/dateWrapper";
import { EntryType } from "@/models/timelineEntry";

// Timeline filter model
export default class TimelineFilter {
    public keyword: string;
    public startDate: StringISODate;
    public endDate: StringISODate;
    public entryTypes: EntryTypeFilter[];
    public pageSize: number;
    public isListView: boolean;

    constructor(types: EntryTypeFilter[]) {
        this.keyword = "";
        this.entryTypes = types;
        this.pageSize = 25;
        this.endDate = "";
        this.startDate = "";
        this.isListView = true;
    }

    public hasActiveFilter(): boolean {
        return (
            !!this.endDate ||
            !!this.startDate ||
            !!this.keyword ||
            this.entryTypes.some((et) => et.isSelected)
        );
    }

    public getActiveFilterCount(): number {
        let count = 0;
        count += this.startDate === "" ? 0 : 1;
        count += this.endDate === "" ? 0 : 1;
        count += this.keyword === "" ? 0 : 1;
        count += this.entryTypes.reduce(
            (acumulator, entry) => (acumulator += entry.isSelected ? 1 : 0),
            0
        );
        return count;
    }

    public clear(): void {
        this.keyword = "";
        this.endDate = "";
        this.startDate = "";
        this.entryTypes.forEach((x) => (x.isSelected = false));
    }
}

// Entry filter model
export interface EntryTypeFilter {
    type: EntryType;
    display: string;
    isEnabled: boolean;
    numEntries: number;
    isSelected: boolean;
}
