import * as SecureStore from "expo-secure-store";
import { Platform } from "react-native";
import { create } from "zustand";

import { describeError, logger } from "@/services/logger";
import { recordCache } from "@/services/recordCache";
import { vault } from "@/services/vault";

const ENABLED_KEY = "hg.appLock.enabled";

interface AppLockState {
    /** The user opted in (device preference, kept across sign-outs). */
    enabled: boolean;
    /** The lock screen covers the app. Never true on web. */
    locked: boolean;
    /** A system prompt is open; a second one would cancel it. */
    authenticating: boolean;
    /** Runs after the prompt passes and before the lock lifts: opening the saved copy on a launch with no login. */
    beforeUnlock?: () => Promise<void>;
    backgroundedAt?: number;
    hydrate: () => Promise<void>;
    /** Cold start: a saved session must be unlocked before the app shows. */
    lockOnLaunch: (signedIn: boolean) => void;
    appBackgrounded: (now: number) => void;
    /** `idleMs`: away this long or less does not ask again. */
    appActivated: (signedIn: boolean, now: number, idleMs: number) => void;
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

export const useAppLockStore = create<AppLockState>((set, get) => {
    const prompt = async () => {
        if (get().authenticating) return false;
        set({ authenticating: true });
        try {
            return (await vault.unlock()) === "unlocked";
        } finally {
            set({ authenticating: false });
        }
    };

    return {
        enabled: false,
        locked: false,
        authenticating: false,
        beforeUnlock: undefined,
        backgroundedAt: undefined,

        hydrate: async () => {
            if (Platform.OS === "web") return;
            try {
                revision += 1;
                const started = revision;
                const stored = await SecureStore.getItemAsync(ENABLED_KEY);
                // reset, or turned on or off, while this waited: this answer is out of date
                if (started === revision) set({ enabled: stored === "true" });
            } catch (error) {
                logger.warn(
                    `Could not read the app lock setting: ${describeError(error)}`
                );
            }
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

        unlock: async () => {
            const passed = await prompt();
            if (!passed) return false;
            const { beforeUnlock } = get();
            if (beforeUnlock) {
                set({ beforeUnlock: undefined });
                await beforeUnlock();
            }
            set({ locked: false });
            return true;
        },

        enable: async () => {
            revision += 1;
            if (!(await prompt())) return false;
            try {
                await SecureStore.setItemAsync(ENABLED_KEY, "true", {
                    keychainAccessible:
                        SecureStore.WHEN_UNLOCKED_THIS_DEVICE_ONLY,
                });
                set({ enabled: true });
                return true;
            } catch (error) {
                logger.warn(
                    `Could not save the app lock setting: ${describeError(error)}`
                );
                return false;
            }
        },

        // Asks first: otherwise anyone holding an unlocked phone could switch the lock off.
        disable: async () => {
            revision += 1;
            if (!(await prompt())) return false;
            try {
                await SecureStore.deleteItemAsync(ENABLED_KEY);
                await recordCache.remove();
                await vault.destroy();
                set({ enabled: false });
                return true;
            } catch (error) {
                logger.warn(
                    `Could not save the app lock setting: ${describeError(error)}`
                );
                return false;
            }
        },

        lock: () => {
            vault.lock();
            set({ locked: true });
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
            try {
                await SecureStore.deleteItemAsync(ENABLED_KEY);
            } catch (error) {
                logger.warn(
                    `Could not remove the app lock setting: ${describeError(error)}`
                );
            }
        },
    };
});
