import { Module } from "vuex";

import { ExternalConfiguration } from "@/models/configData";
import { ConfigState, LoadStatus, RootState } from "@/models/storeState";

import { actions } from "./actions";
import { getters } from "./getters";
import { mutations } from "./mutations";

export const state: ConfigState = {
    statusMessage: "",
    config: new ExternalConfiguration(),
    error: false,
    status: LoadStatus.NONE,
};

const namespaced = true;

export const config: Module<ConfigState, RootState> = {
    namespaced,
    state,
    getters,
    actions,
    mutations,
};
