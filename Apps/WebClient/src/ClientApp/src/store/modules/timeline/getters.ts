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
        debugger;
        if (state.linearDate instanceof DateWrapper) {
            return state.linearDate;
        }

        return Object.assign(new DateWrapper(), state.linearDate);
    },

    selectedDate(state: TimelineState): DateWrapper | null {
        console.log(
            "Timeline Getter Selected Date: " +
                JSON.stringify(state.selectedDate)
        );

        if (
            state.selectedDate === null ||
            state.selectedDate instanceof DateWrapper
        ) {
            return state.selectedDate;
        } else {
            return Object.assign(new DateWrapper(), state.selectedDate);
        }
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
