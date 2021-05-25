import { StringISODateTime } from "./dateWrapper";

export interface CredentialConnection {
    walletConnectionId: string;
    hdid: string;
    issuerConnectionid: string;
    ConnectedDate: StringISODateTime;
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
    WalletConnectionId: string;
    IssuerCredentialId: string;
    sourcetype: string;
    sourceId: string;
    AddedDate: StringISODateTime;
    RevokedDate?: StringISODateTime;
    status: CredentialState;
    version: string;
}

enum CredentialState {
    Created,
    Added,
    Revoked,
}
