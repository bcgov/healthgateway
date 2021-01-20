import { Module } from "vuex";

import { LaboratoryState, RootState, StateType } from "@/models/storeState";

import { actions } from "./actions";
import { getters } from "./getters";
import { mutations } from "./mutations";

export const state: LaboratoryState = {
    statusMessage: "",
    laboratoryOrders: [],
    error: false,
    stateType: StateType.NONE,
};

const namespaced = true;

export const laboratory: Module<LaboratoryState, RootState> = {
    namespaced,
    state,
    getters,
    actions,
    mutations,
};
