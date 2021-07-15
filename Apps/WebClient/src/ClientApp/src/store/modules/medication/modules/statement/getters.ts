import MedicationStatementHistory from "@/models/medicationStatementHistory";
import { LoadStatus } from "@/models/storeOperations";

import { MedicationStatementGetters, MedicationStatementState } from "./types";

export const getters: MedicationStatementGetters = {
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
