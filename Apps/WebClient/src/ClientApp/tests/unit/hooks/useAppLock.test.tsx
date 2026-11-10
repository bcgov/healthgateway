import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";
import AsyncStorage from "@react-native-async-storage/async-storage";
import { act } from "react";
import { AppState, type AppStateStatus, Platform } from "react-native";

import { useAppLockLifecycle, useLocalAuthMethod } from "@/hooks/useAppLock";
import { useAppLockStore } from "@/stores/appLock.store";
import { useAuthStore } from "@/stores/auth.store";
import { updateStores } from "@/test/fixtures";
import { advanceTimers, renderHook } from "@/test/ui";
import { getLocalAuthMethod } from "@/utils/localAuth";

jest.mock("expo-secure-store", () => ({
    getItemAsync: jest.fn(),
    setItemAsync: jest.fn(),
    deleteItemAsync: jest.fn(),
}));
jest.mock("@/utils/localAuth", () => ({
    getLocalAuthMethod: jest.fn(),
}));

const method = jest.mocked(getLocalAuthMethod);
const DEFAULT_IDLE = 20 * 60 * 1000;
const lockStore = () => useAppLockStore.getState();

describe("useAppLock", () => {
    let listeners: ((state: AppStateStatus) => void)[];
    const emit = (state: AppStateStatus) =>
        act(async () => {
            listeners.forEach((listener) => listener(state));
            await Promise.resolve();
        });

    beforeEach(() => {
        listeners = [];
        method.mockResolvedValue("faceId");
        jest.spyOn(AppState, "addEventListener").mockImplementation(
            (_type, listener) => {
                listeners.push(listener);
                return { remove: jest.fn() };
            }
        );
        useAppLockStore.setState({
            enabled: true,
            locked: false,
            backgroundedAt: undefined,
        });
        useAuthStore.setState({ isAuthenticated: true });
    });

    afterEach(async () => {
        jest.useRealTimers();
        jest.restoreAllMocks();
        updateStores(() => {
            useAppLockStore.setState({ enabled: false, locked: false });
            useAuthStore.setState({ isAuthenticated: false });
        });
    });

    describe("useAppLockLifecycle", () => {
        it("locks on return after more than the timeout (native)", async () => {
            const now = jest.spyOn(Date, "now");
            await renderHook(() => useAppLockLifecycle());
            if (Platform.OS === "web") {
                // the web client has no lock: nothing listens
                expect(listeners).toHaveLength(0);
                return;
            }
            now.mockReturnValue(1_000_000);
            await emit("background");
            now.mockReturnValue(1_000_000 + DEFAULT_IDLE + 1);
            await emit("active");
            expect(lockStore().locked).toBe(true);
        });

        it("records when the app left the foreground, for the cold-start check (native)", async () => {
            jest.spyOn(Date, "now").mockReturnValue(1_000_000);
            await renderHook(() => useAppLockLifecycle());
            await emit("background");
            if (Platform.OS !== "web") {
                expect(
                    await AsyncStorage.getItem("hg.session.lastActive")
                ).toBe("1000000");
            }
        });

        it("ignores the inactive state a system prompt causes", async () => {
            await renderHook(() => useAppLockLifecycle());
            await emit("inactive");
            await emit("active");
            expect(lockStore().locked).toBe(false);
            expect(lockStore().backgroundedAt).toBeUndefined();
        });

        it("drops the lock shortly after the session ends, not before", async () => {
            jest.useFakeTimers();
            useAppLockStore.setState({ locked: true });
            await renderHook(() => useAppLockLifecycle());
            await advanceTimers(1000);
            expect(lockStore().locked).toBe(true);

            updateStores(() =>
                useAuthStore.setState({ isAuthenticated: false })
            );
            await advanceTimers(200);
            expect(lockStore().locked).toBe(true);
            await advanceTimers(100);
            expect(lockStore().locked).toBe(false);
        });
    });

    describe("useLocalAuthMethod", () => {
        it("reads the method, and again on return from Settings", async () => {
            const { result } = await renderHook(() => useLocalAuthMethod());
            expect(result.current).toBe("faceId");

            method.mockResolvedValue("touchId");
            await emit("active");
            expect(result.current).toBe("touchId");
        });
    });
});
