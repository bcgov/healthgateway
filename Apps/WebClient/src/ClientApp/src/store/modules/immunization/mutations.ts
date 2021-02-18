import { MutationTree } from "vuex";

import ImmunizationResult from "@/models/immunizationResult";
import { ImmunizationState, LoadStatus } from "@/models/storeState";

export const mutations: MutationTree<ImmunizationState> = {
    setRequested(state: ImmunizationState) {
        state.status = LoadStatus.REQUESTED;
    },
    setImmunizationResult(
        state: ImmunizationState,
        immunizationResult: ImmunizationResult
    ) {
        if (immunizationResult.loadState.refreshInProgress) {
            state.immunizations = [];
            state.error = undefined;
            state.status = LoadStatus.DEFERRED;
        } else {
            state.immunizations = immunizationResult.immunizations;
            state.recommendations = immunizationResult.recommendations;
            state.error = undefined;
            state.status = LoadStatus.LOADED;
        }
    },
    immunizationError(state: ImmunizationState, errorMessage: string) {
        state.statusMessage = errorMessage;
        state.status = LoadStatus.ERROR;
    },
};
