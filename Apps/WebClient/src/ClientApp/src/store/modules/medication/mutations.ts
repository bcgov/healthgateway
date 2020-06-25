import { MutationTree } from "vuex";
import { MedicationState, StateType } from "@/models/storeState";
import MedicationResult from "@/models/medicationResult";

export const mutations: MutationTree<MedicationState> = {
    addMedicationData(
        state: MedicationState,
        medicationResult: MedicationResult
    ) {
        console.log(medicationResult);
        state.medications.push(medicationResult);
        state.error = false;
        state.statusMessage = "success";
        state.stateType = StateType.INITIALIZED;
    },

    medicationError(state: MedicationState, errorMessage: string) {
        state.error = true;
        state.statusMessage = errorMessage;
        state.stateType = StateType.ERROR;
    },
};
