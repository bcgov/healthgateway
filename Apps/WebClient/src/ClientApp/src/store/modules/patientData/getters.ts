import PatientData, { PatientDataFile } from "@/models/patientData";
import { LoadStatus } from "@/models/storeOperations";
import {
    PatientDataGetters,
    PatientDataState,
} from "@/store/modules/patientData/types";
import {
    getPatientDataFileState,
    getPatientDataRecordState,
} from "@/store/modules/patientData/utils";

export const getters: PatientDataGetters = {
    isPatientDataFileLoading(
        state: PatientDataState
    ): (fileId: string) => boolean {
        return (fileId: string) =>
            getPatientDataFileState(state, fileId).status ===
            LoadStatus.REQUESTED;
    },
    patientDataFile(
        state: PatientDataState
    ): (fileId: string) => PatientDataFile | undefined {
        return function (fileId: string) {
            return getPatientDataFileState(state, fileId).data;
        };
    },
    patientData(
        state: PatientDataState
    ): (hdid: string) => PatientData | undefined {
        return (hdid: string) => getPatientDataRecordState(state, hdid).data;
    },
    isPatientDataLoading(state: PatientDataState): (hdid: string) => boolean {
        return (hdid: string) =>
            getPatientDataRecordState(state, hdid).status ===
            LoadStatus.REQUESTED;
    },
};
