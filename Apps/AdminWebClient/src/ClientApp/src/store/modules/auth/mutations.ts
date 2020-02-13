import { MutationTree } from "vuex";
import AuthenticationData from "@/models/authenticationData";
import { AuthState, StateType } from "@/models/storeState";

export const mutations: MutationTree<AuthState> = {
  authenticationRequest(state: AuthState) {
    state.error = false;
    state.statusMessage = "loading";
    state.stateType = StateType.REQUESTED;
  },
  authenticationLoaded(state: AuthState, authData: AuthenticationData) {
    state.error = false;
    state.authentication = authData;
    state.statusMessage = "success";
    state.stateType = StateType.INITIALIZED;
  },
  authenticationError(state: AuthState, errorMessage: string) {
    state.error = true;
    state.authentication = undefined;
    state.statusMessage = errorMessage;
    state.stateType = StateType.ERROR;
  },
  logout(state: AuthState) {
    state.error = false;
    state.statusMessage = "";
    state.authentication = undefined;
    state.stateType = StateType.INITIALIZED;
  }
};
