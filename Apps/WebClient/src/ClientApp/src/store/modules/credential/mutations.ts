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
    addCredentials(
        state: CredentialState,
        credentials: WalletCredential[]
    ): void {
        credentials.forEach((c) => state.connection?.credentials.push(c));
    },
    credentialError(state: CredentialState, errorMessage: string): void {
        state.error = true;
        state.statusMessage = errorMessage;
        state.status = LoadStatus.ERROR;
    },
};
