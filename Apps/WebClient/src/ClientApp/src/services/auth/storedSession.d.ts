// Type façade for the platform-specific stored-session helpers (storedSession.web.ts / .native.ts), which Metro picks
// by platform extension. Kept apart from the auth services so callers do not load keycloak-js or expo-auth-session.

/** Deletes the stored session without needing an initialized auth service; rejects if it may still be stored. */
export function clearStoredSession(): Promise<void>;
