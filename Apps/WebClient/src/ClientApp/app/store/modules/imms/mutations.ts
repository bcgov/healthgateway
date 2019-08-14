import { MutationTree } from "vuex";
import { AuthState } from "@/models/authState";
import { ImmsState } from "@/models/immsState";
import { StateType } from "@/models/rootState";
import ImmsData from "@/models/immsData";

export const mutations: MutationTree<AuthState> = {
  itemsRequest(state: ImmsState) {
    state.error = false;
    state.statusMessage = "loading";
    state.stateType = StateType.REQUESTED;
  },
  immsItemsLoaded(state: ImmsState, data: ImmsData[]) {
    state.error = false;
    state.items = data;
    state.statusMessage = "success";
    state.stateType = StateType.INITIALIZED;
  },
  immsError(state: ImmsState, errorMessage: string) {
    state.error = true;
    state.items = undefined;
    state.statusMessage = errorMessage;
    state.stateType = StateType.ERROR;
  }
};
