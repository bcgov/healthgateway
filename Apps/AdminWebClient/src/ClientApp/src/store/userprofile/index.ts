import { Module } from 'vuex';
import { getters } from './getters';
import { actions } from './actions';
import { mutations } from './mutations';
import UserProfile from '@/models/UserProfile';
import { RootState } from '../types';

export const state: UserProfile = {
  username: '',
  firstName: '',
  lastName: '',
};

const namespaced: boolean = true;

export const userprofile: Module<UserProfile, RootState> = {
  namespaced,
  state,
  getters,
  actions,
  mutations,
};
