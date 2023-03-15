import { Dictionary } from "@/models/baseTypes";
import PatientData from "@/models/patientData";
import { LoadStatus } from "@/models/storeOperations";
import {
    PatientDataFileState,
    PatientDataGetters,
    PatientDataState,
} from "@/store/modules/patientData/types";
import { getPatientDataRecordState } from "@/store/modules/patientData/utils";

export const getters: PatientDataGetters = {
    patientDataFiles(
        state: PatientDataState
    ): Dictionary<PatientDataFileState> {
        return state.patientDataFiles;
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
