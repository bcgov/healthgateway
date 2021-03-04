import { GetterTree } from "vuex";

import PatientData from "@/models/patientData";
import { LoadStatus, PatientState, RootState } from "@/models/storeState";

export const getters: GetterTree<PatientState, RootState> = {
    patientData(state: PatientState): PatientData {
        return state.patientData;
    },
    isLoading(state: PatientState): boolean {
        return state.status === LoadStatus.REQUESTED;
    },
};
