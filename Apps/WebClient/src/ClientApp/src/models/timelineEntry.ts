import { DateWrapper, StringISODate } from "./dateWrapper";

export enum EntryType {
    Medication,
    Immunization,
    Laboratory,
    Encounter,
    Note,
    NONE,
}

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
        let groups = timelineEntries.reduce<Record<string, TimelineEntry[]>>(
            (groups, entry) => {
                let date = entry.date.fromEpoch();

                // Create a new group if it the date doesnt exist in the map
                if (!groups[date]) {
                    groups[date] = [];
                }
                groups[date].push(entry);
                return groups;
            },
            {}
        );
        let groupArrays = Object.keys(groups).map<DateGroup>((dateKey) => {
            return new DateGroup(
                dateKey,
                groups[dateKey][0].date,
                groups[dateKey].sort((a: TimelineEntry, b: TimelineEntry) =>
                    a.type > b.type ? 1 : a.type < b.type ? -1 : 0
                )
            );
        });
        return groupArrays;
    }

    public static sortGroup(
        groupArrays: DateGroup[],
        ascending: boolean = true
    ) {
        groupArrays.sort((a, b) =>
            a.date.isAfter(b.date)
                ? -1 * (ascending ? 1 : -1)
                : a.date.isBefore(b.date)
                ? 1 * (ascending ? 1 : -1)
                : 0
        );
        return groupArrays;
    }
}

// The base timeline entry model
export default abstract class TimelineEntry {
    public readonly id: string;
    public readonly type: EntryType;
    public readonly date: DateWrapper;

    public constructor(id: string, type: EntryType, date: StringISODate) {
        this.id = id;
        this.type = type;
        this.date = new DateWrapper(date);
    }

    public abstract filterApplies(
        filterText: string,
        filterTypes: string[]
    ): boolean;
}
