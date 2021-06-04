import { WalletConnection, WalletCredential } from "@/models/wallet";

import { CredentialGetters, CredentialState } from "./types";

export const getters: CredentialGetters = {
    connection(state: CredentialState): WalletConnection | undefined {
        const { connection } = state;
        return connection;
    },
    credentials(state: CredentialState): WalletCredential[] | null {
        const { connection } = state;
        if (connection === undefined || connection === null) {
            return null;
        } else {
            return connection.credentials;
        }
    },
};
