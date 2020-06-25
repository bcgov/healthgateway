import { MutationTree } from "vuex";
import { LaboratoryState, StateType } from "@/models/storeState";
import {
  LaboratoryOrder,
  LaboratoryReport,
  LaboratoryResult,
} from "@/models/laboratory";

export const mutations: MutationTree<LaboratoryState> = {
  setLaboratoryOrders(
    state: LaboratoryState,
    laboratoryOrders: LaboratoryOrder[]
  ) {
    state.laboratoryOrders = laboratoryOrders;
    state.error = false;
    state.statusMessage = "success";
    state.stateType = StateType.INITIALIZED;
  },

  laboratoryError(state: LaboratoryState, errorMessage: string) {
    state.error = true;
    state.statusMessage = errorMessage;
    state.stateType = StateType.ERROR;
  },
};
