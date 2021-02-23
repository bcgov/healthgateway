import { Module } from "vuex";

import { RootState, TimelineState } from "@/models/storeState";
import { TimelineFilterBuilder } from "@/models/timelineFilter";

import { actions } from "./actions";
import { getters } from "./getters";
import { mutations } from "./mutations";

export const state: TimelineState = {
    filter: TimelineFilterBuilder.buildEmpty(),
    keyword: "",
    isLinearView: true,
};

const namespaced = true;

export const timeline: Module<TimelineState, RootState> = {
    namespaced,
    state,
    getters,
    actions,
    mutations,
};
