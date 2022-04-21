/* eslint-disable @typescript-eslint/no-explicit-any */
import {
    ActionContext,
    ActionTree,
    GetterTree,
    Module,
    MutationTree,
} from "vuex";

import AuthenticationData from "@/models/authenticationData";
import { LoadStatus } from "@/models/storeOperations";
import { OidcTokenDetails, OidcUserInfo } from "@/models/user";
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
    oidcAuthenticationIsChecked(state: AuthState): boolean;
    oidcError(state: AuthState): unknown;
    isValidIdentityProvider(
        _state: AuthState,
        _getters: any,
        _rootState: RootState,
        rootGetters: any
    ): boolean;
}

type StoreContext = ActionContext<AuthState, RootState>;
export interface AuthActions extends ActionTree<AuthState, RootState> {
    signIn(
        context: StoreContext,
        params: { idpHint: string; redirectPath: string }
    ): Promise<void>;
    signOut(): void;
    oidcCheckUser(context: StoreContext): Promise<boolean>;
    getOidcUser(context: StoreContext): Promise<void>;
    oidcWasAuthenticated(
        context: StoreContext,
        params: { tokenDetails: OidcTokenDetails; userInfo: OidcUserInfo }
    ): void;
    clearStorage(context: StoreContext): void;
}

export interface AuthMutations extends MutationTree<AuthState> {
    setOidcAuth(state: AuthState, user: OidcTokenDetails): void;
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
