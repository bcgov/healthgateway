import { Module } from "vuex";
import { getters } from "./getters";
import { actions } from "./actions";
import { mutations } from "./mutations";
import { RootState, StateType, UserState } from "@/models/storeState";
import User from "@/models/user";

export const state: UserState = {
    statusMessage: "",
    user: new User(),
    error: false,
    stateType: StateType.NONE,
};

const namespaced = true;

export const user: Module<UserState, RootState> = {
    namespaced,
    state,
    getters,
    actions,
    mutations,
};
