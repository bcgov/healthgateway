import { voidMethod, voidPromise } from "@test/stubs/util";

import { LoadStatus } from "@/models/storeOperations";
import {
    AuthActions,
    AuthGetters,
    AuthModule,
    AuthMutations,
    AuthState,
} from "@/store/modules/auth/types";

const authState: AuthState = {
    authentication: {
        isChecked: true,
    },
    isAuthenticated: true,
    statusMessage: "",
    error: "",
    status: LoadStatus.NONE,
};

const authGetters: AuthGetters = {
    authenticationStatus(): string {
        return "";
    },
    oidcIsAuthenticated(): boolean {
        return true;
    },
    oidcAuthenticationIsChecked(): boolean {
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
    signIn: voidPromise,
    signOut: voidMethod,
    oidcCheckUser: voidPromise,
    getOidcUser: voidPromise,
    oidcWasAuthenticated: voidMethod,
    clearStorage: voidMethod,
};

const authMutations: AuthMutations = {
    setOidcAuth: voidMethod,
    unsetOidcAuth: voidMethod,
    setOidcAuthIsChecked: voidMethod,
    setOidcError: voidMethod,
};

const authStub: AuthModule = {
    namespaced: true,
    state: authState,
    getters: authGetters,
    actions: authActions,
    mutations: authMutations,
};

export default authStub;
