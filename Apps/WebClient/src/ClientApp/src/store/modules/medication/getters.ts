import { GetterTree } from "vuex";

import MedicationResult from "@/models/medicationResult";
import MedicationStatementHistory from "@/models/medicationStatementHistory";
import { LoadStatus, MedicationState, RootState } from "@/models/storeState";

export const getters: GetterTree<MedicationState, RootState> = {
    medicationStatements(state: MedicationState): MedicationStatementHistory[] {
        return state.medicationStatements;
    },
    medicationInformation: (state: MedicationState) => (
        din: string
    ): MedicationResult | undefined => {
        din = din.padStart(8, "0");

        return state.medications.find((item) => item.din === din);
    },
    protectedWordAttempts(state: MedicationState): number {
        return state.protectiveWordAttempts;
    },
    isProtected(state: MedicationState): boolean {
        return state.status === LoadStatus.PROTECTED;
    },
    isLoading(state: MedicationState): boolean {
        return state.status === LoadStatus.REQUESTED;
    },
};
