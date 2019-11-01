import { GetterTree } from "vuex";
import { RootState, PharmacyState } from "@/models/storeState";
import Pharmacy from "@/models/pharmacy";

export const getters: GetterTree<PharmacyState, RootState> = {
  getStoredPharmacy: (state: PharmacyState) => (
    pharmacyId: string
  ): Pharmacy | undefined => {
    return state.pharmacies.find(item => item.pharmacyId === pharmacyId);
  }
};
