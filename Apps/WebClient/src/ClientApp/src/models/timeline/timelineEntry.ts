import type { DateTime } from "luxon";

import type { EntryType } from "@/constants/entryType";
import type { UserComment } from "@/models/userComment";
import { fromIsoDate } from "@/utils/dateUtil";

import type { TimelineFilter } from "./timelineFilter";

export type { UserComment } from "@/models/userComment";

/** Port of models/timeline/timelineEntry.ts: the base timeline entry. */
export abstract class TimelineEntry {
    public readonly id: string;
    public readonly type: EntryType;
    public readonly date: DateTime;

    public constructor(id: string, type: EntryType, date: DateTime) {
        this.id = id;
        this.type = type;
        this.date = date;
    }

    public abstract get comments(): UserComment[] | null;

    public filterApplies(filter: TimelineFilter): boolean {
        return (
            this.entryTypeApplies(filter.entryTypes) &&
            this.dateRangeApplies(filter) &&
            this.keywordApplies(filter.keyword)
        );
    }

    protected abstract containsText(keyword: string): boolean;

    private keywordApplies(keyword: string): boolean {
        return (
            !keyword ||
            this.containsText(keyword) ||
            this.commentApplies(keyword)
        );
    }

    private commentApplies(keyword: string): boolean {
        if (this.comments === null) return false;
        return this.comments.some((comment) =>
            comment.text.toUpperCase().includes(keyword.toUpperCase())
        );
    }

    private entryTypeApplies(entryTypes: Set<EntryType>): boolean {
        return entryTypes.size === 0 || entryTypes.has(this.type);
    }

    private dateRangeApplies(filter: TimelineFilter): boolean {
        return (
            (!filter.startDate || this.date >= fromIsoDate(filter.startDate)) &&
            (!filter.endDate ||
                this.date < fromIsoDate(filter.endDate).plus({ days: 1 }))
        );
    }
}

/** Port of DateGroup: entries sharing one exact timestamp, keyed by its epoch milliseconds. */
export class DateGroup {
    constructor(
        public key: string,
        public date: DateTime,
        public entries: TimelineEntry[]
    ) {}

    public static createGroups(timelineEntries: TimelineEntry[]): DateGroup[] {
        if (timelineEntries.length === 0) return [];
        const groups = timelineEntries.reduce<Record<string, TimelineEntry[]>>(
            (previous, entry) => {
                const date = entry.date.valueOf().toString();
                (previous[date] ??= []).push(entry);
                return previous;
            },
            {}
        );
        return Object.keys(groups).map((dateKey) => {
            groups[dateKey].sort((a, b) => a.type.localeCompare(b.type));
            return new DateGroup(
                dateKey,
                groups[dateKey][0].date,
                groups[dateKey]
            );
        });
    }

    public static sortGroups(groupArrays: DateGroup[], ascending = true) {
        groupArrays.sort((a, b) => {
            const reverseMultiplier = ascending ? 1 : -1;
            if (a.date < b.date) return 1 * reverseMultiplier;
            if (a.date > b.date) return -1 * reverseMultiplier;
            return 0;
        });
        return groupArrays;
    }
}
