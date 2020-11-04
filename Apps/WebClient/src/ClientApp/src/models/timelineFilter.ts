import { StringISODate } from "@/models/dateWrapper";
import { EntryType } from "@/models/timelineEntry";

// Timeline filter model
export default class TimelineFilter {
    keyword: string;
    startDate: StringISODate;
    endDate: StringISODate;
    entryTypes: EntryTypeFilter[];
    pageSize: number;

    constructor() {
        this.keyword = "";
        this.entryTypes = [];
        this.pageSize = 25;
        this.endDate = "";
        this.startDate = "";
    }

    public static hasFilter(filter: TimelineFilter): boolean {
        return (
            !!filter.endDate ||
            !!filter.startDate ||
            !!filter.keyword ||
            filter.entryTypes?.some((et) => et.isSelected)
        );
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
