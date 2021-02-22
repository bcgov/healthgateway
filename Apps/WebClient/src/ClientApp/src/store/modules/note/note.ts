import { Module } from "vuex";

import { LoadStatus, NoteState, RootState } from "@/models/storeState";

import { actions } from "./actions";
import { getters } from "./getters";
import { mutations } from "./mutations";

export const state: NoteState = {
    notes: [],
    status: LoadStatus.NONE,
    error: undefined,
    statusMessage: "",
};

const namespaced = true;

export const note: Module<NoteState, RootState> = {
    namespaced,
    state,
    getters,
    actions,
    mutations,
};
