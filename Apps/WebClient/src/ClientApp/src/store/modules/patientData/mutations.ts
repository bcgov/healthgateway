import Vue from "vue";

import { ResultError } from "@/models/errors";
import PatientDataResponse, {
    PatientDataFile,
    PatientDataToHealthDataTypeMap,
    PatientDataType,
} from "@/models/patientDataResponse";
import { LoadStatus } from "@/models/storeOperations";
import {
    PatientDataFileState,
    PatientDataMap,
    PatientDataMutations,
    PatientDataRecordState,
    PatientDataState,
} from "@/store/modules/patientData/types";
import {
    getPatientDataRecordState,
    setPatientDataRecordState,
} from "@/store/modules/patientData/utils";

export const mutations: PatientDataMutations = {
    setPatientDataFile(
        state: PatientDataState,
        payload: { fileId: string; file: PatientDataFile }
    ): void {
        const fileState: PatientDataFileState = {
            data: payload.file,
            error: undefined,
            status: LoadStatus.LOADED,
            statusMessage: "success",
        };
        Vue.set(state.patientDataFiles, payload.fileId, fileState);
    },
    setPatientDataFileError(
        state: PatientDataState,
        payload: { fileId: string; error: ResultError }
    ): void {
        const fileState: PatientDataFileState = {
            data: undefined,
            status: LoadStatus.ERROR,
            statusMessage: "File Request Error",
            error: payload.error,
        };
        Vue.set(state.patientDataFiles, payload.fileId, fileState);
    },
    setPatientDataFileRequested(state: PatientDataState, fileId: string): void {
        const fileState: PatientDataFileState = {
            data: undefined,
            status: LoadStatus.REQUESTED,
            statusMessage: "File Requested",
            error: undefined,
        };
        Vue.set(state.patientDataFiles, fileId, fileState);
    },
    setPatientData(
        state: PatientDataState,
        payload: {
            hdid: string;
            patientData: PatientDataResponse;
            patientDataTypes: PatientDataType[];
        }
    ): void {
        const { hdid, patientData, patientDataTypes } = payload;

        const currentState = getPatientDataRecordState(state, hdid);
        const dataToUpdate: PatientDataMap =
            currentState.data ?? ({} as PatientDataMap);

        patientDataTypes.forEach((patientDataType) => {
            dataToUpdate[patientDataType] = patientData.items.filter(
                (i) =>
                    i.type ===
                    PatientDataToHealthDataTypeMap.get(patientDataType)
            );
        });

        const nextState: PatientDataRecordState = {
            ...currentState,
            data: dataToUpdate,
            error: undefined,
            statusMessage: "success",
            status: LoadStatus.LOADED,
        };
        setPatientDataRecordState(state, hdid, nextState);
    },
    setPatientDataError(
        state: PatientDataState,
        payload: {
            hdid: string;
            error: ResultError;
        }
    ): void {
        const { hdid, error } = payload;
        const currentState = getPatientDataRecordState(state, hdid);
        const nextState: PatientDataRecordState = {
            ...currentState,
            error: error,
            statusMessage: "error",
            status: LoadStatus.ERROR,
        };
        setPatientDataRecordState(state, hdid, nextState);
    },
    setPatientDataRequested(state: PatientDataState, hdid: string): void {
        const currentState = getPatientDataRecordState(state, hdid);
        const nextState: PatientDataRecordState = {
            ...currentState,
            status: LoadStatus.REQUESTED,
        };
        setPatientDataRecordState(state, hdid, nextState);
    },
};
