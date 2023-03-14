import PatientData from "@/models/patientData";
import { LoadStatus } from "@/models/storeOperations";
import {
    PatientDataGetters,
    PatientDataState,
} from "@/store/modules/patientData/types";
import { getPatientDataRecordState } from "@/store/modules/patientData/utils";

export const getters: PatientDataGetters = {
    patientData(state: PatientDataState): (hdid: string) => PatientData | null {
        return (hdid: string) => getPatientDataRecordState(state, hdid).data;
    },
    isPatientDataLoading(state: PatientDataState): (hdid: string) => boolean {
        return (hdid: string) =>
            getPatientDataRecordState(state, hdid).status ===
            LoadStatus.REQUESTED;
    },
};
