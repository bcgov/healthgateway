import { Platform } from "react-native";
import { create } from "zustand";

import { describeError, logger } from "@/services/logger";

/**
 * "live": real tokens. "offline": opened from the saved copy with no connection. "expired": connected, but Keycloak
 * refused the refresh token, so the saved copy shows until the user logs in again. The last two need device security.
 */
export type SessionStatus = "live" | "offline" | "expired";

interface ConnectivityState {
    /** the device has a connection */
    isOnline: boolean;
    /** Native: the device has a connection, but a call to Health Gateway could not get an answer at all */
    serviceUnavailable: boolean;
    /** Native: the first background fetch of everything the saved copy holds is running */
    syncing: boolean;
    /** Native: a pull to refresh is running, so the records may be half replaced and must not be saved */
    pullInProgress: boolean;
    /** the short "Syncing…" flash as that starts; it carries on without one, so nobody thinks they are waiting */
    syncPill: boolean;
    sessionStatus: SessionStatus;
    /** when the copy on screen was saved; set while it is what the user sees */
    savedAt?: number;
    /** raises each time the records should sync again (back online, logged in again) */
    syncRequests: number;
    start: () => Promise<void>;
    requestSync: () => void;
    setSessionStatus: (status: SessionStatus, savedAt?: number) => void;
    setServiceUnavailable: (unavailable: boolean) => void;
    setSyncing: (syncing: boolean) => void;
}

let started = false;

/** While the banner explains failures, or nobody asked for what is being fetched, log them quietly. */
const updateLogger = (s: {
    isOnline: boolean;
    serviceUnavailable: boolean;
    syncing: boolean;
    sessionStatus: SessionStatus;
}) =>
    logger.setFailuresExpected(
        !s.isOnline ||
            s.serviceUnavailable ||
            s.syncing ||
            s.sessionStatus !== "live"
    );

/** Dependency free: the HTTP client and the error store both read it. */
export const useConnectivityStore = create<ConnectivityState>((set, get) => ({
    isOnline: true,
    serviceUnavailable: false,
    syncing: false,
    syncPill: false,
    pullInProgress: false,
    sessionStatus: "live",
    savedAt: undefined,
    syncRequests: 0,
    start: async () => {
        if (Platform.OS === "web" || started) return;
        started = true;
        try {
            const network = await import("expo-network");
            const apply = (state: {
                isConnected?: boolean;
                isInternetReachable?: boolean;
            }) => {
                const isOnline =
                    state.isConnected !== false &&
                    state.isInternetReachable !== false;
                if (isOnline === get().isOnline) return;
                set({ isOnline });
                updateLogger(get());
            };
            apply(await network.getNetworkStateAsync());
            network.addNetworkStateListener(apply);
        } catch (error) {
            logger.warn(`Network state unavailable: ${describeError(error)}`);
        }
    },
    requestSync: () => set((s) => ({ syncRequests: s.syncRequests + 1 })),
    setSessionStatus: (sessionStatus, savedAt) => {
        set({ sessionStatus, savedAt });
        updateLogger(get());
    },
    setServiceUnavailable: (serviceUnavailable) => {
        // web shows its own error for each failure, as the web client does
        if (
            Platform.OS === "web" ||
            serviceUnavailable === get().serviceUnavailable
        )
            return;
        set({ serviceUnavailable });
        updateLogger(get());
    },
    setSyncing: (syncing) => {
        set({ syncing });
        updateLogger(get());
    },
}));

/** Whether the API can be reached with a working login. */
export const selectCanFetch = (s: ConnectivityState) =>
    s.isOnline && s.sessionStatus === "live";

/** Offline or Health Gateway unreachable: failures are expected, and the session banner says why. */
export const selectIsUnreachable = (s: ConnectivityState) =>
    !s.isOnline || s.serviceUnavailable;

/** A call to Health Gateway can be expected to work. */
export const selectIsLive = (s: ConnectivityState) =>
    selectCanFetch(s) && !s.serviceUnavailable;
