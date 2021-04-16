import AuthenticationData from "@/models/authenticationData";
import { LoadStatus } from "@/models/storeOperations";
import { RootState } from "@/store/types";
import { User as OidcUser } from "oidc-client";
import {
    ActionContext,
    ActionTree,
    GetterTree,
    Module,
    MutationTree,
} from "vuex";

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

type AuthContext = ActionContext<AuthState, RootState>;
export interface AuthActions extends ActionTree<AuthState, RootState> {
    oidcCheckUser(context: AuthContext): Promise<boolean>;
    authenticateOidc(
        context: AuthContext,
        params: { idpHint: string; redirectPath: string }
    ): Promise<void>;
    oidcSignInCallback(context: AuthContext): Promise<string>;
    authenticateOidcSilent(context: AuthContext): Promise<void>;
    oidcWasAuthenticated(context: AuthContext, oidcUser: OidcUser): void;
    getOidcUser(context: AuthContext): Promise<void>;
    signOutOidc(): void;
    signOutOidcCallback(context: AuthContext): Promise<string>;
    clearStorage(context: AuthContext): void;
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
