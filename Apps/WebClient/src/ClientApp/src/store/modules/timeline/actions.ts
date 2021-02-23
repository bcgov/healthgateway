import { ActionTree } from "vuex";

import { RootState, TimelineState } from "@/models/storeState";
import { TimelineFilterBuilder } from "@/models/timelineFilter";

export const actions: ActionTree<TimelineState, RootState> = {
    setFilter(context, filter: TimelineFilterBuilder) {
        context.commit("setFilter", filter.build());
    },

    setKeyword(context, keyword: string) {
        context.commit("setKeyword", keyword);
    },

    setLinearView(context, isLinearView: boolean) {
        context.commit("setLinearView", isLinearView);
    },

    clearFilter(context) {
        context.commit("clearFilter");
    },
};
