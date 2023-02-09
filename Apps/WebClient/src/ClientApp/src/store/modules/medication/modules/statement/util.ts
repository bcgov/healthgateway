import Vue from "vue";

import { MedicationState } from "@/models/datasetState";
import { LoadStatus } from "@/models/storeOperations";

import { MedicationStatementState } from "./types";

export const defaultMedicationState: MedicationState = {
    data: [],
    status: LoadStatus.NONE,
    statusMessage: "",
    error: undefined,
    protectiveWordAttempts: 0,
};

/**
 * Retrieves the hospital visit state for a particular HDID.
 * @param state The store state.
 * @param hdid The HDID associated with the dataset state.
 * @returns The dataset state for the HDID, if it exists, or a new dataset state initialized to default values.
 */
export function getMedicationState(
    state: MedicationStatementState,
    hdid: string
): MedicationState {
    return state.medications[hdid] ?? { ...defaultMedicationState };
}

/**
 * Updates the hospital visit state for a particular HDID.
 * @param state The store state.
 * @param hdid The HDID associated with the dataset state.
 * @param datasetState The new dataset state.
 */
export function setMedicationState(
    state: MedicationStatementState,
    hdid: string,
    datasetState: MedicationState
) {
    Vue.set(state.medications, hdid, datasetState);
}
