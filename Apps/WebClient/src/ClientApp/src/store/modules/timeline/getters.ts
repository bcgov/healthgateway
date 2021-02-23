import { GetterTree } from "vuex";

import { RootState, TimelineState } from "@/models/storeState";
import TimelineFilter from "@/models/timelineFilter";

export const getters: GetterTree<TimelineState, RootState> = {
    filter(state: TimelineState): TimelineFilter {
        return state.filter;
    },

    hasActiveFilter(state: TimelineState): boolean {
        return state.filter.hasActiveFilter() || state.keyword !== "";
    },

    isListView(state: TimelineState): boolean {
        return state.isLinearView;
    },

    keyword(state: TimelineState): string {
        return state.keyword;
    },
};
