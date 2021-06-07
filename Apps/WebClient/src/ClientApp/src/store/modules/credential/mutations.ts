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
    removeCredential(
        state: CredentialState,
        credential: WalletCredential
    ): void {
        if (state.connection !== undefined) {
            const credentialIndex = state.connection.credentials.findIndex(
                (x) => x.credentialId === credential.credentialId
            );
            if (credentialIndex > -1) {
                delete state.connection.credentials[credentialIndex];
                state.connection.credentials.splice(credentialIndex, 1);
            }
        }
    },
    credentialError(state: CredentialState, errorMessage: string): void {
        state.error = true;
        state.statusMessage = errorMessage;
        state.status = LoadStatus.ERROR;
    },
};
