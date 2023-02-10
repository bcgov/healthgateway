import Vue from "vue";

import { ImmunizationDatasetState } from "@/models/datasetState";
import { LoadStatus } from "@/models/storeOperations";

import { ImmunizationState } from "./types";

export const defaultImmunizationDatasetState: ImmunizationDatasetState = {
    data: [],
    status: LoadStatus.NONE,
    statusMessage: "",
    error: undefined,
    recommendations: [],
};

/**
 * Retrieves the immunization dataset state for a particular HDID.
 * @param state The store state.
 * @param hdid The HDID associated with the dataset state.
 * @returns The dataset state for the HDID, if it exists, or a new dataset state initialized to default values.
 */
export function getImmunizationDatasetState(
    state: ImmunizationState,
    hdid: string
): ImmunizationDatasetState {
    return state.immunizations[hdid] ?? { ...defaultImmunizationDatasetState };
}

/**
 * Updates the immunization state for a particular HDID.
 * @param state The store state.
 * @param hdid The HDID associated with the dataset state.
 * @param datasetState The new dataset state.
 */
export function setImmunizationDatasetState(
    state: ImmunizationState,
    hdid: string,
    datasetState: ImmunizationDatasetState
) {
    Vue.set(state.immunizations, hdid, datasetState);
}
