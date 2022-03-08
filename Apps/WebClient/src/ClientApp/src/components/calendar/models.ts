import { EntryType } from "@/constants/entryType";
import { DateWrapper } from "@/models/dateWrapper";
import TimelineEntry from "@/models/timelineEntry";

// A calendar month includes weeks that are being displayed that could be outside the regular month
export interface CalendarMonth {
    name: string;
    year: number;
    weeks: CalendarWeek[];
}

export interface CalendarWeek {
    id: string;
    days: CalendarDay[];
}

export interface CalendarDay {
    id: string;
    monthDay: number;
    isToday: boolean;
    isCurMonth: boolean;
    weekDay: number;
    date: DateWrapper;
    events: CalendarEntry[];
}

export interface CalendarEntry {
    id: string;
    type: EntryType;
    cellIndex: number;
    entries: TimelineEntry[];
}
