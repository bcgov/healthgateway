import { Module } from "vuex";
import { getters } from "./getters";
import { actions } from "./actions";
import { mutations } from "./mutations";
import { RootState, StateType } from "@/models/rootState";
import { ExternalConfiguration } from "@/models/ConfigData";
import { ConfigState } from "@/models/configState";

export const state: ConfigState = {
  statusMessage: "",
  config: new ExternalConfiguration(),
  error: false,
  stateType: StateType.NONE
};

const namespaced: boolean = true;

export const auth: Module<ConfigState, RootState> = {
  namespaced,
  state,
  getters,
  actions,
  mutations
};
