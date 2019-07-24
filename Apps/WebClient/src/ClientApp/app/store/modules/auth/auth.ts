import { Module } from 'vuex';
import { getters } from './getters';
import { actions } from './actions';
import { mutations } from './mutations';
import { AuthState, RootState } from '@/models/authState';

export const state: AuthState = {
  statusMessage: '',
  authentication: undefined,
  error: false,
  requestedRoute: ''
};

const namespaced: boolean = true;

export const auth: Module<AuthState, RootState> = {
  namespaced,
  state,
  getters,
  actions,
  mutations
};