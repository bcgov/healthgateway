import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";
import { AppState, type AppStateStatus } from "react-native";

// native only (a .ts test): web has no network listener

type NetworkState = { isConnected?: boolean; isInternetReachable?: boolean };

const mockGetNetworkState = jest.fn<() => Promise<NetworkState>>();
const mockAddListener = jest.fn();

jest.mock("expo-network", () => ({
    getNetworkStateAsync: () => mockGetNetworkState(),
    addNetworkStateListener: (listener: unknown) => mockAddListener(listener),
}));

/** A fresh store: `start` runs once per app. */
async function startStore() {
    let store!: typeof import("@/stores/connectivity.store");
    jest.isolateModules(() => {
        store = jest.requireActual("@/stores/connectivity.store");
    });
    await store.useConnectivityStore.getState().start();
    return store;
}

describe("connectivity store", () => {
    let appStateListener: ((state: AppStateStatus) => void) | undefined;

    beforeEach(() => {
        jest.useFakeTimers();
        appStateListener = undefined;
        jest.spyOn(AppState, "addEventListener").mockImplementation(
            (_type, listener) => {
                appStateListener = listener;
                return { remove: jest.fn() };
            }
        );
        mockGetNetworkState.mockReset();
        mockGetNetworkState.mockResolvedValue({ isConnected: true });
        mockAddListener.mockReset();
    });

    afterEach(() => {
        jest.useRealTimers();
        jest.restoreAllMocks();
    });

    it("reads the network at start and listens for changes", async () => {
        mockGetNetworkState.mockResolvedValue({
            isConnected: false,
            isInternetReachable: false,
        });
        const { useConnectivityStore } = await startStore();
        expect(useConnectivityStore.getState().isOnline).toBe(false);
        expect(mockAddListener).toHaveBeenCalledTimes(1);
    });

    it("reads the network again on coming to the foreground: Android reports no change after Doze", async () => {
        mockGetNetworkState.mockResolvedValue({
            isConnected: false,
            isInternetReachable: false,
        });
        const { useConnectivityStore } = await startStore();
        expect(useConnectivityStore.getState().isOnline).toBe(false);

        // woken: the network is back, but no listener event came
        mockGetNetworkState.mockResolvedValue({
            isConnected: true,
            isInternetReachable: true,
        });
        appStateListener?.("active");
        await jest.advanceTimersByTimeAsync(0);
        expect(useConnectivityStore.getState().isOnline).toBe(true);
    });

    it("reads it once more a moment later, as Android may still be checking the network", async () => {
        const { useConnectivityStore, WAKE_RECHECK_MS } = await startStore();
        mockGetNetworkState.mockResolvedValue({
            isConnected: true,
            isInternetReachable: false,
        });
        appStateListener?.("active");
        await jest.advanceTimersByTimeAsync(0);
        expect(useConnectivityStore.getState().isOnline).toBe(false);

        mockGetNetworkState.mockResolvedValue({
            isConnected: true,
            isInternetReachable: true,
        });
        await jest.advanceTimersByTimeAsync(WAKE_RECHECK_MS);
        expect(useConnectivityStore.getState().isOnline).toBe(true);
    });

    it("leaves the state alone when going to the background, and when a read fails", async () => {
        mockGetNetworkState.mockResolvedValue({ isConnected: true });
        const { useConnectivityStore, WAKE_RECHECK_MS } = await startStore();
        const reads = mockGetNetworkState.mock.calls.length;
        appStateListener?.("background");
        expect(mockGetNetworkState.mock.calls.length).toBe(reads);

        // the failure is logged, not shown
        const warn = jest
            .spyOn(console, "warn")
            .mockImplementation(() => undefined);
        mockGetNetworkState.mockRejectedValue(new Error("no permission"));
        appStateListener?.("active");
        await jest.advanceTimersByTimeAsync(WAKE_RECHECK_MS);
        expect(useConnectivityStore.getState().isOnline).toBe(true);
        expect(warn).toHaveBeenCalledWith(
            expect.stringContaining("Network state unavailable")
        );
    });
});
