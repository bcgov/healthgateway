/* eslint-disable @typescript-eslint/no-explicit-any */
import { OidcUserInfo } from "@/models/user";
import { RootState } from "@/store/types";

import { AuthGetters, AuthState } from "./types";

export const getters: AuthGetters = {
    authenticationStatus(state: AuthState): string {
        return state.statusMessage;
    },
    oidcIsAuthenticated(state: AuthState): boolean {
        return state.isAuthenticated;
    },
    oidcAuthenticationIsChecked(state: AuthState): boolean {
        return state.authentication.isChecked;
    },
    oidcError(state: AuthState): unknown {
        return state.error;
    },
    isValidIdentityProvider: (
        _state: AuthState,
        _getters: any,
        _rootState: RootState,
        rootGetters: any
    ): boolean => {
        const userInfo = <OidcUserInfo | undefined>(
            rootGetters["user/oidcUserInfo"]
        );

        return userInfo === undefined ? false : userInfo.idp !== "IDIR";
    },
};
