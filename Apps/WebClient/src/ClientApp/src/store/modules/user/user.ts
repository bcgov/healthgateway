import { Module } from "vuex";

import { RootState, StateType, UserState } from "@/models/storeState";
import User from "@/models/user";

import { actions } from "./actions";
import { getters } from "./getters";
import { mutations } from "./mutations";

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
