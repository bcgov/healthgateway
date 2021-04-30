import { LoadStatus } from "@/models/storeOperations";
import {
    AuthActions,
    AuthGetters,
    AuthModule,
    AuthMutations,
    AuthState,
} from "@/store/modules/auth/types";

import { stubbedPromise, voidMethod } from "../util";

const authState: AuthState = {
    authentication: {
        isChecked: true,
        identityProvider: "",
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
    oidcScopes(): string[] | undefined {
        return undefined;
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
    oidcCheckUser: stubbedPromise,
    authenticateOidc: stubbedPromise,
    oidcSignInCallback: stubbedPromise,
    authenticateOidcSilent: stubbedPromise,
    oidcWasAuthenticated: voidMethod,
    getOidcUser: stubbedPromise,
    signOutOidc: voidMethod,
    signOutOidcCallback: stubbedPromise,
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
