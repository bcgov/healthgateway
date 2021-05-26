import {
    CredentialConnection,
    WalletCredential,
} from "@/models/credentialConnection";

import { CredentialGetters, CredentialState } from "./types";

export const getters: CredentialGetters = {
    connection(state: CredentialState): CredentialConnection | undefined {
        const { connection } = state;
        return connection;
    },
    credentials(state: CredentialState): WalletCredential[] {
        const { credentials } = state;
        return credentials;
    },
};
