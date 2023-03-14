import PatientData from "@/models/patientData";
import { LoadStatus } from "@/models/storeOperations";
import {
    PatientDataMutations,
    PatientDataRecordState,
    PatientDataState,
} from "@/store/modules/patientData/types";
import {
    getPatientDataRecordState,
    setPatientDataRecordState,
} from "@/store/modules/patientData/utils";

export const mutations: PatientDataMutations = {
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
            statusMessage: "success",
            status: LoadStatus.LOADED,
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
