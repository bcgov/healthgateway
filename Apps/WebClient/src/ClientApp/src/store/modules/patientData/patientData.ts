import { actions } from "@/store/modules/patientData/actions";
import { getters } from "@/store/modules/patientData/getters";
import { mutations } from "@/store/modules/patientData/mutations";
import {
    PatientDataModule,
    PatientDataState,
} from "@/store/modules/patientData/types";

const state: PatientDataState = {
    patientDataFiles: {},
    patientDataRecords: {},
};

const namespaced = true;

export const patientData: PatientDataModule = {
    namespaced,
    state,
    getters,
    actions,
    mutations,
};
