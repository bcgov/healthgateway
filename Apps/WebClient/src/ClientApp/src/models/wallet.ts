import { StringISODateTime } from "./dateWrapper";

export interface WalletConnection {
    walletConnectionId: string;
    hdid: string;
    issuerConnectionId: string;
    connectedDate: StringISODateTime;
    disconnectedDate: StringISODateTime;
    invitationEndpoint: string;
    state: ConnectionState;
    version: string;
    credentials: WalletCredential[];
}

export enum ConnectionState {
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

export enum CredentialState {
    Created,
    Added,
    Revoked,
}
