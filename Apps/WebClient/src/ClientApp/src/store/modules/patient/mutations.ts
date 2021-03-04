import { MutationTree } from "vuex";

import PatientData from "@/models/patientData";
import { LoadStatus, PatientState } from "@/models/storeState";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { ILogger } from "@/services/interfaces";

const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);

export const mutations: MutationTree<PatientState> = {
    setRequested(state: PatientState) {
        state.status = LoadStatus.REQUESTED;
    },
    setPatientData(state: PatientState, patientData: PatientData) {
        logger.verbose(`PatientState:setPatientData`);
        state.patientData = patientData;
        state.error = undefined;
        state.statusMessage = "success";
        state.status = LoadStatus.LOADED;
    },
    patientError(state: PatientState, error: Error) {
        state.statusMessage = error.message;
        state.status = LoadStatus.ERROR;
    },
};
