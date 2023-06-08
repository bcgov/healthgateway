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

    setLinearDate(context, linearDate: DateWrapper) {
        context.commit("setLinearDate", linearDate);
    },

    setSelectedDate(context, selectedDate: DateWrapper | null) {
        context.commit("setSelectedDate", selectedDate);
    },
};
