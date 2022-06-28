import { DateWrapper } from "@/models/dateWrapper";
import { TimelineFilterBuilder } from "@/models/timelineFilter";

import { TimelineActions } from "./types";

export const actions: TimelineActions = {
    setFilter(context, filterBuilder: TimelineFilterBuilder) {
        context.commit("setFilter", filterBuilder.build());
    },

    clearFilter(context) {
        context.commit("clearFilter");
    },

    setLinearDate(context, currentPage: number) {
        context.commit("setLinearDate", currentPage);
    },

    setSelectedDate(context, selectedDate: DateWrapper | null) {
        context.commit("setSelectedDate", selectedDate);
    },
};
