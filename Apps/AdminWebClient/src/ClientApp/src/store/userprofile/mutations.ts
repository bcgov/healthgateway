import { MutationTree } from 'vuex';
import UserProfile from '@/models/UserProfile';

export const mutations: MutationTree<UserProfile> = {
  setFirstName(state, name) {
    state.firstName = name;
  },
  setLastName(state, name) {
    state.lastName = name;
  },
};
