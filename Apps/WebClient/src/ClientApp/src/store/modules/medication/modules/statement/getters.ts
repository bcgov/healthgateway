import MedicationStatementHistory from "@/models/medicationStatementHistory";
import { LoadStatus } from "@/models/storeOperations";

import { MedicationStatementGetters, MedicationStatementState } from "./types";
import { getMedicationState } from "./util";

export const getters: MedicationStatementGetters = {
    medications(
        state: MedicationStatementState
    ): (hdid: string) => MedicationStatementHistory[] {
        return (hdid: string) => getMedicationState(state, hdid).data;
    },
    medicationsCount(
        state: MedicationStatementState
    ): (hdid: string) => number {
        return (hdid: string) => getMedicationState(state, hdid).data.length;
    },
    medicationsAreLoading(
        state: MedicationStatementState
    ): (hdid: string) => boolean {
        return (hdid: string) =>
            getMedicationState(state, hdid).status === LoadStatus.REQUESTED;
    },
    medicationsAreProtected(
        state: MedicationStatementState
    ): (hdid: string) => boolean {
        return (hdid: string) =>
            getMedicationState(state, hdid).status === LoadStatus.PROTECTED;
    },
    protectiveWordAttempts(
        state: MedicationStatementState
    ): (hdid: string) => number {
        return (hdid: string) =>
            getMedicationState(state, hdid).protectiveWordAttempts;
    },
};
