import { Dependent } from "@/models/dependent";
import { ResultError } from "@/models/errors";
import { LoadStatus } from "@/models/storeOperations";

import { DependentMutations, DependentState } from "./types";

export const mutations: DependentMutations = {
    setDependentsRequested(state: DependentState) {
        state.status = LoadStatus.REQUESTED;
    },
    setDependents(state: DependentState, dependents: Dependent[]) {
        state.dependents = dependents;
        state.error = undefined;
        state.status = LoadStatus.LOADED;
    },
    setDependentsError(state: DependentState, error: ResultError) {
        state.error = error;
        state.statusMessage = error.resultMessage;
        state.status = LoadStatus.ERROR;
    },
};
