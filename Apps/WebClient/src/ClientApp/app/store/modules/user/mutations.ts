import Vue from "vue";
import { MutationTree } from "vuex";
import { StateType, UserState } from "@/models/storeState";
import PatientData from "@/models/patientData";
import { User as OidcUser } from "oidc-client";
import User from "@/models/user";

export const mutations: MutationTree<UserState> = {
  setOidcUserData(state: UserState, oidcUser: OidcUser) {
    Vue.set(state.user, "firstName", oidcUser.profile.given_name);
    Vue.set(state.user, "lastName", oidcUser.profile.family_name);
    Vue.set(state.user, "email", oidcUser.profile.email);
    Vue.set(state.user, "hdid", oidcUser.profile.hdid);
    state.error = false;
    state.statusMessage = "success";
    state.stateType = StateType.INITIALIZED;
  },
  setPatientData(state: UserState, patientData: PatientData) {
    Vue.set(state.user, "phn", patientData.personalHealthNumber);
    state.error = false;
    state.statusMessage = "success";
    state.stateType = StateType.INITIALIZED;
  },
  clearUserData(state: UserState) {
    state.user = new User();
    state.error = false;
    state.statusMessage = "success";
    state.stateType = StateType.INITIALIZED;
  },
  userError(state: UserState, errorMessage: string) {
    state.error = true;
    state.statusMessage = errorMessage;
    state.stateType = StateType.ERROR;
  }
};
