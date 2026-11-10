import * as SecureStore from "expo-secure-store";

/** SecureStore key of the native session (tokens and expiry). */
export const STORAGE_KEY = "hg.oidc.session";

/**
 * Calls on the stored session run in the order they were asked for, so a slow write cannot finish after the delete
 * that should have removed it. Module level: a new service can replace one whose write is still running.
 */
let storage: Promise<unknown> = Promise.resolve();
export function inOrder<T>(work: () => Promise<T>): Promise<T> {
    const run = storage.then(work);
    storage = run.catch(() => undefined);
    return run;
}

/** Deletes the stored session, in order with the service's own writes. Needs no service, so a reset works before
 * the app has started (configuration down). SecureStore resolves when nothing is stored, so a rejection means the
 * session may still be there: it is passed on, and a reset stops rather than leave a login for the next start. */
export function clearStoredSession(): Promise<void> {
    return inOrder(() => SecureStore.deleteItemAsync(STORAGE_KEY));
}
