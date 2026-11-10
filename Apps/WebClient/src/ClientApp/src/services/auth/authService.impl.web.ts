import Keycloak, { type KeycloakConfig } from "keycloak-js";

import type { OpenIdConnectConfiguration } from "@/models/configData";
import type { OidcTokenDetails, OidcUserInfo } from "@/models/user";
import { describeError, logger } from "@/services/logger";

import { type IAuthService, REFRESH_CUSHION } from "./authService";
import {
    clearSsoRedirect,
    isSsoRedirectRecent,
    markSsoRedirect,
    readHadSession,
    removeLegacyTokens,
    setHadSession,
} from "./storedSession.web";

const INIT_TIMEOUT_MS = 5000;

/** keycloak-js's third-party cookie check gives up after this, inside INIT_TIMEOUT_MS: a check that answers late
 * cannot start a full-page redirect after the app has carried on signed out. */
const THIRD_PARTY_COOKIE_CHECK_TIMEOUT_MS = 4000;

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
            setHadSession(true);
        };
        kc.onAuthError = (e) =>
            logger.error(`Keycloak: onAuthError - ${e?.error}`);
        kc.onAuthRefreshSuccess = () =>
            logger.verbose("Keycloak: onAuthRefreshSuccess");
        kc.onAuthRefreshError = () =>
            logger.verbose("Keycloak: onAuthRefreshError");
        kc.onAuthLogout = () => {
            logger.verbose("Keycloak: onAuthLogout");
            setHadSession(false);
        };
        kc.onTokenExpired = () => logger.verbose("Keycloak: onTokenExpired");
    }

    async init(): Promise<void> {
        if (this.initialized) return;
        this.initialized = true;
        removeLegacyTokens();
        // The tokens live only in memory, so every page load asks Keycloak for them again (check-sso).
        // - Third-party cookies allowed: a hidden iframe (silent-check-sso.html) asks, and the page stays.
        // - Blocked (Safari, Firefox strict, Brave): the iframe cannot see Keycloak's session, so keycloak-js falls
        //   back to a full-page prompt=none redirect that comes back to this URL. That is only worth it when this
        //   browser was signed in at its last load (hadSession): anyone else stays on the page, signed out.
        // - A redirect made from this tab moments ago that came back without its callback must not repeat, or the
        //   tab would loop: this load stays, signed out.
        // keycloak-js also parses the ?code=&state= on /loginCallback, and the fallback's #state=, here.
        // If the silent iframe never answers (e.g. this origin is not a valid redirect URI yet, so Keycloak renders
        // an error page inside it), give up after INIT_TIMEOUT_MS and stay signed out.
        const now = Date.now();
        const fallback = readHadSession() && !isSsoRedirectRecent(now);
        if (fallback) markSsoRedirect(now);
        const keycloakInit = this.keycloak.init({
            onLoad: "check-sso",
            silentCheckSsoRedirectUri: `${globalThis.location.origin}/silent-check-sso.html`,
            silentCheckSsoFallback: fallback,
            messageReceiveTimeout: THIRD_PARTY_COOKIE_CHECK_TIMEOUT_MS,
            checkLoginIframe: false,
            pkceMethod: "S256",
            scope: this.scope,
        });
        // Settling means the page did not leave for Keycloak. Only an answer from Keycloak changes hadSession: a
        // failure (Keycloak unreachable) says nothing about the session.
        void keycloakInit.then(
            (authenticated) => {
                clearSsoRedirect();
                setHadSession(authenticated);
            },
            () => clearSsoRedirect()
        );
        try {
            await Promise.race([
                keycloakInit,
                new Promise<never>((_, reject) =>
                    setTimeout(
                        () => reject(new Error("check-sso timed out")),
                        INIT_TIMEOUT_MS
                    )
                ),
            ]);
        } catch (error) {
            logger.warn(
                `Keycloak init failed; continuing unauthenticated: ${describeError(error)}`
            );
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
        setHadSession(false);
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
        setHadSession(false);
    }
}

export function createAuthService(
    oidcConfig: OpenIdConnectConfiguration
): IAuthService {
    return new WebAuthService(oidcConfig);
}
