import { EntryType } from "@/constants/entryType";
import { DateWrapper } from "@/models/dateWrapper";
import TimelineFilter from "@/models/timelineFilter";
import { UserComment } from "@/models/userComment";

export class DateGroup {
    key: string;
    date: DateWrapper;
    entries: TimelineEntry[];

    constructor(key: string, date: DateWrapper, entries: TimelineEntry[]) {
        this.key = key;
        this.date = date;
        this.entries = entries;
    }

    public static createGroups(timelineEntries: TimelineEntry[]): DateGroup[] {
        if (timelineEntries.length === 0) {
            return [];
        }
        const groups = timelineEntries.reduce<Record<string, TimelineEntry[]>>(
            (previousValue, entry) => {
                const date = entry.date.fromEpoch();

                // Create a new group if it the date doesnt exist in the map
                if (!previousValue[date]) {
                    previousValue[date] = [];
                }
                previousValue[date].push(entry);
                return previousValue;
            },
            {}
        );
        return Object.keys(groups).map<DateGroup>((dateKey) => {
            groups[dateKey].sort((a: TimelineEntry, b: TimelineEntry) =>
                a.type.localeCompare(b.type)
            );
            return new DateGroup(
                dateKey,
                groups[dateKey][0].date,
                groups[dateKey]
            );
        });
    }

    public static sortGroups(
        groupArrays: DateGroup[],
        ascending = true
    ): DateGroup[] {
        groupArrays.sort((a, b) => {
            const reverseMultiplier = ascending ? 1 : -1;

            if (a.date.isBefore(b.date)) {
                return 1 * reverseMultiplier;
            }
            if (a.date.isAfter(b.date)) {
                return -1 * reverseMultiplier;
            }
            return 0;
        });
        return groupArrays;
    }
}

// The base timeline entry model
export default abstract class TimelineEntry {
    public readonly id: string;
    public readonly type: EntryType;
    public readonly date: DateWrapper;

    public constructor(id: string, type: EntryType, date: DateWrapper) {
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
        if (this.comments !== null) {
            return this.comments.some((comment) =>
                comment.text.toUpperCase().includes(keyword.toUpperCase())
            );
        } else {
            return false;
        }
    }

    private entryTypeApplies(entryTypes: Set<EntryType>): boolean {
        return entryTypes.size === 0 || entryTypes.has(this.type);
    }

    private dateRangeApplies(filter: TimelineFilter): boolean {
        const startDateWapper = new DateWrapper(filter.startDate);
        const endDateWapper = new DateWrapper(filter.endDate + "T23:59:59.999");
        return (
            (!filter.startDate || this.date.isAfterOrSame(startDateWapper)) &&
            (!filter.endDate || this.date.isBeforeOrSame(endDateWapper))
        );
    }
}
