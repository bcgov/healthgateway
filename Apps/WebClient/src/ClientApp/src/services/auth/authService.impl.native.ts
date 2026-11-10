import * as AuthSession from "expo-auth-session";
import * as SecureStore from "expo-secure-store";
import * as WebBrowser from "expo-web-browser";

import type { OpenIdConnectConfiguration } from "@/models/configData";
import { HttpError, SessionEndedError } from "@/models/errors";
import type { OidcTokenDetails, OidcUserInfo } from "@/models/user";
import { describeError, logger } from "@/services/logger";

import { classifyAuthFailure } from "./authFailure";
import {
    type AuthInitOptions,
    type IAuthService,
    REFRESH_CUSHION,
    type RestoreFailure,
} from "./authService";
import { decodeJwtPayload } from "./jwt";
import {
    clearStoredSession,
    inOrder,
    STORAGE_KEY,
} from "./storedSession.native";

const SCHEME = "healthgateway";

interface StoredSession {
    accessToken: string;
    refreshToken?: string;
    idToken: string;
    /** Unix seconds. */
    expiresAt: number;
    sessionState?: string;
}

const DEFAULT_LIFETIME_SECONDS = 300;

/** Keycloak answered, but the answer holds no usable tokens. Classified as unexpected: see `authFailure.ts`. */
export class MalformedTokenError extends Error {
    constructor() {
        super("The token response was not usable");
        this.name = "MalformedTokenError";
    }
}

export class AuthCancelledError extends Error {
    constructor() {
        super("Login was cancelled");
        this.name = "AuthCancelledError";
    }
}

/**
 * Native implementation: Authorization Code + PKCE through expo-auth-session.
 * - The auth sheet is opened with `preferEphemeralSession: true`, so iOS does not show the
 *   "wants to use … to sign in" consent sheet and no cookies are shared with Safari.
 * - Logout is done server-side (POST end_session with the refresh token); no browser is opened.
 * - Tokens live in expo-secure-store (Keychain / Keystore).
 */
export class NativeAuthService implements IAuthService {
    private discovery?: AuthSession.DiscoveryDocument;
    private session?: StoredSession;
    private userInfo?: OidcUserInfo;
    private restoreFailure?: RestoreFailure;
    private refreshing?: Promise<boolean>;
    /** Raised when the session goes, so what a call started under it comes back to can be told apart. */
    private login = 0;
    private readonly redirectUri = AuthSession.makeRedirectUri({
        scheme: SCHEME,
        path: "loginCallback",
    });
    private readonly postLogoutRedirectUri = AuthSession.makeRedirectUri({
        scheme: SCHEME,
        path: "logoutComplete",
    });

    constructor(private readonly oidcConfig: OpenIdConnectConfiguration) {}

    getRestoreFailure(): RestoreFailure | undefined {
        return this.restoreFailure;
    }

    async init(options?: AuthInitOptions): Promise<void> {
        const login = this.login;
        try {
            const raw = await inOrder(() =>
                SecureStore.getItemAsync(STORAGE_KEY)
            );
            if (raw && login === this.login)
                this.session = JSON.parse(raw) as StoredSession;
        } catch (error) {
            logger.warn(
                `Unable to read stored session: ${describeError(error)}`
            );
        }
        if (this.session && this.isExpired() && this.session.refreshToken) {
            try {
                await this.refreshToken(true);
            } catch (error) {
                if (options?.keepSessionOnFailure) {
                    // only a lost connection may keep a login going; a response that makes no sense must not
                    this.restoreFailure =
                        classifyAuthFailure(error) === "connection"
                            ? "network"
                            : "rejected";
                } else {
                    await this.clearSession();
                }
            }
        }
    }

    private async getDiscovery(): Promise<AuthSession.DiscoveryDocument> {
        if (!this.discovery) {
            this.discovery = await AuthSession.fetchDiscoveryAsync(
                this.oidcConfig.authority
            );
        }
        return this.discovery;
    }

    async signIn(
        _redirectPath: string,
        idpHint?: string
    ): Promise<OidcTokenDetails> {
        const existing = this.getOidcTokenDetails();
        if (existing && !existing.expired) return existing;

        // signing out or resetting while this waits must not bring its login back, now or on the next launch
        const login = this.login;
        const discovery = await this.getDiscovery();
        if (login !== this.login) throw new SessionEndedError();
        const request = new AuthSession.AuthRequest({
            clientId: this.oidcConfig.clientId,
            redirectUri: this.redirectUri,
            responseType: AuthSession.ResponseType.Code,
            codeChallengeMethod: AuthSession.CodeChallengeMethod.S256,
            usePKCE: true,
            scopes: this.oidcConfig.scope.split(" ").filter(Boolean),
            extraParams: idpHint ? { kc_idp_hint: idpHint } : {},
        });

        logger.verbose(`Opening auth session, redirect ${this.redirectUri}`);
        const result = await request.promptAsync(discovery, {
            preferEphemeralSession: true,
        });
        if (login !== this.login) throw new SessionEndedError();
        if (result.type !== "success") {
            // "cancel"/"dismiss" = the sheet closed without returning to the redirect URI; "error" carries the
            // provider's error. Log enough to tell those apart when a login "just fails".
            const detail =
                result.type === "error" ? ` ${result.error?.code ?? ""}` : "";
            logger.warn(
                `Auth session ended with ${result.type}${detail} (redirect ${this.redirectUri})`
            );
            throw new AuthCancelledError();
        }
        logger.debug(
            `Auth session returned to ${this.redirectUri}; exchanging code`
        );
        if (result.params.error) {
            throw new Error(
                result.params.error_description ?? result.params.error
            );
        }

        const token = await AuthSession.exchangeCodeAsync(
            {
                clientId: this.oidcConfig.clientId,
                code: result.params.code,
                redirectUri: this.redirectUri,
                extraParams: { code_verifier: request.codeVerifier ?? "" },
            },
            discovery
        );
        if (login !== this.login) {
            // Keycloak has just started a login nobody wants: it is not kept here, so do not leave it open there
            void this.endKeycloakSession(token.refreshToken);
            throw new SessionEndedError();
        }
        await this.storeTokenResponse(token);
        if (login !== this.login) throw new SessionEndedError();
        this.userInfo = undefined;
        return this.getOidcTokenDetails()!;
    }

    async signOut(): Promise<void> {
        const session = this.session;
        await this.clearSession();
        await this.endKeycloakSession(session?.refreshToken);
    }

    private async endKeycloakSession(refreshToken?: string): Promise<void> {
        if (!refreshToken) return;
        try {
            const discovery = await this.getDiscovery();
            if (!discovery.endSessionEndpoint) return;
            const body = new URLSearchParams({
                client_id: this.oidcConfig.clientId,
                refresh_token: refreshToken,
            });
            const response = await fetch(discovery.endSessionEndpoint, {
                method: "POST",
                headers: {
                    "Content-Type": "application/x-www-form-urlencoded",
                },
                body: body.toString(),
            });
            if (!response.ok)
                logger.warn(`Keycloak logout returned ${response.status}`);
        } catch (error) {
            logger.warn(`Keycloak logout failed: ${describeError(error)}`);
        }
    }

    async refreshToken(force = false): Promise<boolean> {
        if (!this.session?.refreshToken) return false;
        const secondsLeft =
            this.session.expiresAt - Math.ceil(Date.now() / 1000);
        if (!force && secondsLeft > REFRESH_CUSHION + 5) return false;
        // One at a time: if Keycloak rotates refresh tokens, a second use of the same one is refused and ends the login
        this.refreshing ??= this.performRefresh(
            this.session.refreshToken
        ).finally(() => {
            this.refreshing = undefined;
        });
        return this.refreshing;
    }

    private async performRefresh(refreshToken: string): Promise<boolean> {
        const discovery = await this.getDiscovery();
        const token = await AuthSession.refreshAsync(
            { clientId: this.oidcConfig.clientId, refreshToken },
            discovery
        );
        // signed out while this was on its way: its tokens must not bring the login back
        if (this.session?.refreshToken !== refreshToken) return false;
        await this.storeTokenResponse(token, this.session);
        return true;
    }

    getOidcTokenDetails(): OidcTokenDetails | null {
        const s = this.session;
        if (!s) return null;
        const claims = decodeJwtPayload<{ hdid?: string }>(s.idToken);
        return {
            idToken: s.idToken,
            sessionState: s.sessionState,
            refreshToken: s.refreshToken,
            accessToken: s.accessToken,
            expired: this.isExpired(),
            refreshTokenTime: s.expiresAt - REFRESH_CUSHION,
            hdid: claims?.hdid ?? "",
        };
    }

    async getOidcUserInfo(): Promise<OidcUserInfo> {
        if (this.userInfo) return this.userInfo;
        // this login's: signing out while waiting must not leave its identity behind for the next
        const login = this.login;
        const discovery = await this.getDiscovery();
        if (login !== this.login) throw new SessionEndedError();
        if (!discovery.userInfoEndpoint || !this.session) {
            throw new Error("Not authenticated");
        }
        const response = await fetch(discovery.userInfoEndpoint, {
            headers: { Authorization: `Bearer ${this.session.accessToken}` },
        });
        if (login !== this.login) throw new SessionEndedError();
        if (!response.ok)
            throw new HttpError("Unable to load user info", response.status);
        const info = (await response.json()) as OidcUserInfo;
        if (login !== this.login) throw new SessionEndedError();
        this.userInfo = info;
        return info;
    }

    clearState(): void {
        void this.clearSession();
    }

    private isExpired(): boolean {
        return (
            !this.session ||
            this.session.expiresAt <= Math.ceil(Date.now() / 1000)
        );
    }

    private async storeTokenResponse(
        token: AuthSession.TokenResponse,
        previous?: StoredSession
    ) {
        // expo-auth-session maps a successful answer without checking it: a broken one must not replace a working login
        const idToken = token.idToken ?? previous?.idToken;
        const lifetime = token.expiresIn ?? DEFAULT_LIFETIME_SECONDS;
        if (
            typeof token.accessToken !== "string" ||
            !token.accessToken ||
            !idToken ||
            typeof lifetime !== "number" ||
            !(lifetime > 0)
        )
            throw new MalformedTokenError();
        this.restoreFailure = undefined;
        const issuedAt = token.issuedAt ?? Math.floor(Date.now() / 1000);
        const session: StoredSession = {
            accessToken: token.accessToken,
            refreshToken: token.refreshToken ?? previous?.refreshToken,
            idToken,
            expiresAt: issuedAt + lifetime,
            sessionState:
                (token as { sessionState?: string }).sessionState ??
                previous?.sessionState,
        };
        this.session = session;
        const login = this.login;
        await inOrder(async () => {
            if (login !== this.login) return;
            try {
                await SecureStore.setItemAsync(
                    STORAGE_KEY,
                    JSON.stringify(session),
                    // each device logs in for itself: never restored onto another from a backup
                    {
                        keychainAccessible:
                            SecureStore.WHEN_UNLOCKED_THIS_DEVICE_ONLY,
                    }
                );
            } catch (error) {
                logger.warn(
                    `Unable to persist session: ${describeError(error)}`
                );
            }
        });
    }

    private async clearSession() {
        this.login += 1;
        this.session = undefined;
        this.userInfo = undefined;
        // logout and expiry still end the session in memory; Reset App deletes the stored copy itself and stops on
        // a failure (resetApp)
        try {
            await clearStoredSession();
        } catch (error) {
            logger.warn(
                `Unable to delete the stored session: ${describeError(error)}`
            );
        }
    }
}

// Allows Android Custom Tabs to warm up; harmless elsewhere.
void WebBrowser.warmUpAsync?.();

export function createAuthService(
    oidcConfig: OpenIdConnectConfiguration
): IAuthService {
    return new NativeAuthService(oidcConfig);
}
