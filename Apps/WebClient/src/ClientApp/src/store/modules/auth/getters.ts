import { AuthGetters, AuthState } from "./types";

export const getters: AuthGetters = {
    authenticationStatus(state: AuthState): string {
        return state.statusMessage;
    },
    oidcIsAuthenticated(state: AuthState): boolean {
        return state.isAuthenticated;
    },

    oidcScopes(state: AuthState): string[] | undefined {
        return state.authentication.scopes;
    },

    oidcAuthenticationIsChecked(state: AuthState): boolean {
        return state.authentication.isChecked;
    },
    oidcError(state: AuthState): unknown {
        return state.error;
    },
    isValidIdentityProvider(state: AuthState): boolean {
        return state.authentication === undefined
            ? false
            : state.authentication.identityProvider !== "IDIR";
    },
};
