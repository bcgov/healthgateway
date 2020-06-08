import { Module } from "vuex";
import { getters } from "./getters";
import { actions } from "./actions";
import { mutations } from "./mutations";
import { RootState, StateType, MedicationState } from "@/models/storeState";

export const state: MedicationState = {
  statusMessage: "",
  medications: new Array(),
  error: false,
  stateType: StateType.NONE,
};

const namespaced: boolean = true;

export const medication: Module<MedicationState, RootState> = {
  namespaced,
  state,
  getters,
  actions,
  mutations,
};
