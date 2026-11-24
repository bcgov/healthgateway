import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";
import * as SecureStore from "expo-secure-store";

import { recordCache } from "@/services/recordCache";
import { vault, type VaultUnlock } from "@/services/vault";
import { setSessionLock, useAppLockStore } from "@/stores/appLock.store";

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

    describe("the login's tokens, sealed like the records", () => {
        const hooks = {
            seal: jest.fn(async () => undefined),
            unseal: jest.fn(async (): Promise<boolean> => true),
            protect: jest.fn(async (_on: boolean) => undefined),
        };
        let remove: () => void = () => undefined;
        beforeEach(() => {
            hooks.seal.mockReset().mockResolvedValue(undefined);
            hooks.unseal.mockReset().mockResolvedValue(true);
            hooks.protect.mockReset().mockResolvedValue(undefined);
            remove = setSessionLock(hooks);
        });
        afterEach(() => remove());

        it("locking takes them out of memory", () => {
            store().lock();
            expect(hooks.seal).toHaveBeenCalledTimes(1);
        });

        it("unlocking brings them back before the lock lifts", async () => {
            hooks.unseal.mockImplementation(async () => {
                expect(store().locked).toBe(true);
                return true;
            });
            useAppLockStore.setState({ locked: true });
            await expect(store().unlock()).resolves.toBe(true);
            expect(hooks.unseal).toHaveBeenCalledTimes(1);
            expect(store().locked).toBe(false);
        });

        it("stays locked when they can't be brought back", async () => {
            hooks.unseal.mockResolvedValue(false);
            useAppLockStore.setState({ locked: true });
            await expect(store().unlock()).resolves.toBe(false);
            expect(store().locked).toBe(true);
            expect(store().authenticating).toBe(false);
        });

        it("a launch's opening runs instead, and stays for the next unlock when it can't finish", async () => {
            const beforeUnlock = jest.fn(async () => false);
            useAppLockStore.setState({ locked: true, beforeUnlock });
            await expect(store().unlock()).resolves.toBe(false);
            expect(hooks.unseal).not.toHaveBeenCalled();
            expect(store().locked).toBe(true);
            expect(store().beforeUnlock).toBe(beforeUnlock);
            useAppLockStore.setState({ beforeUnlock: undefined });
        });

        it("counts as authenticating while the opening runs, so a second prompt can't start it again", async () => {
            let finish: () => void = () => undefined;
            const beforeUnlock = jest.fn(
                () => new Promise<void>((resolve) => (finish = resolve))
            );
            useAppLockStore.setState({ locked: true, beforeUnlock });
            const first = store().unlock();
            await new Promise((resolve) => setImmediate(resolve));
            expect(store().authenticating).toBe(true);
            // still set: the lock isn't dropped for want of a login meanwhile
            expect(store().beforeUnlock).toBe(beforeUnlock);
            await expect(store().unlock()).resolves.toBe(false);
            finish();
            await expect(first).resolves.toBe(true);
            expect(beforeUnlock).toHaveBeenCalledTimes(1);
            expect(store().beforeUnlock).toBeUndefined();
        });

        it("turning on seals the stored login", async () => {
            await expect(store().enable()).resolves.toBe(true);
            expect(hooks.protect).toHaveBeenCalledWith(true);
        });

        it("stays off, and forgets the opt-in, when the login can't be sealed", async () => {
            hooks.protect.mockRejectedValue(new Error("no vault"));
            await expect(store().enable()).resolves.toBe(false);
            expect(store().enabled).toBe(false);
            expect(secure.deleteItemAsync).toHaveBeenCalledWith(KEY);
        });

        it("turning off unseals the stored login before the key goes", async () => {
            useAppLockStore.setState({ enabled: true });
            jest.mocked(vault.destroy).mockImplementation(async () => {
                expect(hooks.protect).toHaveBeenCalledWith(false);
            });
            await expect(store().disable()).resolves.toBe(true);
            expect(vault.destroy).toHaveBeenCalled();
        });

        it("seals the login again when the setting could not be turned off", async () => {
            useAppLockStore.setState({ enabled: true });
            secure.deleteItemAsync.mockRejectedValueOnce(new Error("locked"));
            await expect(store().disable()).resolves.toBe(false);
            // still on: the login must not be left unsealed behind a switch that says it is protected
            expect(hooks.protect.mock.calls).toEqual([[false], [true]]);
            expect(store().enabled).toBe(true);
            expect(vault.destroy).not.toHaveBeenCalled();
        });

        it("is off once the setting is gone, even if the saved records could not be removed", async () => {
            useAppLockStore.setState({ enabled: true });
            jest.mocked(recordCache.remove).mockRejectedValueOnce(
                new Error("disk")
            );
            await expect(store().disable()).resolves.toBe(true);
            expect(store().enabled).toBe(false);
            expect(hooks.protect.mock.calls).toEqual([[false]]);
            // without the key, what could not be removed can't be read
            expect(vault.destroy).toHaveBeenCalled();
        });

        it("stays on, key and all, when the login can't be unsealed", async () => {
            useAppLockStore.setState({ enabled: true });
            hooks.protect.mockRejectedValue(new Error("locked"));
            await expect(store().disable()).resolves.toBe(false);
            expect(store().enabled).toBe(true);
            expect(vault.destroy).not.toHaveBeenCalled();
        });
    });
});
