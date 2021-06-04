import {
    ActionContext,
    ActionTree,
    GetterTree,
    Module,
    MutationTree,
} from "vuex";

import { ResultError } from "@/models/requestResult";
import { LoadStatus } from "@/models/storeOperations";
import { WalletConnection, WalletCredential } from "@/models/wallet";
import { RootState } from "@/store/types";

export interface CredentialState {
    connection?: WalletConnection;
    statusMessage: string;
    error: boolean;
    status: LoadStatus;
}

export interface CredentialGetters
    extends GetterTree<CredentialState, RootState> {
    connection(state: CredentialState): WalletConnection | undefined;
    credentials(state: CredentialState): WalletCredential[] | null;
}

type CredentialContext = ActionContext<CredentialState, RootState>;
export interface CredentialActions
    extends ActionTree<CredentialState, RootState> {
    createConnection(
        context: CredentialContext,
        params: { hdid: string }
    ): Promise<boolean>;
    retrieveConnection(
        context: CredentialContext,
        params: { hdid: string }
    ): Promise<boolean>;
    createCredential(
        context: CredentialContext,
        params: { hdid: string; targetId: string }
    ): Promise<boolean>;
    handleError(context: CredentialContext, error: ResultError): void;
}

export interface CredentialMutation extends MutationTree<CredentialState> {
    setRequested(state: CredentialState): void;
    setConnection(state: CredentialState, connection: WalletConnection): void;
    credentialError(state: CredentialState, errorMessage: string): void;
}

export interface CredentialModule extends Module<CredentialState, RootState> {
    namespaced: boolean;
    state: CredentialState;
    getters: CredentialGetters;
    actions: CredentialActions;
    mutations: CredentialMutation;
}
