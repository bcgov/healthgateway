import { LoadStatus } from "@/models/storeOperations";
import {
    AuthActions,
    AuthGetters,
    AuthModule,
    AuthMutations,
    AuthState,
} from "@/store/modules/auth/types";

var authState: AuthState = {
    authentication: {
        isChecked: true,
        identityProvider: "",
    },
    isAuthenticated: true,
    statusMessage: "",
    error: "",
    status: LoadStatus.NONE,
};

var authGetters: AuthGetters = {
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

var authActions: AuthActions = {
    oidcCheckUser(): Promise<boolean> {
        return new Promise(() => {});
    },
    authenticateOidc(): Promise<void> {
        return new Promise(() => {});
    },
    oidcSignInCallback(): Promise<string> {
        return new Promise(() => {});
    },
    authenticateOidcSilent(): Promise<void> {
        return new Promise(() => {});
    },
    oidcWasAuthenticated(): void {},
    getOidcUser(): Promise<void> {
        return new Promise(() => {});
    },
    signOutOidc(): void {},
    signOutOidcCallback(): Promise<string> {
        return new Promise(() => {});
    },
    clearStorage(): void {},
};

var authMutations: AuthMutations = {
    setOidcAuth(): void {},
    unsetOidcAuth(): void {},
    setOidcAuthIsChecked(): void {},
    setOidcError(): void {},
};

var authStub: AuthModule = {
    namespaced: true,
    state: authState,
    getters: authGetters,
    actions: authActions,
    mutations: authMutations,
};

export default authStub;
