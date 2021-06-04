import { LoadStatus } from "@/models/storeOperations";
import { WalletConnection, WalletCredential } from "@/models/wallet";

import { CredentialMutation, CredentialState } from "./types";

export const mutations: CredentialMutation = {
    setRequested(state: CredentialState) {
        state.status = LoadStatus.REQUESTED;
    },
    setConnection(state: CredentialState, connection: WalletConnection): void {
        state.connection = connection;
    },
    addCredential(state: CredentialState, credential: WalletCredential): void {
        state.connection?.credentials.push(credential);
    },
    credentialError(state: CredentialState, errorMessage: string): void {
        state.error = true;
        state.statusMessage = errorMessage;
        state.status = LoadStatus.ERROR;
    },
};
