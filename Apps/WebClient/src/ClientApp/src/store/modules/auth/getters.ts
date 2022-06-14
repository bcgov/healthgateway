/* eslint-disable @typescript-eslint/no-explicit-any */
import { OidcUserInfo } from "@/models/user";
import { RootState } from "@/store/types";

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
    isValidIdentityProvider: (
        _state: AuthState,
        _getters: any,
        _rootState: RootState,
        rootGetters: any
    ): boolean => {
        const userInfo = <OidcUserInfo | undefined>(
            rootGetters["user/oidcUserInfo"]
        );

        return userInfo === undefined
            ? false
            : userInfo.idp === "BCSC" || userInfo.idp === undefined;
    },
};
