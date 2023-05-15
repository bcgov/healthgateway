import Vue from "vue";

import { LoadStatus } from "@/models/storeOperations";
import VaccineRecordState from "@/models/vaccineRecordState";

import { VaccinationStatusState } from "./types";

export const defaultVaccineRecordState: VaccineRecordState = {
    hdid: "",
    download: false,
    status: LoadStatus.NONE,
    statusMessage: "",
    resultMessage: "",
};

/**
 * Retrieves the vaccine record state for a particular HDID.
 * @param state The store state.
 * @param hdid The HDID associated with the vaccine record state.
 * @returns The vaccine record state for the HDID, if it exists, or a new vaccine record state initialized to default values.
 */
export function getAuthenticatedVaccineRecordState(
    state: VaccinationStatusState,
    hdid: string
): VaccineRecordState {
    return (
        state.authenticatedVaccineRecordStates[hdid] ?? {
            ...defaultVaccineRecordState,
            hdid,
        }
    );
}

/**
 * Updates the vaccine record state for a particular HDID.
 * @param state The store state.
 * @param hdid The HDID associated with the vaccine record state.
 * @param vaccineRecordState The new vaccine record state.
 */
export function setAuthenticatedVaccineRecordState(
    state: VaccinationStatusState,
    hdid: string,
    vaccineRecordState: VaccineRecordState
) {
    Vue.set(state.authenticatedVaccineRecordStates, hdid, vaccineRecordState);
}
