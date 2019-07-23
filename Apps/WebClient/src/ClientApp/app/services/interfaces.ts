import BearerToken from "@/models/bearerToken"

export interface IAuthenticationService {
    startLoginFlow(idpHint: string, redirectUri: string): void;
    getBearerToken(): Promise<BearerToken>;
    refreshToken(): Promise<BearerToken>;
    destroyToken(): void;
}