import { MutationTree } from 'vuex';
import { AuthState } from '@/models/authState';
import AuthenticationData from '@/models/authenticationData';

export const mutations: MutationTree<AuthState> = {
    authenticationInitialized(state: AuthState, authData: AuthenticationData) {
        state.error = false;
        state.authentication = authData;
        state.statusMessage = 'initialized';
    },
    authenticationRequest(state: AuthState) {
        state.error = false;
        state.statusMessage = 'loading';
    },
    authenticationLoaded(state: AuthState, authData: AuthenticationData) {
        state.error = false;
        state.authentication = authData;
        state.statusMessage = 'success';
    },
    authenticationError(state: AuthState, errorMessage: string) {
        state.error = true;
        state.authentication = undefined;
        state.statusMessage = errorMessage;
    },
    logout(state: AuthState) {
        state.error = false;
        state.statusMessage = ''
        state.authentication = undefined;
    },
};