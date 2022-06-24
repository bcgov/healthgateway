import { DateWrapper } from "@/models/dateWrapper";
import TimelineFilter from "@/models/timelineFilter";

import { TimelineGetters, TimelineState } from "./types";

export const getters: TimelineGetters = {
    filter(state: TimelineState): TimelineFilter {
        return state.filter;
    },

    hasActiveFilter(state: TimelineState): boolean {
        return state.filter.hasActiveFilter() || state.keyword !== "";
    },

    keyword(state: TimelineState): string {
        return state.keyword;
    },

    linearDate(state: TimelineState): DateWrapper {
        return state.linearDate;
    },

    calendarDate(state: TimelineState): DateWrapper {
        return state.calendarDate;
    },

    selectedDate(state: TimelineState): DateWrapper | null {
        return state.selectedDate;
    },
};
