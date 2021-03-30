import { GetterTree } from "vuex";

import MedicationStatementHistory from "@/models/medicationStatementHistory";
import {
    LoadStatus,
    MedicationStatementState,
    RootState,
} from "@/models/storeState";

export const getters: GetterTree<MedicationStatementState, RootState> = {
    medicationStatements(
        state: MedicationStatementState
    ): MedicationStatementHistory[] {
        return state.medicationStatements;
    },
    medicationStatementCount(state: MedicationStatementState): number {
        return state.medicationStatements.length;
    },
    protectedWordAttempts(state: MedicationStatementState): number {
        return state.protectiveWordAttempts;
    },
    isProtected(state: MedicationStatementState): boolean {
        return state.status === LoadStatus.PROTECTED;
    },
    isMedicationStatementLoading(state: MedicationStatementState): boolean {
        return state.status === LoadStatus.REQUESTED;
    },
};
