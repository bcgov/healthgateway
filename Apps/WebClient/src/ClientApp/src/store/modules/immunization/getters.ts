import { GetterTree } from "vuex";
import { ImmunizationState, RootState, StateType } from "@/models/storeState";
import ImmunizationModel from "@/models/immunizationModel";

export const getters: GetterTree<ImmunizationState, RootState> = {
    getStoredImmunizations(state: ImmunizationState): ImmunizationModel[] {
        return state.immunizations;
    },
    isDeferredLoad(state: ImmunizationState): boolean {
        return state.stateType === StateType.DEFERRED;
    },
};
