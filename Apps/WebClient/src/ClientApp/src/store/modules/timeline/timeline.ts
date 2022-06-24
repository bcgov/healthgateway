import { DateWrapper } from "@/models/dateWrapper";
import { TimelineFilterBuilder } from "@/models/timelineFilter";

import { actions } from "./actions";
import { getters } from "./getters";
import { mutations } from "./mutations";
import { TimelineModule, TimelineState } from "./types";

const state: TimelineState = {
    filter: TimelineFilterBuilder.buildEmpty(),
    keyword: "",
    linearDate: new DateWrapper(),
    calendarDate: new DateWrapper(),
    selectedDate: null,
};

const namespaced = true;

export const timeline: TimelineModule = {
    namespaced,
    state,
    getters,
    actions,
    mutations,
};
