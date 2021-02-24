import { ActionTree } from "vuex";

import { DateWrapper } from "@/models/dateWrapper";
import { RootState, TimelineState } from "@/models/storeState";
import { TimelineFilterBuilder } from "@/models/timelineFilter";

export const actions: ActionTree<TimelineState, RootState> = {
    setFilter(context, filterBuilder: TimelineFilterBuilder) {
        context.commit("setFilter", filterBuilder.build());
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

    setLinearDate(context, currentPage: number) {
        context.commit("setLinearDate", currentPage);
    },

    setCalendarDate(context, currentDate: DateWrapper) {
        context.commit("setCalendarDate", currentDate);
    },

    setSelectedDate(context, selectedDate: DateWrapper | null) {
        context.commit("setSelectedDate", selectedDate);
    },
};
