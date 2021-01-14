import { GetterTree } from "vuex";

import { AuthState, RootState } from "@/models/storeState";
import User from "@/models/userProfile";

export const getters: GetterTree<AuthState, RootState> = {
    isAuthenticated(state: AuthState): boolean {
        const { authentication } = state;
        return authentication != undefined
            ? authentication.isAuthenticated
            : false;
    },
    isAuthorized(state: AuthState): boolean {
        const { authentication } = state;
        return authentication != undefined
            ? authentication.isAuthorized
            : false;
    },
    authenticationStatus(state: AuthState): string {
        return state.statusMessage;
    },
    authenticatedUser(state: AuthState): User | undefined {
        const { authentication } = state;
        return authentication != undefined
            ? authentication.userProfile
            : undefined;
    }
};
