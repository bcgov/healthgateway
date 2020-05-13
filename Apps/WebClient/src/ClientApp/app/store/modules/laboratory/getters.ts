import { GetterTree } from "vuex";
import { RootState, LaboratoryState } from "@/models/storeState";
import { LaboratoryOrder } from "@/models/laboratory";

export const getters: GetterTree<LaboratoryState, RootState> = {
  getStoredLaboratoryOrders: (
    state: LaboratoryState
  ) => (): LaboratoryOrder[] => {
    return state.laboratoryOrders;
  }
};
