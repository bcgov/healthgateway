import { Module } from "vuex";

import { LoadStatus, MedicationState, RootState } from "@/models/storeState";

import { request } from "./modules/request/request";
import { statement } from "./modules/statement/statement";

export const state: MedicationState = {
    status: LoadStatus.NONE,
};

const namespaced = true;

export const medication: Module<MedicationState, RootState> = {
    namespaced,
    state,
    modules: { statement, request },
};
