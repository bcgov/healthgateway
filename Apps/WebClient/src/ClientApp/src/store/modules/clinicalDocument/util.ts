import Vue from "vue";

import { ClinicalDocumentDatasetState } from "@/models/datasetState";
import { LoadStatus } from "@/models/storeOperations";

import { ClinicalDocumentState } from "./types";

export const defaultClinicalDocumentDatasetState: ClinicalDocumentDatasetState =
    {
        data: [],
        statusMessage: "",
        status: LoadStatus.NONE,
        error: undefined,
    };

/**
 * Retrieves the clinical document dataset state for a particular HDID.
 * @param state The store state.
 * @param hdid The HDID associated with the dataset state.
 * @returns The dataset state for the HDID, if it exists, or a new dataset state initialized to default values.
 */
export function getClinicalDocumentDatasetState(
    state: ClinicalDocumentState,
    hdid: string
): ClinicalDocumentDatasetState {
    return (
        state.clinicalDocuments[hdid] ?? {
            ...defaultClinicalDocumentDatasetState,
        }
    );
}

/**
 * Updates the clinical document dataset state for a particular HDID.
 * @param state The store state.
 * @param hdid The HDID associated with the dataset state.
 * @param datasetState The new dataset state.
 */
export function setClinicalDocumentDatasetState(
    state: ClinicalDocumentState,
    hdid: string,
    datasetState: ClinicalDocumentDatasetState
) {
    Vue.set(state.clinicalDocuments, hdid, datasetState);
}
