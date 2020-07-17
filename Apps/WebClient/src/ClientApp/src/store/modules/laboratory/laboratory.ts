import { Module } from "vuex";
import { getters } from "./getters";
import { actions } from "./actions";
import { mutations } from "./mutations";
import { LaboratoryState, RootState, StateType } from "@/models/storeState";

export const state: LaboratoryState = {
    statusMessage: "",
    laboratoryOrders: new Array(),
    error: false,
    stateType: StateType.NONE,
};

const namespaced: boolean = true;

export const laboratory: Module<LaboratoryState, RootState> = {
    namespaced,
    state,
    getters,
    actions,
    mutations,
};
