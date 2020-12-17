import { MutationTree } from "vuex";
import { ImmunizationState, StateType } from "@/models/storeState";
import ImmunizationResult from "@/models/immunizationResult";

export const mutations: MutationTree<ImmunizationState> = {
    setImmunizationResult(
        state: ImmunizationState,
        immunizationResult: ImmunizationResult
    ) {
        state.immunizations = immunizationResult.immunizations;
        state.error = false;
        state.statusMessage = "success";
        state.stateType = immunizationResult.loadState.refreshInProgress
            ? StateType.DEFERRED
            : StateType.INITIALIZED;
    },
    immunizationError(state: ImmunizationState, errorMessage: string) {
        state.error = true;
        state.statusMessage = errorMessage;
        state.stateType = StateType.ERROR;
    },
};
