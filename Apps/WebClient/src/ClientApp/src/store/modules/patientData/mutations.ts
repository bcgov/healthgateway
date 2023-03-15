import Vue from "vue";

import { HttpError } from "@/models/errors";
import PatientData, { PatientDataFile } from "@/models/patientData";
import { LoadStatus } from "@/models/storeOperations";
import {
    PatientDataFileState,
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
        payload: { fileId: string; error: HttpError | unknown }
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
        payload: { hdid: string; patientData: PatientData }
    ): void {
        const { hdid, patientData } = payload;
        const currentState = getPatientDataRecordState(state, hdid);
        const nextState: PatientDataRecordState = {
            ...currentState,
            data: patientData,
            error: undefined,
            statusMessage: "success",
            status: LoadStatus.LOADED,
        };
        setPatientDataRecordState(state, hdid, nextState);
    },
    setPatientDataError(
        state: PatientDataState,
        payload: { hdid: string; error: unknown }
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
