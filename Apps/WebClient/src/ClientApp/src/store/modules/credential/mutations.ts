import {
    CredentialConnection,
    WalletCredential,
} from "@/models/credentialConnection";
import { LoadStatus } from "@/models/storeOperations";

import { CredentialMutation, CredentialState } from "./types";

export const mutations: CredentialMutation = {
    setRequested(state: CredentialState) {
        state.status = LoadStatus.REQUESTED;
    },
    setConnection(
        state: CredentialState,
        connection: CredentialConnection
    ): void {
        state.connection = connection;
    },
    setCredentials(
        state: CredentialState,
        credentials: WalletCredential[]
    ): void {
        state.credentials = credentials;
    },
    credentialError(state: CredentialState, errorMessage: string): void {
        state.error = true;
        state.statusMessage = errorMessage;
        state.status = LoadStatus.ERROR;
    },
};
