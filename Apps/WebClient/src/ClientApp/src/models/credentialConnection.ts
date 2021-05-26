import { StringISODateTime } from "./dateWrapper";

export interface CredentialConnection {
    walletConnectionId: string;
    hdid: string;
    issuerConnectionid: string;
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
    sourcetype: string;
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
