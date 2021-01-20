import { MutationTree } from "vuex";

import MedicationResult from "@/models/medicationResult";
import MedicationStatementHistory from "@/models/medicationStatementHistory";
import { MedicationState, StateType } from "@/models/storeState";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { ILogger } from "@/services/interfaces";
const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);

export const mutations: MutationTree<MedicationState> = {
    setMedicationStatements(
        state: MedicationState,
        medicationStatements: MedicationStatementHistory[]
    ) {
        state.medicationStatements = medicationStatements;
        state.error = false;
        state.statusMessage = "success";
        state.stateType = StateType.INITIALIZED;
    },
    addMedicationInformation(
        state: MedicationState,
        medicationResult: MedicationResult
    ) {
        logger.debug(
            `addMedicationInformation medicationResult: ${JSON.stringify(
                medicationResult
            )}`
        );
        state.medications.push(medicationResult);
        state.error = false;
        state.statusMessage = "success";
        state.stateType = StateType.INITIALIZED;
    },

    medicationError(state: MedicationState, errorMessage: string) {
        state.error = true;
        state.statusMessage = errorMessage;
        state.stateType = StateType.ERROR;
    },
};
