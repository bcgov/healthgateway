import { Module } from "vuex";

import PatientData from "@/models/patientData";
import { LoadStatus, PatientState, RootState } from "@/models/storeState";

import { actions } from "./actions";
import { getters } from "./getters";
import { mutations } from "./mutations";

export const state: PatientState = {
    statusMessage: "",
    patientData: new PatientData(),
    error: undefined,
    status: LoadStatus.NONE,
};

const namespaced = true;

export const patient: Module<PatientState, RootState> = {
    namespaced,
    state,
    getters,
    actions,
    mutations,
};
