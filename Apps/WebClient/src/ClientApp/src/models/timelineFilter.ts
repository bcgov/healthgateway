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
}

// Entry filter model
export interface EntryTypeFilter {
    type: EntryType;
    display: string;
    isEnabled: boolean;
    numEntries: number;
    isSelected: boolean;
}
