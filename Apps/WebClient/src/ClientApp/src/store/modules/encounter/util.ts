import Vue from "vue";

import { HealthVisitState, HospitalVisitState } from "@/models/datasetState";
import { LoadStatus } from "@/models/storeOperations";

import { EncounterState } from "./types";

export const defaultHealthVisitState: HealthVisitState = {
    data: [],
    status: LoadStatus.NONE,
    statusMessage: "",
    error: undefined,
};

export const defaultHospitalVisitState: HospitalVisitState = {
    data: [],
    status: LoadStatus.NONE,
    statusMessage: "",
    error: undefined,
    queued: false,
};

/**
 * Retrieves the health visit state for a particular HDID.
 * @param state The store state.
 * @param hdid The HDID associated with the dataset state.
 * @returns The dataset state for the HDID, if it exists, or a new dataset state initialized to default values.
 */
export function getHealthVisitState(
    state: EncounterState,
    hdid: string
): HealthVisitState {
    return state.healthVisits[hdid] ?? { ...defaultHealthVisitState };
}

/**
 * Updates the health visit state for a particular HDID.
 * @param state The store state.
 * @param hdid The HDID associated with the dataset state.
 * @param datasetState The new dataset state.
 */
export function setHealthVisitState(
    state: EncounterState,
    hdid: string,
    datasetState: HealthVisitState
) {
    Vue.set(state.healthVisits, hdid, datasetState);
}

/**
 * Retrieves the hospital visit state for a particular HDID.
 * @param state The store state.
 * @param hdid The HDID associated with the dataset state.
 * @returns The dataset state for the HDID, if it exists, or a new dataset state initialized to default values.
 */
export function getHospitalVisitState(
    state: EncounterState,
    hdid: string
): HospitalVisitState {
    return state.hospitalVisits[hdid] ?? { ...defaultHospitalVisitState };
}

/**
 * Updates the hospital visit state for a particular HDID.
 * @param state The store state.
 * @param hdid The HDID associated with the dataset state.
 * @param datasetState The new dataset state.
 */
export function setHospitalVisitState(
    state: EncounterState,
    hdid: string,
    datasetState: HospitalVisitState
) {
    Vue.set(state.hospitalVisits, hdid, datasetState);
}
