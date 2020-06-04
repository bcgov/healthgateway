import { Module } from "vuex";
import { getters } from "./getters";
import { actions } from "./actions";
import { mutations } from "./mutations";
import { RootState, StateType, PharmacyState } from "@/models/storeState";

export const state: PharmacyState = {
  statusMessage: "",
  pharmacies: new Array(),
  error: false,
  stateType: StateType.NONE
};

const namespaced: boolean = true;

export const pharmacy: Module<PharmacyState, RootState> = {
  namespaced,
  state,
  getters,
  actions,
  mutations
};
