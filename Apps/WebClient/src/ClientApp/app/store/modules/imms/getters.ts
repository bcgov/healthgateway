import { GetterTree } from 'vuex';
import { RootState, StateType } from '@/models/rootState';
import { ImmsState } from '@/models/immsState';
import ImmsData from '@/models/immsData';

export const getters: GetterTree<ImmsState, RootState> = {
    items(state: ImmsState): ImmsData[] {
        const { items } = state;
        return items != undefined ? items : [];
    },
    isLoading(state: ImmsState): boolean {
        const { stateType } = state;
        return stateType == StateType.REQUESTED;
    },
    hasErrors(state: ImmsState): boolean {
        const { error } = state;
        return error;
    },
};