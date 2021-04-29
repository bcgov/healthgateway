import { LoadStatus } from "@/models/storeOperations";
import {
    AuthActions,
    AuthGetters,
    AuthModule,
    AuthMutations,
    AuthState,
} from "@/store/modules/auth/types";

import { stubbedPromise, stubbedVoid } from "../../utility/stubUtil";

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
    oidcWasAuthenticated: stubbedVoid,
    getOidcUser: stubbedPromise,
    signOutOidc: stubbedVoid,
    signOutOidcCallback: stubbedPromise,
    clearStorage: stubbedVoid,
};

const authMutations: AuthMutations = {
    setOidcAuth: stubbedVoid,
    unsetOidcAuth: stubbedVoid,
    setOidcAuthIsChecked: stubbedVoid,
    setOidcError: stubbedVoid,
};

const authStub: AuthModule = {
    namespaced: true,
    state: authState,
    getters: authGetters,
    actions: authActions,
    mutations: authMutations,
};

export default authStub;
