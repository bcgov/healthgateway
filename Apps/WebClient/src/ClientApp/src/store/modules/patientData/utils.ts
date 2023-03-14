import Vue from "vue";

import { LoadStatus } from "@/models/storeOperations";
import {
    PatientDataRecordState,
    PatientDataState,
} from "@/store/modules/patientData/types";

const defaultPatientDataState: PatientDataRecordState = {
    data: null,
    statusMessage: "",
    status: LoadStatus.NONE,
    // TODO: Error mapping here.
};

/**
 * Retrieves the patient's data state for a particular HDID.
 * @param state The store state.
 * @param hdid The HDID associated with the patient's data state.
 * @returns the health options state for the HDID, if it exists, or a new state initialized to default values.
 */
export function getPatientDataRecordState(
    state: PatientDataState,
    hdid: string
): PatientDataRecordState {
    return (
        state.patientDataRecords[hdid] ?? {
            ...defaultPatientDataState,
        }
    );
}

/**
 * Update the patient's data state for a particular HDID.
 * @param state The store state.
 * @param hdid The HDID associated with the patient's data state.
 * @param patientDataRecordState The patient's health options state.
 */
export function setPatientDataRecordState(
    state: PatientDataState,
    hdid: string,
    patientDataRecordState: PatientDataRecordState
): void {
    Vue.set(state.patientDataRecords, hdid, patientDataRecordState);
}
