import Vue from "vue";

import { Covid19TestResultState, LabResultState } from "@/models/datasetState";
import { LoadStatus } from "@/models/storeOperations";

import { LaboratoryState } from "./types";

export const defaultLabResultState: LabResultState = {
    data: [],
    status: LoadStatus.NONE,
    statusMessage: "",
    error: undefined,
    queued: false,
};

export const defaultCovid19TestResultState: Covid19TestResultState = {
    data: [],
    status: LoadStatus.NONE,
    statusMessage: "",
    error: undefined,
};

/**
 * Retrieves the lab result state for a particular HDID.
 * @param state The store state.
 * @param hdid The HDID associated with the dataset state.
 * @returns The dataset state for the HDID, if it exists, or a new dataset state initialized to default values.
 */
export function getLabResultState(
    state: LaboratoryState,
    hdid: string
): LabResultState {
    return state.labResults[hdid] ?? { ...defaultLabResultState };
}

/**
 * Updates the lab result state for a particular HDID.
 * @param state The store state.
 * @param hdid The HDID associated with the dataset state.
 * @param datasetState The new dataset state.
 */
export function setLabResultState(
    state: LaboratoryState,
    hdid: string,
    datasetState: LabResultState
) {
    Vue.set(state.labResults, hdid, datasetState);
}

/**
 * Retrieves the COVID-19 test result state for a particular HDID.
 * @param state The store state.
 * @param hdid The HDID associated with the dataset state.
 * @returns The dataset state for the HDID, if it exists, or a new dataset state initialized to default values.
 */
export function getCovid19TestResultState(
    state: LaboratoryState,
    hdid: string
): Covid19TestResultState {
    return (
        state.covid19TestResults[hdid] ?? { ...defaultCovid19TestResultState }
    );
}

/**
 * Updates the COVID-19 test result state for a particular HDID.
 * @param state The store state.
 * @param hdid The HDID associated with the dataset state.
 * @param datasetState The new dataset state.
 */
export function setCovid19TestResultState(
    state: LaboratoryState,
    hdid: string,
    datasetState: Covid19TestResultState
) {
    Vue.set(state.covid19TestResults, hdid, datasetState);
}
