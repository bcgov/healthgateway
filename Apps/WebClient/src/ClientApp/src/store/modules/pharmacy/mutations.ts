import { MutationTree } from "vuex";
import { StateType, PharmacyState } from "@/models/storeState";
import Pharmacy from "@/models/pharmacy";

export const mutations: MutationTree<PharmacyState> = {
  addPharmacyData(state: PharmacyState, pharmacy: Pharmacy) {
    state.pharmacies.push(pharmacy);
    state.error = false;
    state.statusMessage = "success";
    state.stateType = StateType.INITIALIZED;
  },

  pharmacyError(state: PharmacyState, errorMessage: string) {
    state.error = true;
    state.statusMessage = errorMessage;
    state.stateType = StateType.ERROR;
  },
};
