import { StringISODateTime } from "./dateWrapper";

export interface WalletConnection {
    walletConnectionId: string;
    hdid: string;
    issuerConnectionId: string;
    connectedDate: StringISODateTime;
    disconnectedDate: StringISODateTime;
    qrCode: string;
    state: ConnectionState;
    version: string;
}

enum ConnectionState {
    Pending,
    Connected,
    Disconnected,
}

export interface WalletCredential {
    credentialId: string;
    walletConnectionId: string;
    issuerCredentialId: string;
    sourceType: string;
    sourceId: string;
    addedDate: StringISODateTime;
    revokedDate?: StringISODateTime;
    status: CredentialState;
    version: string;
}

enum CredentialState {
    Created,
    Added,
    Revoked,
}
