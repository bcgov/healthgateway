import {
    ActionContext,
    ActionTree,
    GetterTree,
    Module,
    MutationTree,
} from "vuex";

import {
    CredentialConnection,
    WalletCredential,
} from "@/models/credentialConnection";
import { ResultError } from "@/models/requestResult";
import { LoadStatus } from "@/models/storeOperations";
import { RootState } from "@/store/types";

export interface CredentialState {
    connection?: CredentialConnection;
    credentials: WalletCredential[];
    statusMessage: string;
    error: boolean;
    status: LoadStatus;
}

export interface CredentialGetters
    extends GetterTree<CredentialState, RootState> {
    connection(state: CredentialState): CredentialConnection | undefined;
    credentials(state: CredentialState): WalletCredential[];
}

type CredentialContext = ActionContext<CredentialState, RootState>;
export interface CredentialActions
    extends ActionTree<CredentialState, RootState> {
    retrieveConnection(
        context: CredentialContext,
        params: { hdid: string }
    ): Promise<boolean>;
    retrieveCredentials(
        context: CredentialContext,
        params: { hdid: string }
    ): Promise<boolean>;
    createCredentialConnection(
        context: CredentialContext,
        params: { hdid: string; targetIds: string[] }
    ): Promise<boolean>;
    handleError(context: CredentialContext, error: ResultError): void;
}

export interface CredentialMutation extends MutationTree<CredentialState> {
    setRequested(state: CredentialState): void;
    setConnection(
        state: CredentialState,
        connection: CredentialConnection
    ): void;
    setCredentials(
        state: CredentialState,
        credentials: WalletCredential[]
    ): void;
    credentialError(state: CredentialState, errorMessage: string): void;
}

export interface CredentialModule extends Module<CredentialState, RootState> {
    namespaced: boolean;
    state: CredentialState;
    getters: CredentialGetters;
    actions: CredentialActions;
    mutations: CredentialMutation;
}
