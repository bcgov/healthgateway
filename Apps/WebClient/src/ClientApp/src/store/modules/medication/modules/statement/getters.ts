import MedicationStatementHistory from "@/models/medicationStatementHistory";
import { LoadStatus } from "@/models/storeOperations";

import { MedicationStatementGetters, MedicationStatementState } from "./types";
import { getMedicationState } from "./util";

export const getters: MedicationStatementGetters = {
    medicationStatements(
        state: MedicationStatementState
    ): (hdid: string) => MedicationStatementHistory[] {
        return (hdid: string) => getMedicationState(state, hdid).data;
    },
    medicationStatementCount(
        state: MedicationStatementState
    ): (hdid: string) => number {
        return (hdid: string) => getMedicationState(state, hdid).data.length;
    },
    protectedWordAttempts(
        state: MedicationStatementState
    ): (hdid: string) => number {
        return (hdid: string) =>
            getMedicationState(state, hdid).protectiveWordAttempts;
    },
    isProtected(state: MedicationStatementState): (hdid: string) => boolean {
        return (hdid: string) =>
            getMedicationState(state, hdid).status === LoadStatus.PROTECTED;
    },
    isMedicationStatementLoading(
        state: MedicationStatementState
    ): (hdid: string) => boolean {
        return (hdid: string) =>
            getMedicationState(state, hdid).status === LoadStatus.REQUESTED;
    },
};
