import { Module } from "vuex";
import { getters } from "./getters";
import { actions } from "./actions";
import { mutations } from "./mutations";
import { CommentState, RootState, StateType } from "@/models/storeState";

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
