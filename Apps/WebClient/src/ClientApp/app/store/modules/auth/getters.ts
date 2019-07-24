import { GetterTree } from 'vuex';
import { AuthState, RootState } from '@/models/authState';

export const getters: GetterTree<AuthState, RootState> = {
    isAuthenticated(state: AuthState): boolean {
        const { authentication } = state;
        return authentication != undefined;
        //return true;
    },
    authenticationStatus(state: AuthState): string { 
        return state.statusMessage 
    },
    requestedRoute(state: AuthState): string {
        return state.requestedRoute
    }
};