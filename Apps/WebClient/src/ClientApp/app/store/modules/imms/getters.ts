import { GetterTree } from 'vuex';
import { RootState } from '@/models/rootState';
import { ImmsState } from '@/models/immsState';
import ImmsData from '@/models/immsData';

export const getters: GetterTree<ImmsState, RootState> = {
    items(state: ImmsState): ImmsData[] {
        const { items } = state;
        return items != undefined ? items : [];
    },
};