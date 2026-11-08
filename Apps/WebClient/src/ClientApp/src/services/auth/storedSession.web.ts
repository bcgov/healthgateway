/** sessionStorage key of the web tokens. */
export const TOKEN_STORAGE_KEY = "hg.oidc.web.tokens";

/** Deletes the stored tokens. Needs no service, so a reset works before the app has started. */
export function clearStoredSession(): Promise<void> {
    try {
        sessionStorage.removeItem(TOKEN_STORAGE_KEY);
    } catch {
        // storage unavailable
    }
    return Promise.resolve();
}
