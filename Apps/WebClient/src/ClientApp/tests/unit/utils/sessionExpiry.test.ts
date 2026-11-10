import { beforeEach, describe, expect, it, jest } from "@jest/globals";
import AsyncStorage from "@react-native-async-storage/async-storage";

import { services } from "@/services/container";
import { useAppLockStore } from "@/stores/appLock.store";
import { useAuthStore } from "@/stores/auth.store";
import { expireSessionIfIdle, recordLastActive } from "@/utils/sessionExpiry";

jest.mock("expo-secure-store", () => ({
    getItemAsync: jest.fn(),
    setItemAsync: jest.fn(),
    deleteItemAsync: jest.fn(),
}));

const IDLE = 300000;
const COUNTDOWN = 60000;
const NOW = 10_000_000;
const KEY = "hg.session.lastActive";

describe("sessionExpiry", () => {
    const signOut = jest.fn(async () => undefined);

    beforeEach(async () => {
        await AsyncStorage.clear();
        signOut.mockClear();
        useAuthStore.setState({ signOut });
        useAppLockStore.setState({ enabled: false });
        jest.spyOn(services, "auth", "get").mockReturnValue({
            getOidcTokenDetails: () => ({ idToken: "id" }),
        } as never);
    });

    const expireAfter = async (away: number) => {
        await recordLastActive(NOW - away);
        await expireSessionIfIdle(IDLE, NOW);
    };

    it("records the time", async () => {
        await recordLastActive(NOW);
        expect(await AsyncStorage.getItem(KEY)).toBe(String(NOW));
    });

    it("keeps a session back within the limit", async () => {
        await expireAfter(IDLE + COUNTDOWN);
        expect(signOut).not.toHaveBeenCalled();
    });

    it("removes the tokens past the limit", async () => {
        await expireAfter(IDLE + COUNTDOWN + 1);
        expect(signOut).toHaveBeenCalledTimes(1);
    });

    it("keeps the tokens past the limit with device security on", async () => {
        useAppLockStore.setState({ enabled: true });
        await expireAfter(24 * 60 * 60 * 1000);
        expect(signOut).not.toHaveBeenCalled();
    });

    it("removes the tokens when the last active time is unknown", async () => {
        await expireSessionIfIdle(IDLE, NOW);
        expect(signOut).toHaveBeenCalledTimes(1);
    });

    it("removes the tokens when the stored time is unusable", async () => {
        await AsyncStorage.setItem(KEY, "not a time");
        await expireSessionIfIdle(IDLE, NOW);
        expect(signOut).toHaveBeenCalledTimes(1);
    });

    it("removes the tokens when the clock moved backwards", async () => {
        await expireAfter(-1);
        expect(signOut).toHaveBeenCalledTimes(1);
    });

    it("does nothing without a session", async () => {
        jest.spyOn(services, "auth", "get").mockReturnValue({
            getOidcTokenDetails: () => null,
        } as never);
        await expireSessionIfIdle(IDLE, NOW);
        expect(signOut).not.toHaveBeenCalled();
    });

    it("does nothing when the configuration has no idle time", async () => {
        await expireSessionIfIdle(0, NOW);
        expect(signOut).not.toHaveBeenCalled();
    });
});
