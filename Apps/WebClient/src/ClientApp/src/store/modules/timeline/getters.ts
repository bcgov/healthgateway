import { GetterTree } from "vuex";

import { DateWrapper } from "@/models/dateWrapper";
import { RootState, TimelineState } from "@/models/storeState";
import TimelineFilter from "@/models/timelineFilter";

export const getters: GetterTree<TimelineState, RootState> = {
    filter(state: TimelineState): TimelineFilter {
        return state.filter;
    },

    hasActiveFilter(state: TimelineState): boolean {
        return state.filter.hasActiveFilter() || state.keyword !== "";
    },

    isLinearView(state: TimelineState): boolean {
        return state.isLinearView;
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
