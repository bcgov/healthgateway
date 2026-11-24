import * as SecureStore from "expo-secure-store";
import { Platform } from "react-native";
import { create } from "zustand";

import { describeError, logger } from "@/services/logger";
import { recordCache } from "@/services/recordCache";
import { vault, type VaultUnlock } from "@/services/vault";

const ENABLED_KEY = "hg.appLock.enabled";

/**
 * What the lock does to the login, given by the auth side (`useAppLockLifecycle`), which this store can't import.
 * The tokens are sealed by the vault, like the saved records, and only in memory while the app is unlocked.
 */
export interface SessionLock {
    /** The lock went on: the login's tokens leave memory. */
    seal: () => Promise<void>;
    /** The prompt passed: the tokens come back. False keeps the lock on. */
    unseal: () => Promise<boolean>;
    /** Device security turned on or off: the stored login moves to match. Rejects when it could not. */
    protect: (on: boolean) => Promise<void>;
}

let sessionLock: SessionLock | undefined;

/** Returns the way to take them back off. */
export function setSessionLock(hooks: SessionLock): () => void {
    sessionLock = hooks;
    return () => {
        if (sessionLock === hooks) sessionLock = undefined;
    };
}

interface AppLockState {
    /** The user opted in (device preference, kept across sign-outs). */
    enabled: boolean;
    /** The lock screen covers the app. Never true on web. */
    locked: boolean;
    /** A system prompt is open; a second one would cancel it. */
    authenticating: boolean;
    /**
     * Runs after the prompt passes and before the lock lifts: opening the sealed login, or the saved copy on a launch with
     * no working login. Resolving false keeps the lock on, and it runs again at the next unlock.
     */
    beforeUnlock?: () => Promise<boolean | void>;
    backgroundedAt?: number;
    hydrate: () => Promise<void>;
    /** Cold start: a saved session must be unlocked before the app shows. */
    lockOnLaunch: (signedIn: boolean) => void;
    appBackgrounded: (now: number) => void;
    /** `idleMs`: away this long or less does not ask again. */
    appActivated: (signedIn: boolean, now: number, idleMs: number) => void;
    /** Only the system prompt: what came of it. */
    authenticate: () => Promise<VaultUnlock>;
    /** Each of these asks for the system prompt and reports whether it passed. */
    unlock: () => Promise<boolean>;
    enable: () => Promise<boolean>;
    disable: () => Promise<boolean>;
    /** The idle timer ran out: the lock replaces the logout, keeping the tokens. */
    lock: () => void;
    clearLock: () => void;
    /** Reset App: forgets the opt-in without asking, as the user has confirmed and is signed out by now. */
    reset: () => Promise<void>;
}

let revision = 0;

/** Runs `work`, logging what could not be done instead of throwing. Resolves whether it worked. */
async function attempt(
    what: string,
    work: () => Promise<unknown>,
    level: "warn" | "error" = "warn"
): Promise<boolean> {
    try {
        await work();
        return true;
    } catch (error) {
        logger[level](`Could not ${what}: ${describeError(error)}`);
        return false;
    }
}

export const useAppLockStore = create<AppLockState>((set, get) => {
    /** One system prompt at a time, as a second would cancel the first: `busy` is the answer while one is open. */
    const exclusive = async <T>(busy: T, work: () => Promise<T>) => {
        if (get().authenticating) return busy;
        set({ authenticating: true });
        try {
            return await work();
        } finally {
            set({ authenticating: false });
        }
    };
    const prompt = async () => (await get().authenticate()) === "unlocked";

    return {
        enabled: false,
        locked: false,
        authenticating: false,
        beforeUnlock: undefined,
        backgroundedAt: undefined,

        hydrate: async () => {
            if (Platform.OS === "web") return;
            revision += 1;
            const started = revision;
            await attempt("read the app lock setting", async () => {
                const stored = await SecureStore.getItemAsync(ENABLED_KEY);
                // reset, or turned on or off, while this waited: this answer is out of date
                if (started === revision) set({ enabled: stored === "true" });
            });
        },

        lockOnLaunch: (signedIn) => {
            const locked = signedIn && get().enabled;
            if (locked) vault.lock();
            set({ locked });
        },

        appBackgrounded: (now) => {
            set({ backgroundedAt: now });
        },

        appActivated: (signedIn, now, idleMs) => {
            const { backgroundedAt, enabled } = get();
            if (backgroundedAt === undefined) return;
            set({ backgroundedAt: undefined });
            if (!signedIn || !enabled) return;
            // Real elapsed time: the old app took minutes modulo 60, so 61 minutes away did not ask.
            // A clock moved backwards also locks, or setting the date back would skip the prompt.
            const away = now - backgroundedAt;
            if (away > idleMs || away < 0) get().lock();
        },

        authenticate: () => exclusive("cancelled", () => vault.unlock()),

        // Still authenticating while `beforeUnlock` runs: it may wait on the network, and a second prompt meanwhile
        // would run it twice. It stays set until done, so the lock isn't dropped for want of a login meanwhile.
        unlock: () =>
            exclusive(false, async () => {
                if ((await vault.unlock()) !== "unlocked") return false;
                const { beforeUnlock } = get();
                // a lock that went on while the app was open: the login's tokens come back
                const opening = beforeUnlock ?? sessionLock?.unseal;
                if (opening && (await opening()) === false) return false;
                // signing out meanwhile replaces it
                set({
                    locked: false,
                    beforeUnlock:
                        get().beforeUnlock === beforeUnlock
                            ? undefined
                            : get().beforeUnlock,
                });
                return true;
            }),

        enable: async () => {
            revision += 1;
            if (!(await prompt())) return false;
            const saved = await attempt("save the app lock setting", () =>
                SecureStore.setItemAsync(ENABLED_KEY, "true", {
                    keychainAccessible:
                        SecureStore.WHEN_UNLOCKED_THIS_DEVICE_ONLY,
                })
            );
            if (!saved) return false;
            if (
                !(await attempt("seal the login", async () =>
                    sessionLock?.protect(true)
                ))
            ) {
                // on without it would promise the login a protection it doesn't have
                await attempt("remove the app lock setting", () =>
                    SecureStore.deleteItemAsync(ENABLED_KEY)
                );
                return false;
            }
            set({ enabled: true });
            return true;
        },

        // Asks first: otherwise anyone holding an unlocked phone could switch the lock off.
        disable: async () => {
            revision += 1;
            if (!(await prompt())) return false;
            // before the key goes, or nothing could read the login after
            if (
                !(await attempt("unseal the login", async () =>
                    sessionLock?.protect(false)
                ))
            )
                return false;
            if (
                !(await attempt("save the app lock setting", () =>
                    SecureStore.deleteItemAsync(ENABLED_KEY)
                ))
            ) {
                // still on: the login goes back under the key, or the setting would promise a protection it doesn't have
                await attempt(
                    "seal the login again",
                    async () => sessionLock?.protect(true),
                    "error"
                );
                return false;
            }
            // off from here: the setting is gone and the login is unsealed, so what is left is only cleared away
            set({ enabled: false });
            await attempt("remove the saved records", () =>
                recordCache.remove()
            );
            // without the key, records that could not be removed can't be read either
            await attempt("remove the vault key", () => vault.destroy());
            return true;
        },

        lock: () => {
            vault.lock();
            set({ locked: true });
            void sessionLock?.seal();
        },

        clearLock: () => {
            set({ locked: false });
        },

        reset: async () => {
            revision += 1;
            set({
                enabled: false,
                locked: false,
                backgroundedAt: undefined,
                beforeUnlock: undefined,
            });
            await recordCache.remove();
            await vault.destroy();
            // signed out by now: only the next login is stored unsealed
            await sessionLock?.protect(false).catch(() => undefined);
            await attempt("remove the app lock setting", () =>
                SecureStore.deleteItemAsync(ENABLED_KEY)
            );
        },
    };
});
