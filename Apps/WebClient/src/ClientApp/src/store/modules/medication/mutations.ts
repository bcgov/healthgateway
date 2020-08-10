import { MutationTree } from "vuex";
import { ILogger } from "@/services/interfaces";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { MedicationState, StateType } from "@/models/storeState";
import MedicationResult from "@/models/medicationResult";
const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);

export const mutations: MutationTree<MedicationState> = {
    addMedicationData(
        state: MedicationState,
        medicationResult: MedicationResult
    ) {
        logger.info(
            `addMedicationData medicationResult: ${JSON.stringify(
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
