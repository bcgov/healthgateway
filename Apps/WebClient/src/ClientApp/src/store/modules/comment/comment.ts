import { Module } from "vuex";

import { CommentState, LoadStatus, RootState } from "@/models/storeState";

import { actions } from "./actions";
import { getters } from "./getters";
import { mutations } from "./mutations";

export const state: CommentState = {
    statusMessage: "",
    profileComments: {},
    error: undefined,
    status: LoadStatus.NONE,
};

const namespaced = true;

export const comment: Module<CommentState, RootState> = {
    namespaced,
    state,
    getters,
    actions,
    mutations,
};
