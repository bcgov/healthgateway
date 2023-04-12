import Vue from "vue";

import { PatientDataType } from "@/models/patientDataResponse";
import { LoadStatus } from "@/models/storeOperations";
import {
    PatientDataFileState,
    PatientDataRecordState,
    PatientDataState,
} from "@/store/modules/patientData/types";

const defaultPatientDataState: PatientDataRecordState = {
    data: undefined,
    statusMessage: "",
    status: LoadStatus.NONE,
    error: undefined,
};

const defaultPatientDataFileState: PatientDataFileState = {
    data: undefined,
    statusMessage: "",
    status: LoadStatus.NONE,
    error: undefined,
};

/**
 * Retrieves the patient's data state for a particular HDID.
 * @param state The store state.
 * @param hdid The HDID associated with the patient's data state.
 * @returns the patient's data state for the HDID, if it exists, or a new state initialized to default values.
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
export function areAllPatientDataTypesStored(
    state: PatientDataState,
    hdid: string,
    patientDataTypes: PatientDataType[]
): boolean {
    const patientDataRecordState = getPatientDataRecordState(state, hdid);
    if (!patientDataRecordState.data) {
        return false;
    }
    for (const patientDataType of patientDataTypes) {
        if (!patientDataRecordState.data[patientDataType]) {
            return false;
        }
    }
    return true;
}

/**
 * Update the patient's data state for a particular HDID.
 * @param state The store state.
 * @param hdid The HDID associated with the patient's data state.
 * @param patientDataRecordState The patient's data state.
 */
export function setPatientDataRecordState(
    state: PatientDataState,
    hdid: string,
    patientDataRecordState: PatientDataRecordState
): void {
    Vue.set(state.patientDataRecords, hdid, patientDataRecordState);
}

/**
 * Retrieves the patient's data files state for a particular file id.
 * @param state The store state.
 * @param fileId The file id associated with the patient's data files state.
 * @returns the patient data file state, if it exists, or a new state initialized to default values.
 */
export function getPatientDataFileState(
    state: PatientDataState,
    fileId: string
): PatientDataFileState {
    return (
        state.patientDataFiles[fileId] ?? {
            ...defaultPatientDataFileState,
        }
    );
}
