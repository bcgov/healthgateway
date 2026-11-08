import { beforeEach, describe, expect, it, jest } from "@jest/globals";
import * as SecureStore from "expo-secure-store";

import { recordCache } from "@/services/recordCache";
import { vault, type VaultUnlock } from "@/services/vault";
import { useAppLockStore } from "@/stores/appLock.store";

jest.mock("@/services/vault", () => ({
    vault: {
        unlock: jest.fn(),
        lock: jest.fn(),
        destroy: jest.fn(async () => undefined),
    },
}));
jest.mock("expo-secure-store", () => ({
    getItemAsync: jest.fn(),
    setItemAsync: jest.fn(),
    deleteItemAsync: jest.fn(),
}));

const secure = jest.mocked(SecureStore);
const authenticate = jest.mocked(vault.unlock);
const store = () => useAppLockStore.getState();
const KEY = "hg.appLock.enabled";
const APP_LOCK_TIMEOUT_MS = 20 * 60 * 1000;

describe("app lock store", () => {
    beforeEach(() => {
        jest.resetAllMocks();
        secure.getItemAsync.mockResolvedValue(null);
        secure.setItemAsync.mockResolvedValue();
        secure.deleteItemAsync.mockResolvedValue();
        authenticate.mockResolvedValue("unlocked");
        useAppLockStore.setState({
            enabled: false,
            locked: false,
            authenticating: false,
            backgroundedAt: undefined,
        });
    });

    describe("hydrate", () => {
        it("restores the saved opt-in", async () => {
            secure.getItemAsync.mockResolvedValue("true");
            await store().hydrate();
            expect(store().enabled).toBe(true);
        });

        it("stays off when nothing is saved or the store cannot be read", async () => {
            await store().hydrate();
            expect(store().enabled).toBe(false);
            secure.getItemAsync.mockRejectedValue(new Error("locked"));
            await store().hydrate();
            expect(store().enabled).toBe(false);
        });
    });

    describe("lockOnLaunch", () => {
        it("locks a saved session when opted in", () => {
            useAppLockStore.setState({ enabled: true });
            store().lockOnLaunch(true);
            expect(store().locked).toBe(true);
        });

        it("does not lock without a session: reading the saved copy is what asks", () => {
            useAppLockStore.setState({ enabled: true });
            store().lockOnLaunch(false);
            expect(store().locked).toBe(false);
            expect(authenticate).not.toHaveBeenCalled();
        });

        it("does not lock when not opted in", () => {
            store().lockOnLaunch(true);
            expect(store().locked).toBe(false);
        });
    });

    describe("returning from the background", () => {
        const away = (ms: number, signedIn = true) => {
            store().appBackgrounded(1_000_000);
            store().appActivated(signedIn, 1_000_000 + ms, APP_LOCK_TIMEOUT_MS);
        };

        beforeEach(() => useAppLockStore.setState({ enabled: true }));

        it("does not ask at exactly the timeout", () => {
            away(APP_LOCK_TIMEOUT_MS);
            expect(store().locked).toBe(false);
        });

        it("asks just past the timeout", () => {
            away(APP_LOCK_TIMEOUT_MS + 1);
            expect(store().locked).toBe(true);
        });

        it("asks after 61 minutes, which the old app's hourly wrap let through", () => {
            away(61 * 60 * 1000);
            expect(store().locked).toBe(true);
        });

        it("asks when the clock moved backwards", () => {
            away(-1);
            expect(store().locked).toBe(true);
        });

        it("does not ask when signed out or not opted in", () => {
            away(APP_LOCK_TIMEOUT_MS + 1, false);
            expect(store().locked).toBe(false);
            useAppLockStore.setState({ enabled: false });
            away(APP_LOCK_TIMEOUT_MS + 1);
            expect(store().locked).toBe(false);
        });

        it("ignores an activation with no background before it (a system prompt closing)", () => {
            store().appActivated(true, 1_000_000, APP_LOCK_TIMEOUT_MS);
            expect(store().locked).toBe(false);
        });
    });

    it("lock covers the app, for the idle timer", () => {
        store().lock();
        expect(store().locked).toBe(true);
    });

    describe("unlock", () => {
        beforeEach(() => useAppLockStore.setState({ locked: true }));

        it("lifts the lock when the prompt passes", async () => {
            await expect(store().unlock()).resolves.toBe(true);
            expect(store().locked).toBe(false);
            expect(store().authenticating).toBe(false);
        });

        it("stays locked when the prompt fails", async () => {
            authenticate.mockResolvedValue("cancelled");
            await expect(store().unlock()).resolves.toBe(false);
            expect(store().locked).toBe(true);
        });

        it("does not open a second prompt over one already open", async () => {
            let finish: (result: VaultUnlock) => void = () => undefined;
            authenticate.mockReturnValue(
                new Promise((resolve) => (finish = resolve))
            );
            const first = store().unlock();
            await expect(store().unlock()).resolves.toBe(false);
            expect(authenticate).toHaveBeenCalledTimes(1);
            finish("unlocked");
            await expect(first).resolves.toBe(true);
        });
    });

    it("opens the saved copy after the prompt, before the lock lifts", async () => {
        const beforeUnlock = jest.fn(async () => {
            expect(store().locked).toBe(true);
        });
        useAppLockStore.setState({ locked: true, beforeUnlock });
        await store().unlock();
        expect(beforeUnlock).toHaveBeenCalledTimes(1);
        expect(store().locked).toBe(false);
        expect(store().beforeUnlock).toBeUndefined();
    });

    it("forgets the key from memory when locking", () => {
        useAppLockStore.setState({ enabled: true });
        store().lock();
        expect(vault.lock).toHaveBeenCalled();
        expect(store().locked).toBe(true);
    });

    it("turning off deletes the saved records and the key", async () => {
        const remove = jest.spyOn(recordCache, "remove");
        useAppLockStore.setState({ enabled: true });
        await store().disable();
        expect(remove).toHaveBeenCalled();
        expect(vault.destroy).toHaveBeenCalled();
    });

    describe("enable", () => {
        it("saves the opt-in once the prompt passes", async () => {
            await expect(store().enable()).resolves.toBe(true);
            expect(
                jest.mocked(secure.setItemAsync).mock.calls[0].slice(0, 2)
            ).toEqual([KEY, "true"]);
            expect(store().enabled).toBe(true);
        });

        it("stays off when the prompt fails", async () => {
            authenticate.mockResolvedValue("cancelled");
            await expect(store().enable()).resolves.toBe(false);
            expect(secure.setItemAsync).not.toHaveBeenCalled();
            expect(store().enabled).toBe(false);
        });

        it("stays off when it cannot be saved", async () => {
            secure.setItemAsync.mockRejectedValue(new Error("full"));
            await expect(store().enable()).resolves.toBe(false);
            expect(store().enabled).toBe(false);
        });
    });

    describe("disable", () => {
        beforeEach(() => useAppLockStore.setState({ enabled: true }));

        it("needs the prompt to pass", async () => {
            authenticate.mockResolvedValue("cancelled");
            await expect(store().disable()).resolves.toBe(false);
            expect(secure.deleteItemAsync).not.toHaveBeenCalled();
            expect(store().enabled).toBe(true);
        });

        it("removes the opt-in", async () => {
            await expect(store().disable()).resolves.toBe(true);
            expect(secure.deleteItemAsync).toHaveBeenCalledWith(KEY);
            expect(store().enabled).toBe(false);
        });

        it("stays on when it cannot be saved", async () => {
            secure.deleteItemAsync.mockRejectedValue(new Error("locked"));
            await expect(store().disable()).resolves.toBe(false);
            expect(store().enabled).toBe(true);
        });
    });
});
