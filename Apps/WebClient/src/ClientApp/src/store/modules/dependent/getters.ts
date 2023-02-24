import { Dependent } from "@/models/dependent";
import { LoadStatus } from "@/models/storeOperations";

import { DependentGetters, DependentState } from "./types";

export const getters: DependentGetters = {
    dependents(state: DependentState): Dependent[] {
        return state.dependents;
    },
    dependentsAreLoading(state: DependentState): boolean {
        return state.status === LoadStatus.REQUESTED;
    },
};
