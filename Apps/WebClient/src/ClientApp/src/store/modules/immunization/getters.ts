import { GetterTree } from "vuex";

import { ImmunizationEvent, Recomendation } from "@/models/immunizationModel";
import { ImmunizationState, RootState, StateType } from "@/models/storeState";

export const getters: GetterTree<ImmunizationState, RootState> = {
    getStoredImmunizations(state: ImmunizationState): ImmunizationEvent[] {
        return state.immunizations;
    },
    getStoredRecommendations(state: ImmunizationState): Recomendation[] {
        return state.recomendations;
    },
    isDeferredLoad(state: ImmunizationState): boolean {
        return state.stateType === StateType.DEFERRED;
    },
};
