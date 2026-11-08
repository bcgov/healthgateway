import Keycloak, { type KeycloakConfig } from "keycloak-js";

import type { OpenIdConnectConfiguration } from "@/models/configData";
import type { OidcTokenDetails, OidcUserInfo } from "@/models/user";
import { describeError, logger } from "@/services/logger";

import { type IAuthService, REFRESH_CUSHION } from "./authService";
import { TOKEN_STORAGE_KEY } from "./storedSession.web";

const INIT_TIMEOUT_MS = 5000;

function readStoredTokens(): {
    token?: string;
    refreshToken?: string;
    idToken?: string;
} {
    try {
        const raw = sessionStorage.getItem(TOKEN_STORAGE_KEY);
        return raw
            ? (JSON.parse(raw) as {
                  token?: string;
                  refreshToken?: string;
                  idToken?: string;
              })
            : {};
    } catch {
        return {};
    }
}

/**
 * Web implementation: keycloak-js, matching Apps/WebClient/src/ClientApp/src/services/restAuthService.ts.
 * Redirect flow with PKCE (S256). Callbacks are `${origin}/loginCallback` and `${origin}/logoutComplete`
 * unless the configuration supplies `openIdConnect.callbacks.Logon` / `.Logout`.
 */
export class WebAuthService implements IAuthService {
    private readonly keycloak: Keycloak;
    private readonly scope: string;
    private readonly logonCallback: string;
    private readonly logoutCallback: string;
    private initialized = false;

    constructor(private readonly oidcConfig: OpenIdConnectConfiguration) {
        const [url, realm] = oidcConfig.authority.split("/realms/");
        const keycloakConfig: KeycloakConfig = {
            url,
            realm,
            clientId: oidcConfig.clientId,
        };
        this.keycloak = new Keycloak(keycloakConfig);
        this.scope = oidcConfig.scope;
        this.logonCallback =
            oidcConfig.callbacks?.["Logon"] ||
            `${globalThis.location.origin}/loginCallback`;
        this.logoutCallback =
            oidcConfig.callbacks?.["Logout"] ||
            `${globalThis.location.origin}/logoutComplete`;

        const kc = this.keycloak;
        kc.onReady = () => logger.verbose("Keycloak: onReady");
        kc.onAuthSuccess = () => {
            logger.verbose("Keycloak: onAuthSuccess");
            this.persistTokens();
        };
        kc.onAuthError = (e) =>
            logger.error(`Keycloak: onAuthError - ${e?.error}`);
        kc.onAuthRefreshSuccess = () => {
            logger.verbose("Keycloak: onAuthRefreshSuccess");
            this.persistTokens();
        };
        kc.onAuthRefreshError = () =>
            logger.verbose("Keycloak: onAuthRefreshError");
        kc.onAuthLogout = () => {
            logger.verbose("Keycloak: onAuthLogout");
            this.persistTokens();
        };
        kc.onTokenExpired = () => logger.verbose("Keycloak: onTokenExpired");
    }

    async init(): Promise<void> {
        if (this.initialized) return;
        this.initialized = true;
        const stored = readStoredTokens();
        try {
            // Never leave the page on load: check-sso runs in a hidden iframe (silent-check-sso.html) and
            // `silentCheckSsoFallback: false` disables keycloak-js's full-page redirect fallback, so the
            // public landing page always renders. Tokens from the previous page load are handed back to
            // keycloak-js so a reload keeps the session even when third-party cookies are blocked.
            // keycloak-js also parses the ?code=&state= on /loginCallback here.
            // If the silent iframe never answers (e.g. this origin is not a valid redirect URI yet, so
            // Keycloak renders an error page inside it), give up after INIT_TIMEOUT_MS and stay logged out.
            await Promise.race([
                this.keycloak.init({
                    onLoad: "check-sso",
                    silentCheckSsoRedirectUri: `${globalThis.location.origin}/silent-check-sso.html`,
                    silentCheckSsoFallback: false,
                    checkLoginIframe: false,
                    pkceMethod: "S256",
                    scope: this.scope,
                    ...stored,
                }),
                new Promise<never>((_, reject) =>
                    setTimeout(
                        () => reject(new Error("silent check-sso timed out")),
                        INIT_TIMEOUT_MS
                    )
                ),
            ]);
        } catch (error) {
            logger.warn(
                `Keycloak init failed; continuing unauthenticated: ${describeError(error)}`
            );
        }
        this.persistTokens();
    }

    private persistTokens() {
        try {
            if (this.keycloak.authenticated && this.keycloak.token) {
                sessionStorage.setItem(
                    TOKEN_STORAGE_KEY,
                    JSON.stringify({
                        token: this.keycloak.token,
                        refreshToken: this.keycloak.refreshToken,
                        idToken: this.keycloak.idToken,
                    })
                );
            } else {
                sessionStorage.removeItem(TOKEN_STORAGE_KEY);
            }
        } catch {
            // storage unavailable
        }
    }

    async signIn(
        redirectPath: string,
        idpHint?: string
    ): Promise<OidcTokenDetails> {
        const existing = this.getOidcTokenDetails();
        if (existing !== null) {
            logger.verbose("Already authenticated");
            return existing;
        }
        logger.verbose(
            "Not yet authenticated; redirecting to Keycloak login..."
        );
        const callbackUri = `${this.logonCallback}?redirect=${encodeURI(redirectPath)}`;
        await this.keycloak.login({
            scope: this.scope,
            redirectUri: callbackUri,
            idpHint,
        });
        // keycloak.login() navigates away; reaching this line means the redirect failed.
        throw new Error("Redirect to Keycloak login page failed");
    }

    signOut(): Promise<void> {
        try {
            sessionStorage.removeItem(TOKEN_STORAGE_KEY);
        } catch {
            // storage unavailable
        }
        return this.keycloak.logout({ redirectUri: this.logoutCallback });
    }

    refreshToken(): Promise<boolean> {
        return this.keycloak.updateToken(REFRESH_CUSHION + 5);
    }

    getOidcTokenDetails(): OidcTokenDetails | null {
        if (!this.keycloak.authenticated) return null;
        const now = Math.ceil(Date.now() / 1000);
        const exp = this.keycloak.tokenParsed?.exp ?? now;
        const skew = this.keycloak.timeSkew ?? 0;
        return {
            idToken: this.keycloak.idToken ?? "",
            sessionState: this.keycloak.sessionId,
            refreshToken: this.keycloak.refreshToken,
            accessToken: this.keycloak.token ?? "",
            expired: this.keycloak.isTokenExpired(),
            refreshTokenTime: exp + skew - REFRESH_CUSHION,
            hdid:
                (this.keycloak.idTokenParsed as { hdid?: string } | undefined)
                    ?.hdid ?? "",
        };
    }

    async getOidcUserInfo(): Promise<OidcUserInfo> {
        if (!this.keycloak.userInfo) await this.keycloak.loadUserInfo();
        return this.keycloak.userInfo as OidcUserInfo;
    }

    clearState(): void {
        void this.keycloak.clearToken();
        this.persistTokens();
    }
}

export function createAuthService(
    oidcConfig: OpenIdConnectConfiguration
): IAuthService {
    return new WebAuthService(oidcConfig);
}
