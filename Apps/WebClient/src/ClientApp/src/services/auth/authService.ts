import type { OpenIdConnectConfiguration } from "@/models/configData";
import type { OidcTokenDetails, OidcUserInfo } from "@/models/user";

/** Number of seconds before access-token expiry at which a refresh is scheduled (same as web). */
export const REFRESH_CUSHION = 30;

export interface AuthInitOptions {
    /** Device security is on: a session that cannot refresh stays, to open the saved records. */
    keepSessionOnFailure?: boolean;
    /**
     * Native, device security on: the session is stored sealed by the vault. `init` then only notes that one is there;
     * `unseal` reads it once the lock's prompt has passed.
     */
    sealed?: boolean;
}

/** "rejected": Keycloak refused the refresh token. "network": it could not be reached. */
export type RestoreFailure = "rejected" | "network";

/**
 * "unsealed": the session is in memory. "cancelled": the prompt was turned down, and the session stays sealed.
 * "unavailable": it could not be read this time, for a reason that may pass; it stays sealed too.
 * "unreadable": the key or the stored copy is no good (device security removed and set up again); the session is gone.
 * Also the answer when there was no session, sealed or in memory, to begin with.
 */
export type UnsealResult =
    "unsealed" | "cancelled" | "unavailable" | "unreadable";

/** Turned down, or not readable this time: the login stays sealed, and the lock stays on. */
export const keepsSealed = (result: UnsealResult) =>
    result === "cancelled" || result === "unavailable";

export interface UnsealOptions {
    /** As `init` would: an expired access token is refreshed, and a failure kept for `getRestoreFailure`. */
    restore?: boolean;
}

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
    /** Native: a stored session is waiting for `unseal`, and no token is in memory. */
    isSealed?(): boolean;
    /** Native: reads the sealed session into memory. The OS asks first unless the vault's prompt has just passed. */
    unseal?(options?: UnsealOptions): Promise<UnsealResult>;
    /**
     * Native, device security on: the lock went on, so the tokens leave memory and only the sealed copy is left. Waits for
     * a refresh on its way, whose tokens are saved first. Keeps them when the stored copy could not be sealed.
     */
    seal?(): Promise<void>;
    /** Native: device security turned on or off; the stored session moves to match. Rejects when it could not. */
    setSealed?(sealed: boolean): Promise<void>;
}

export type AuthServiceFactory = (
    oidcConfig: OpenIdConnectConfiguration
) => IAuthService;
