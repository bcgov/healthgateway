import { LoadStatus } from "@/models/storeOperations";
import {
    AuthState,
    AuthGetters,
    AuthActions,
    AuthMutations,
    AuthModule,
} from "@/store/modules/auth/types";

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

const authMutations: AuthMutations = {
    setOidcAuth(): void {},
    unsetOidcAuth(): void {},
    setOidcAuthIsChecked(): void {},
    setOidcError(): void {},
};

const authStub: AuthModule = {
    namespaced: true,
    state: authState,
    getters: authGetters,
    actions: authActions,
    mutations: authMutations,
};

export default authStub;
