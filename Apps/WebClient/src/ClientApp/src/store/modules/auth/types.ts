import { User as OidcUser } from "oidc-client";
import {
    ActionContext,
    ActionTree,
    GetterTree,
    Module,
    MutationTree,
} from "vuex";

import AuthenticationData from "@/models/authenticationData";
import { LoadStatus } from "@/models/storeOperations";
import { RootState } from "@/store/types";

export interface AuthState {
    authentication: AuthenticationData;
    isAuthenticated: boolean;
    statusMessage: string;
    error: unknown;
    status: LoadStatus;
}

export interface AuthGetters extends GetterTree<AuthState, RootState> {
    authenticationStatus(state: AuthState): string;
    oidcIsAuthenticated(state: AuthState): boolean;
    oidcScopes(state: AuthState): string[] | undefined;
    oidcAuthenticationIsChecked(state: AuthState): boolean;
    oidcError(state: AuthState): unknown;
    isValidIdentityProvider(state: AuthState): boolean;
}

type StoreContext = ActionContext<AuthState, RootState>;
export interface AuthActions extends ActionTree<AuthState, RootState> {
    oidcCheckUser(context: StoreContext): Promise<boolean>;
    authenticateOidc(
        context: StoreContext,
        params: { idpHint: string; redirectPath: string }
    ): Promise<void>;
    oidcSignInCallback(context: StoreContext): Promise<string>;
    authenticateOidcSilent(context: StoreContext): Promise<void>;
    oidcWasAuthenticated(context: StoreContext, oidcUser: OidcUser): void;
    getOidcUser(context: StoreContext): Promise<void>;
    signOutOidc(): void;
    signOutOidcCallback(context: StoreContext): Promise<string>;
    clearStorage(context: StoreContext): void;
}

export interface AuthMutations extends MutationTree<AuthState> {
    setOidcAuth(state: AuthState, user: OidcUser): void;
    unsetOidcAuth(state: AuthState): void;
    setOidcAuthIsChecked(state: AuthState): void;
    setOidcError(state: AuthState, error: unknown): void;
}

export interface AuthModule extends Module<AuthState, RootState> {
    namespaced: boolean;
    state: AuthState;
    getters: AuthGetters;
    actions: AuthActions;
    mutations: AuthMutations;
}
