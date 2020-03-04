import { Module } from "vuex";
import { getters } from "./getters";
import { actions } from "./actions";
import { mutations } from "./mutations";
import { RootState, StateType, ConfigState } from "@/models/storeState";
import ExternalConfiguration from "@/models/externalConfiguration";

export const state: ConfigState = {
  statusMessage: "",
  config: new ExternalConfiguration(),
  error: false,
  stateType: StateType.NONE
};

const namespaced: boolean = true;

export const config: Module<ConfigState, RootState> = {
  namespaced,
  state,
  getters,
  actions,
  mutations
};
