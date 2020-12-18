import { MutationTree } from "vuex";
import { ImmunizationState, StateType } from "@/models/storeState";
import ImmunizationResult from "@/models/immunizationResult";

export const mutations: MutationTree<ImmunizationState> = {
    setImmunizationResult(
        state: ImmunizationState,
        immunizationResult: ImmunizationResult
    ) {
        if (immunizationResult.loadState.refreshInProgress) {
            state.immunizations = [];
            state.stateType = StateType.DEFERRED;
        } else {
            state.immunizations = immunizationResult.immunizations;
            state.stateType = StateType.INITIALIZED;
        }

        state.error = false;
        state.statusMessage = "success";
    },
    immunizationError(state: ImmunizationState, errorMessage: string) {
        state.error = true;
        state.statusMessage = errorMessage;
        state.stateType = StateType.ERROR;
    },
};
