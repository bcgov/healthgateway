import { Module } from "vuex";
import { getters } from "./getters";
import { actions } from "./actions";
import { mutations } from "./mutations";
import { RootState, SidebarState } from "@/models/storeState";

export const state: SidebarState = {
  isOpen: false,
};

const namespaced: boolean = true;

export const sidebar: Module<SidebarState, RootState> = {
  namespaced,
  state,
  getters,
  actions,
  mutations,
};
