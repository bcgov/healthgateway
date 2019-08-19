import { MutationTree } from "vuex";
import { ImmsState, StateType } from "@/models/storeState";
import ImmsData from "@/models/immsData";

export const mutations: MutationTree<ImmsState> = {
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
