import { Module } from "vuex";

import {
    LoadStatus,
    MedicationStatementState,
    RootState,
} from "@/models/storeState";

import { actions } from "./actions";
import { getters } from "./getters";
import { mutations } from "./mutations";

export const state: MedicationStatementState = {
    medicationStatements: [],
    protectiveWordAttempts: 0,
    status: LoadStatus.NONE,
    error: undefined,
    statusMessage: "",
};

export const statement: Module<MedicationStatementState, RootState> = {
    state,
    getters,
    actions,
    mutations,
};
