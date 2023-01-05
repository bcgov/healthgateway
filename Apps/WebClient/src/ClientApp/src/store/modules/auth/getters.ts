import { AuthGetters, AuthState } from "./types";

export const getters: AuthGetters = {
    oidcIsAuthenticated(state: AuthState): boolean {
        return (
            state.tokenDetails !== undefined &&
            state.tokenDetails.idToken.length > 0
        );
    },
    oidcError(state: AuthState): unknown {
        return state.error;
    },
};
