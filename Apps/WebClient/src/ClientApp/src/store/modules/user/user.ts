import { Module } from "vuex";

import PatientData from "@/models/patientData";
import { LoadStatus, RootState, UserState } from "@/models/storeState";
import User from "@/models/user";

import { actions } from "./actions";
import { getters } from "./getters";
import { mutations } from "./mutations";

export const state: UserState = {
    statusMessage: "",
    user: new User(),
    patientData: new PatientData(),
    error: false,
    status: LoadStatus.NONE,
};

const namespaced = true;

export const user: Module<UserState, RootState> = {
    namespaced,
    state,
    getters,
    actions,
    mutations,
};
