import { ImmunizationEvent, Recommendation } from "@/models/immunizationModel";
import { LoadStatus } from "@/models/storeOperations";

import { ImmunizationGetters, ImmunizationState } from "./types";

export const getters: ImmunizationGetters = {
    immunizations(state: ImmunizationState): ImmunizationEvent[] {
        return state.immunizations;
    },
    recomendations(state: ImmunizationState): Recommendation[] {
        return state.recommendations;
    },
    immunizationCount(state: ImmunizationState): number {
        return state.immunizations.length;
    },
    isDeferredLoad(state: ImmunizationState): boolean {
        return (
            state.status === LoadStatus.DEFERRED ||
            state.status === LoadStatus.ASYNC_REQUESTED
        );
    },
    isLoading(state: ImmunizationState): boolean {
        return state.status === LoadStatus.REQUESTED;
    },
};
