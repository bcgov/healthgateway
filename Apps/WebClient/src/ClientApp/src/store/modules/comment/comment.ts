import { Module } from "vuex";

import { CommentState, RootState, StateType } from "@/models/storeState";

import { actions } from "./actions";
import { getters } from "./getters";
import { mutations } from "./mutations";

export const state: CommentState = {
    statusMessage: "",
    profileComments: {},
    error: false,
    stateType: StateType.NONE,
};

const namespaced = true;

export const comment: Module<CommentState, RootState> = {
    namespaced,
    state,
    getters,
    actions,
    mutations,
};
