// Web: the tokens live only in memory (the keycloak-js instance). Nothing here is a credential; these are hints that
// decide whether a page load may leave the app for Keycloak to restore the session.

/** localStorage: this browser had a Keycloak session at its last page load. Set at sign-in, cleared at sign-out. */
const HAD_SESSION_KEY = "hg.auth.hadSession";

/** sessionStorage: when this tab last left for Keycloak's full-page check-sso, to stop a redirect loop. */
const SSO_REDIRECT_KEY = "hg.auth.ssoRedirectAt";

/** How long a check-sso redirect counts as just made: longer than a slow round trip to Keycloak and back. */
const SSO_REDIRECT_WINDOW_MS = 60_000;

/** sessionStorage key of the tokens kept by earlier builds; removed so none outlive this change. */
const LEGACY_TOKEN_KEY = "hg.oidc.web.tokens";

function read(storage: () => Storage, key: string): string | null {
    try {
        return storage().getItem(key);
    } catch {
        return null; // storage unavailable
    }
}

function write(storage: () => Storage, key: string, value: string | null) {
    try {
        if (value === null) storage().removeItem(key);
        else storage().setItem(key, value);
    } catch {
        // storage unavailable
    }
}

const local = () => globalThis.localStorage;
const session = () => globalThis.sessionStorage;

export function readHadSession(): boolean {
    return read(local, HAD_SESSION_KEY) === "true";
}

export function setHadSession(value: boolean): void {
    write(local, HAD_SESSION_KEY, value ? "true" : null);
}

/** True when this tab left for Keycloak's check-sso within the last minute and has not finished loading since. */
export function isSsoRedirectRecent(now: number): boolean {
    const at = Number(read(session, SSO_REDIRECT_KEY));
    return at > 0 && now - at >= 0 && now - at < SSO_REDIRECT_WINDOW_MS;
}

export function markSsoRedirect(now: number): void {
    write(session, SSO_REDIRECT_KEY, String(now));
}

export function clearSsoRedirect(): void {
    write(session, SSO_REDIRECT_KEY, null);
}

export function removeLegacyTokens(): void {
    write(session, LEGACY_TOKEN_KEY, null);
}

/** Forgets the session hints. Needs no service, so a reset works before the app has started. */
export function clearStoredSession(): Promise<void> {
    setHadSession(false);
    clearSsoRedirect();
    removeLegacyTokens();
    return Promise.resolve();
}
