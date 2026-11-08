import type { OpenIdConnectConfiguration } from "@/models/configData";
import type { OidcTokenDetails, OidcUserInfo } from "@/models/user";

/** Number of seconds before access-token expiry at which a refresh is scheduled (same as web). */
export const REFRESH_CUSHION = 30;

export interface AuthInitOptions {
    /** Device security is on: a session that cannot refresh stays, to open the saved records. */
    keepSessionOnFailure?: boolean;
}

/** "rejected": Keycloak refused the refresh token. "network": it could not be reached. */
export type RestoreFailure = "rejected" | "network";

export interface IAuthService {
    /** Restore any persisted session; resolves once the service knows whether a user is signed in. */
    init(options?: AuthInitOptions): Promise<void>;
    /** Native: why a kept session could not be refreshed at `init`. */
    getRestoreFailure?(): RestoreFailure | undefined;
    /**
     * Start the login flow. On web this redirects the page and never resolves;
     * on native it resolves with the token details once the auth session completes.
     */
    signIn(redirectPath: string, idpHint?: string): Promise<OidcTokenDetails>;
    /** End the Keycloak session and clear local state. */
    signOut(): Promise<void>;
    /** Refresh the access token if it is within REFRESH_CUSHION seconds of expiry. Resolves true if refreshed. */
    refreshToken(force?: boolean): Promise<boolean>;
    getOidcTokenDetails(): OidcTokenDetails | null;
    getOidcUserInfo(): Promise<OidcUserInfo>;
    clearState(): void;
}

export type AuthServiceFactory = (
    oidcConfig: OpenIdConnectConfiguration
) => IAuthService;
