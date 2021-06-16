import { StringISODateTime } from "./dateWrapper";

export interface WalletConnection {
    walletConnectionId: string;
    hdid: string;
    issuerConnectionId: string;
    connectedDate: StringISODateTime;
    disconnectedDate: StringISODateTime;
    invitationEndpoint: string;
    status: ConnectionStatus;
    version: string;
    credentials: WalletCredential[];
}

export enum ConnectionStatus {
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
    status: CredentialStatus;
    version: string;
}

export enum CredentialStatus {
    Created,
    Added,
    Revoked,
}
