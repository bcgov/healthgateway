import { Module } from "vuex";

import { LoadStatus, RootState, UserState } from "@/models/storeState";
import User from "@/models/user";

import { actions } from "./actions";
import { getters } from "./getters";
import { mutations } from "./mutations";

export const state: UserState = {
    statusMessage: "",
    user: new User(),
    error: false,
    status: LoadStatus.LOADED,
};

const namespaced = true;

export const user: Module<UserState, RootState> = {
    namespaced,
    state,
    getters,
    actions,
    mutations,
};
