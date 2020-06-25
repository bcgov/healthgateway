import { GetterTree } from "vuex";
import { MedicationState, RootState, UserState } from "@/models/storeState";
import MedicationResult from "@/models/medicationResult";

export const getters: GetterTree<MedicationState, RootState> = {
  getStoredMedication: (state: MedicationState) => (
    din: string
  ): MedicationResult | undefined => {
    din = din.padStart(8, "0");
    console.log(din);

    return state.medications.find((item) => item.din === din);
  },
};
