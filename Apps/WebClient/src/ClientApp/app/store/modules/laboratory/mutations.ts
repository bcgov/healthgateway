import { MutationTree } from "vuex";
import { LaboratoryState, StateType } from "@/models/storeState";
import { LaboratoryResult, LaboratoryReport } from "@/models/laboratory";

export const mutations: MutationTree<LaboratoryState> = {
  setLaboratoryReports(
    state: LaboratoryState,
    laboratoryReports: LaboratoryReport[]
  ) {
    state.laboratoryReports = laboratoryReports;
    state.error = false;
    state.statusMessage = "success";
    state.stateType = StateType.INITIALIZED;
  },

  laboratoryError(state: LaboratoryState, errorMessage: string) {
    state.error = true;
    state.statusMessage = errorMessage;
    state.stateType = StateType.ERROR;
  }
};
