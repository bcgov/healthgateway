import { Module } from "vuex";

import ExternalConfiguration from "@/models/externalConfiguration";
import { ConfigState, RootState, StateType } from "@/models/storeState";

import { actions } from "./actions";
import { getters } from "./getters";
import { mutations } from "./mutations";

export const state: ConfigState = {
    statusMessage: "",
    config: new ExternalConfiguration(),
    error: false,
    stateType: StateType.NONE,
};

const namespaced = true;

export const config: Module<ConfigState, RootState> = {
    namespaced,
    state,
    getters,
    actions,
    mutations,
};
