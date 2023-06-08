import Vue from "vue";

import { SpecialAuthorityRequestState } from "@/models/datasetState";
import { LoadStatus } from "@/models/storeOperations";

import { MedicationRequestState } from "./types";

export const defaultSpecialAuthorityRequestState: SpecialAuthorityRequestState =
    {
        data: [],
        status: LoadStatus.NONE,
        statusMessage: "",
        error: undefined,
    };

/**
 * Retrieves the Special Authority request state for a particular HDID.
 * @param state The store state.
 * @param hdid The HDID associated with the dataset state.
 * @returns The dataset state for the HDID, if it exists, or a new dataset state initialized to default values.
 */
export function getSpecialAuthorityRequestState(
    state: MedicationRequestState,
    hdid: string
): SpecialAuthorityRequestState {
    return (
        state.specialAuthorityRequests[hdid] ?? {
            ...defaultSpecialAuthorityRequestState,
        }
    );
}

/**
 * Updates the Special Authority request state for a particular HDID.
 * @param state The store state.
 * @param hdid The HDID associated with the dataset state.
 * @param datasetState The new dataset state.
 */
export function setSpecialAuthorityRequestState(
    state: MedicationRequestState,
    hdid: string,
    datasetState: SpecialAuthorityRequestState
) {
    Vue.set(state.specialAuthorityRequests, hdid, datasetState);
}
