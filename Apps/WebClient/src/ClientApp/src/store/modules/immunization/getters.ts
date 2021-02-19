import { GetterTree } from "vuex";

import { ImmunizationEvent, Recommendation } from "@/models/immunizationModel";
import { ImmunizationState, LoadStatus, RootState } from "@/models/storeState";

export const getters: GetterTree<ImmunizationState, RootState> = {
    immunizations(state: ImmunizationState): ImmunizationEvent[] {
        return state.immunizations;
    },
    getStoredRecommendations(state: ImmunizationState): Recommendation[] {
        return state.recommendations;
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
