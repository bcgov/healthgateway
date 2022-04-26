import { voidMethod, voidPromise } from "@test/stubs/util";

import {
    AuthActions,
    AuthGetters,
    AuthModule,
    AuthMutations,
    AuthState,
} from "@/store/modules/auth/types";

const authState: AuthState = {
    tokenDetails: undefined,
    error: "",
};

const authGetters: AuthGetters = {
    oidcIsAuthenticated(): boolean {
        return true;
    },
    oidcError(): unknown {
        return "";
    },
    isValidIdentityProvider(): boolean {
        return true;
    },
};

const authActions: AuthActions = {
    initialize: voidPromise,
    signIn: voidPromise,
    signOut: voidMethod,
    refreshToken: voidPromise,
    clearStorage: voidMethod,
    handleSuccessfulAuthentication: voidMethod,
};

const authMutations: AuthMutations = {
    setAuthenticated: voidMethod,
    setUnauthenticated: voidMethod,
    setError: voidMethod,
};

const authStub: AuthModule = {
    namespaced: true,
    state: authState,
    getters: authGetters,
    actions: authActions,
    mutations: authMutations,
};

export default authStub;
