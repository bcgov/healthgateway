/* eslint-disable @typescript-eslint/no-explicit-any */
import {
    ActionContext,
    ActionTree,
    GetterTree,
    Module,
    MutationTree,
} from "vuex";

import { OidcTokenDetails } from "@/models/user";
import { RootState } from "@/store/types";

export interface AuthState {
    tokenDetails?: OidcTokenDetails;
    error: unknown;
}

export interface AuthGetters extends GetterTree<AuthState, RootState> {
    oidcIsAuthenticated(state: AuthState): boolean;
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
    initialize(context: StoreContext): Promise<void>;
    signIn(
        context: StoreContext,
        params: { idpHint: string; redirectPath: string }
    ): Promise<void>;
    signOut(): void;
    refreshToken(context: StoreContext): Promise<void>;
    clearStorage(context: StoreContext): void;
    handleSuccessfulAuthentication(
        context: StoreContext,
        tokenDetails: OidcTokenDetails
    ): void;
}

export interface AuthMutations extends MutationTree<AuthState> {
    setAuthenticated(state: AuthState, user: OidcTokenDetails): void;
    setUnauthenticated(state: AuthState): void;
    setError(state: AuthState, error: unknown): void;
}

export interface AuthModule extends Module<AuthState, RootState> {
    namespaced: boolean;
    state: AuthState;
    getters: AuthGetters;
    actions: AuthActions;
    mutations: AuthMutations;
}
