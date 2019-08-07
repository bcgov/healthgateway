import { Module } from 'vuex';
import { getters } from './getters';
import { actions } from './actions';
import { mutations } from './mutations';
import { ImmsState } from '@/models/immsState';
import { RootState, StateType } from '@/models/rootState';

export const state: ImmsState = {
    statusMessage: '',
    items: undefined,
    error: false,
    stateType: StateType.NONE
};

const namespaced: boolean = true;

export const imms: Module<ImmsState, RootState> = {
    namespaced,
    state,
    getters,
    actions,
    mutations
};
