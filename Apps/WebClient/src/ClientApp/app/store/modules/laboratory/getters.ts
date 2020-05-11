import { GetterTree } from "vuex";
import { RootState, LaboratoryState } from "@/models/storeState";
import { LaboratoryReport } from "@/models/laboratory";

export const getters: GetterTree<LaboratoryState, RootState> = {
  getStoredLaboratoryReports: (
    state: LaboratoryState
  ) => (): LaboratoryReport[] => {
    return state.laboratoryReports;
  }
};
