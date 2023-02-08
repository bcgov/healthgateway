import Vue from "vue";

import { DatasetState, HospitalVisitState } from "@/models/datasetState";
import { Encounter } from "@/models/encounter";
import { LoadStatus } from "@/models/storeOperations";

import { EncounterState } from "./types";

export const defaultEncounterState: DatasetState<Encounter[]> = {
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
 * Retrieves the encounter state for a particular HDID.
 * @param state The store state.
 * @param hdid The HDID associated with the dataset state.
 * @returns The dataset state for the HDID, if it exists, or a new dataset state initialized to default values.
 */
export function getEncounterState(
    state: EncounterState,
    hdid: string
): DatasetState<Encounter[]> {
    return state.encounters[hdid] ?? { ...defaultEncounterState };
}

/**
 * Updates the encounter state for a particular HDID.
 * @param state The store state.
 * @param hdid The HDID associated with the dataset state.
 * @param datasetState The new dataset state.
 */
export function setEncounterState(
    state: EncounterState,
    hdid: string,
    datasetState: DatasetState<Encounter[]>
) {
    Vue.set(state.encounters, hdid, datasetState);
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
