import { GetterTree } from 'vuex';
import { RootState } from '../types';
import UserProfile from '@/models/UserProfile';

export const getters: GetterTree<UserProfile, RootState> = {
    firstName(state): string {
        return state.firstName;
    },
};
