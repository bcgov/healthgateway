import * as AuthSession from "expo-auth-session";
import * as SecureStore from "expo-secure-store";
import * as WebBrowser from "expo-web-browser";

import type { OpenIdConnectConfiguration } from "@/models/configData";
import { HttpError, SessionEndedError } from "@/models/errors";
import type { OidcTokenDetails, OidcUserInfo } from "@/models/user";
import { describeError, logger } from "@/services/logger";
import {
    isVaultCancelled,
    isVaultCorrupt,
    isVaultKeyGone,
    vault,
} from "@/services/vault";

import { classifyAuthFailure } from "./authFailure";
import {
    type AuthInitOptions,
    type IAuthService,
    REFRESH_CUSHION,
    type RestoreFailure,
    type UnsealOptions,
    type UnsealResult,
} from "./authService";
import { decodeJwtPayload } from "./jwt";
import {
    clearStoredSession,
    inOrder,
    SEALED_STORAGE_KEY,
    STORAGE_KEY,
} from "./storedSession.native";

const SCHEME = "healthgateway";

interface StoredSession {
    accessToken: string;
    refreshToken?: string;
    idToken: string;
    /** Unix seconds. */
    expiresAt: number;
    /**
     * Unix seconds, when the tokens were issued: which of two stored copies is newer (`init`). Not `expiresAt`,
     * which Keycloak can bring forward to the end of its session. Missing from a login stored by an earlier build.
     */
    issuedAt?: number;
    sessionState?: string;
}

/**
 * What is stored under `SEALED_STORAGE_KEY`: the session encrypted by the vault, and when its tokens were issued, in
 * the clear, so it can be compared with an unsealed copy without the prompt.
 */
interface SealedCopy {
    issuedAt?: number;
    data: string;
}

const DEFAULT_LIFETIME_SECONDS = 300;

/** Each device logs in for itself: never restored onto another from a backup. */
const STORE_OPTIONS: SecureStore.SecureStoreOptions = {
    keychainAccessible: SecureStore.WHEN_UNLOCKED_THIS_DEVICE_ONLY,
};

/** What was unsealed, if it is a session: a damaged copy must not go into memory as one. */
function parseSession(text: string): StoredSession | undefined {
    let value: Partial<StoredSession> | null;
    try {
        value = JSON.parse(text) as Partial<StoredSession> | null;
    } catch {
        return undefined;
    }
    if (
        !value ||
        typeof value.accessToken !== "string" ||
        !value.accessToken ||
        typeof value.idToken !== "string" ||
        !value.idToken ||
        typeof value.expiresAt !== "number" ||
        (value.issuedAt !== undefined && typeof value.issuedAt !== "number") ||
        (value.refreshToken !== undefined &&
            typeof value.refreshToken !== "string")
    )
        return undefined;
    return value as StoredSession;
}

/** The stored sealed copy, if it is one: a damaged one has nothing to decrypt. */
function parseSealedCopy(text: string): SealedCopy | undefined {
    let value: Partial<SealedCopy> | null;
    try {
        value = JSON.parse(text) as Partial<SealedCopy> | null;
    } catch {
        return undefined;
    }
    if (
        !value ||
        typeof value.data !== "string" ||
        (value.issuedAt !== undefined && typeof value.issuedAt !== "number")
    )
        return undefined;
    return value as SealedCopy;
}

/**
 * The unsealed copy left beside a sealed one holds newer tokens: sealing them failed, and so did removing the sealed
 * copy (`persist`). Otherwise it is left over from a move, or from a save the sealed copy has since replaced. Without
 * a time to go by, or with a damaged sealed copy, it is not taken for newer.
 */
function isNewerThan(unsealed: StoredSession, sealed?: SealedCopy): boolean {
    if (unsealed.issuedAt === undefined || !sealed) return false;
    // only a login stored by an earlier build has no time, and every save of new tokens since has one
    return sealed.issuedAt === undefined || unsealed.issuedAt > sealed.issuedAt;
}

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
 * - Tokens live in expo-secure-store (Keychain / Keystore). With device security on they are first encrypted by the vault,
 *   the key that seals the saved records, so reading them takes the same prompt; and while the app is locked they are
 *   not in memory either (`seal` / `unseal`).
 */
export class NativeAuthService implements IAuthService {
    private discovery?: AuthSession.DiscoveryDocument;
    private session?: StoredSession;
    private userInfo?: OidcUserInfo;
    private restoreFailure?: RestoreFailure;
    private refreshing?: Promise<boolean>;
    /** Raised when the session goes, so what a call started under it comes back to can be told apart. */
    private login = 0;
    /** Device security is on: the session is stored sealed by the vault. */
    private protect = false;
    /** The stored session is sealed and not in memory: `unseal` brings it back. */
    private sealedCopy = false;
    /**
     * The session in memory is stored sealed, so it may leave memory; one that could only be stored unsealed may not.
     * Undefined until this service has stored one: a copy of the other kind may be left from an earlier login, and the
     * first save removes it (`write`).
     */
    private storedSealed?: boolean;
    /** An unsealed copy could not be removed from beside the sealed login: the next save, or unsealing, tries again. */
    private unsealedCopyLeft = false;
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
        this.protect = options?.sealed === true;
        const login = this.login;
        // with device security off, a sealed copy is not read: the next login replaces it
        const sealed = this.protect ? await this.readSealedCopy() : null;
        if (login !== this.login) return;
        const unsealed = await this.readUnsealedCopy();
        if (login !== this.login) return;
        // A copy left unsealed beside a sealed login is usually older, and goes. One with newer tokens is the login
        // (`isNewerThan`), and is sealed below in place of the old one, or the refresh token it holds would be lost.
        if (
            sealed &&
            !(unsealed && isNewerThan(unsealed, parseSealedCopy(sealed)))
        ) {
            this.sealedCopy = true;
            await this.removeUnsealedCopy();
            return;
        }
        this.session = unsealed;
        if (this.session && this.protect) {
            // kept unsealed by an earlier build, or newer than the sealed copy: sealed now, and opened after the prompt
            // like any other
            await this.persist(this.session, login);
            if (login !== this.login) return;
            if (this.storedSealed) {
                this.dropFromMemory();
                return;
            }
        }
        await this.restore(options?.keepSessionOnFailure);
    }

    /**
     * Device security on: the stored sealed session, if there is one. Rejects when the store could not be read: taken as
     * "none", a sealed login would look signed out, and the next login would replace it. Start-up stops instead, and can
     * be tried again.
     */
    private async readSealedCopy(): Promise<string | null> {
        try {
            return await inOrder(() =>
                SecureStore.getItemAsync(SEALED_STORAGE_KEY)
            );
        } catch (error) {
            logger.warn(
                `Unable to read the sealed session: ${describeError(error)}`
            );
            throw error;
        }
    }

    /**
     * The stored unsealed session, if it holds one. One that could not be read is taken as none: signed out, or older
     * than a sealed copy beside it.
     */
    private async readUnsealedCopy(): Promise<StoredSession | undefined> {
        try {
            const raw = await inOrder(() =>
                SecureStore.getItemAsync(STORAGE_KEY)
            );
            return raw ? parseSession(raw) : undefined;
        } catch (error) {
            logger.warn(
                `Unable to read stored session: ${describeError(error)}`
            );
            return undefined;
        }
    }

    /**
     * The unsealed copy left beside a sealed login goes. One that could not be removed is remembered, so unsealing and
     * every save after it try again: otherwise the tokens would stay outside the vault for as long as the login lasts.
     */
    private async removeUnsealedCopy(): Promise<void> {
        try {
            await inOrder(() => SecureStore.deleteItemAsync(STORAGE_KEY));
            this.unsealedCopyLeft = false;
        } catch (error) {
            this.unsealedCopyLeft = true;
            logger.warn(
                `Unable to delete the unsealed session: ${describeError(error)}`
            );
        }
    }

    /** An expired access token is refreshed straight away. */
    private async restore(keepSessionOnFailure?: boolean): Promise<void> {
        if (this.session && this.isExpired() && this.session.refreshToken) {
            try {
                await this.refreshToken(true);
            } catch (error) {
                if (keepSessionOnFailure) {
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

    isSealed(): boolean {
        return this.sealedCopy;
    }

    async unseal(options?: UnsealOptions): Promise<UnsealResult> {
        if (!this.sealedCopy) return this.session ? "unsealed" : "unreadable";
        const login = this.login;
        let text: string | undefined;
        try {
            const stored = await inOrder(() =>
                SecureStore.getItemAsync(SEALED_STORAGE_KEY)
            );
            const copy = stored ? parseSealedCopy(stored) : undefined;
            if (copy) text = await vault.decrypt(copy.data);
        } catch (error) {
            if (isVaultCancelled(error)) return "cancelled";
            logger.warn(
                `Unable to unseal the stored session: ${describeError(error)}`
            );
            // signed out or reset while the prompt was open
            if (login !== this.login) return "unreadable";
            // only a key gone for good, or a damaged copy, ends the login: anything else may pass at the next try, and
            // it stays sealed
            if (!isVaultKeyGone(error) && !isVaultCorrupt(error))
                return "unavailable";
        }
        if (login !== this.login) return "unreadable";
        const session = text === undefined ? undefined : parseSession(text);
        if (!session) {
            // nothing will ever read it: the key went with the device's security, or the copy is missing or damaged
            await this.clearSession();
            return "unreadable";
        }
        this.session = session;
        this.sealedCopy = false;
        this.storedSealed = true;
        if (this.unsealedCopyLeft) await this.removeUnsealedCopy();
        if (login !== this.login) return "unreadable";
        if (options?.restore) await this.restore(true);
        return "unsealed";
    }

    seal(): Promise<void> {
        return this.sealFor(this.login);
    }

    /** `login`: the one the lock went on over. Signed out while this waited, the next login is not its to seal. */
    private async sealFor(login: number): Promise<void> {
        // a refresh on its way: its tokens are saved before they go, or a rotated refresh token would be lost with them
        while (this.refreshing) {
            try {
                await this.refreshing;
            } catch {
                // its caller hears of it
            }
        }
        // and any save still running has finished
        await inOrder(() => Promise.resolve());
        if (login !== this.login) return;
        if (this.refreshing) return this.sealFor(login);
        if (!this.protect || !this.session || !this.storedSealed) return;
        this.dropFromMemory();
    }

    private dropFromMemory() {
        this.session = undefined;
        this.userInfo = undefined;
        this.sealedCopy = true;
    }

    async setSealed(sealed: boolean): Promise<void> {
        const previous = this.protect;
        this.protect = sealed;
        const session = this.session;
        if (!session) return;
        const login = this.login;
        try {
            await inOrder(async () => {
                if (login === this.login)
                    await this.write(session, sealed, true);
            });
        } catch (error) {
            this.protect = previous;
            throw error;
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
            issuedAt,
            sessionState:
                (token as { sessionState?: string }).sessionState ??
                previous?.sessionState,
        };
        this.session = session;
        await this.persist(session, this.login);
    }

    /** Stores the session, sealed when device security is on. A failure is logged: the session still works in memory. */
    private async persist(session: StoredSession, login: number) {
        await inOrder(async () => {
            if (login !== this.login) return;
            try {
                await this.write(session, this.protect);
                return;
            } catch (error) {
                if (this.protect)
                    // an error, not a warning: device security is on, and the login is about to be stored without it
                    logger.error(
                        `Unable to seal session: ${describeError(error)}`
                    );
                else
                    logger.warn(
                        `Unable to persist session: ${describeError(error)}`
                    );
            }
            if (!this.protect) return;
            // still there at the next start, which seals it then; until then it can't leave memory (`seal`)
            try {
                await this.write(session, false);
            } catch (error) {
                logger.warn(
                    `Unable to persist session: ${describeError(error)}`
                );
            }
        });
    }

    /**
     * Call in order (`inOrder`). The other copy goes once this one is saved, so a failure between never leaves neither.
     * It must not leave both for long either: an unsealed copy beside a sealed one is a login on disk without the
     * protection the setting promises, and a sealed one left beside a newer unsealed one is older than the login (the next
     * start tells them apart by `issuedAt`: `init`). So an other copy that could not be removed is remembered,
     * and removed at the next save.
     *
     * `move`: Device Security is being turned on or off, and the caller must know whether it worked. The session is the
     * same in both copies, so a move that can't finish is undone and rejects. A save of new tokens is never undone,
     * which would put the old ones back on disk: it stands, and only the clearing up is left for later.
     */
    private async write(session: StoredSession, sealed: boolean, move = false) {
        const text = JSON.stringify(session);
        if (sealed) {
            const copy: SealedCopy = {
                issuedAt: session.issuedAt,
                data: await vault.encrypt(text),
            };
            await SecureStore.setItemAsync(
                SEALED_STORAGE_KEY,
                JSON.stringify(copy),
                STORE_OPTIONS
            );
        } else {
            await SecureStore.setItemAsync(STORAGE_KEY, text, STORE_OPTIONS);
        }
        if (this.storedSealed === sealed && !(sealed && this.unsealedCopyLeft))
            return;
        const [saved, other] = sealed
            ? [SEALED_STORAGE_KEY, STORAGE_KEY]
            : [STORAGE_KEY, SEALED_STORAGE_KEY];
        try {
            await SecureStore.deleteItemAsync(other);
        } catch (error) {
            if (move && this.storedSealed === !sealed) {
                let undone = false;
                try {
                    await SecureStore.deleteItemAsync(saved);
                    undone = true;
                } catch (undoError) {
                    logger.warn(
                        `Unable to undo the stored session's move: ${describeError(undoError)}`
                    );
                }
                // as it was before the move
                if (undone) throw error;
            }
            // both are there. Sealed, the new copy is the login and the unsealed one is only left over. Unsealed, it
            // is no longer known that this is the only copy, so every save tries again, and the tokens stay in memory
            // under the lock (`seal`)
            if (sealed) {
                this.storedSealed = true;
                this.unsealedCopyLeft = true;
            } else {
                this.storedSealed = undefined;
            }
            if (move) throw error;
            logger.warn(
                `Unable to delete the stored session's other copy: ${describeError(error)}`
            );
            return;
        }
        this.storedSealed = sealed;
        if (sealed) this.unsealedCopyLeft = false;
    }

    private async clearSession() {
        this.login += 1;
        this.session = undefined;
        this.userInfo = undefined;
        this.sealedCopy = false;
        this.storedSealed = undefined;
        this.unsealedCopyLeft = false;
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
