import type { DateTime } from "luxon";

import type { EntryType } from "@/constants/entryType";
import type { UserComment } from "@/models/userComment";
import { fromIsoDate } from "@/utils/dateUtil";

import type { TimelineFilter } from "./timelineFilter";

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

    protected abstract containsText(keyword: string): boolean;

    /**
     * Determines whether the entry matches a filter's entry types, date range and keyword.
     * @param filter The timeline filter.
     * @param comments The entry's comments, which the keyword is also matched against.
     * @returns True when the entry matches every part of the filter.
     */
    public matchesFilter(
        filter: TimelineFilter,
        comments?: UserComment[] | null
    ): boolean {
        const keyword = filter.keyword.toUpperCase();
        return (
            (filter.entryTypes.size === 0 ||
                filter.entryTypes.has(this.type)) &&
            (!filter.startDate || this.date >= fromIsoDate(filter.startDate)) &&
            (!filter.endDate ||
                this.date < fromIsoDate(filter.endDate).plus({ days: 1 })) &&
            (!keyword ||
                this.containsText(filter.keyword) ||
                (comments ?? []).some((c) =>
                    c.text.toUpperCase().includes(keyword)
                ))
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
