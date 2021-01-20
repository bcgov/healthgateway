import { GetterTree } from "vuex";

import ImmunizationModel from "@/models/immunizationModel";
import { ImmunizationState, RootState, StateType } from "@/models/storeState";

export const getters: GetterTree<ImmunizationState, RootState> = {
    getStoredImmunizations(state: ImmunizationState): ImmunizationModel[] {
        return state.immunizations;
    },
    isDeferredLoad(state: ImmunizationState): boolean {
        return state.stateType === StateType.DEFERRED;
    },
};
