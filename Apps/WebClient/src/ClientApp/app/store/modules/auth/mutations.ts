import { MutationTree } from 'vuex';
import { AuthState, Authentication } from '@/models/authState';

export const mutations: MutationTree<AuthState> = {
    authenticationRequest(state: AuthState) {
        state.error = false;
        state.statusMessage = 'loading';
        state.requestedRoute = '';
    },
    authenticationLoaded(state: AuthState, payload: Authentication) {
        state.error = false;
        state.authentication = payload;
        state.statusMessage = 'success';
        state.requestedRoute = '';
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
    requestedRoute(state: AuthState, to: string) {
        state.requestedRoute = to;
    }

};