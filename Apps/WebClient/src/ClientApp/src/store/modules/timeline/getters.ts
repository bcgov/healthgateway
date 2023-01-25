import { EntryType } from "@/constants/entryType";
import { DateWrapper } from "@/models/dateWrapper";
import TimelineFilter, { TimelineFilterBuilder } from "@/models/timelineFilter";

import { TimelineGetters, TimelineState } from "./types";

export const getters: TimelineGetters = {
    filter(state: TimelineState): TimelineFilter {
        if (state.filter instanceof TimelineFilter) {
            return state.filter;
        }

        return Object.assign(TimelineFilterBuilder.buildEmpty(), state.filter);
    },

    hasActiveFilter(state: TimelineState): boolean {
        if (state.filter instanceof TimelineFilter) {
            return state.filter.hasActiveFilter() || state.keyword !== "";
        }

        const timelineFilter: TimelineFilter = Object.assign(
            TimelineFilterBuilder.buildEmpty(),
            state.filter
        );

        return timelineFilter.hasActiveFilter() || state.keyword !== "";
    },

    keyword(state: TimelineState): string {
        return state.keyword;
    },

    linearDate(state: TimelineState): DateWrapper {
        return new DateWrapper(state.linearDate);
    },

    selectedDate(state: TimelineState): DateWrapper | null {
        if (state.selectedDate === null) {
            return null;
        }

        return new DateWrapper(state.linearDate);
    },

    entryTypes(state: TimelineState): Set<EntryType> {
        if (
            state.filter instanceof TimelineFilter &&
            state.filter.entryTypes instanceof Set<EntryType>
        ) {
            return state.filter.entryTypes;
        }

        return Object.assign(new Set<EntryType>(), state.filter.entryTypes);
    },
};
