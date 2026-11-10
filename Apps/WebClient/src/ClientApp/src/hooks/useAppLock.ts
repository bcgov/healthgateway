import { useEffect, useRef, useState } from "react";
import { AppState, Platform } from "react-native";

import { useAppLockStore } from "@/stores/appLock.store";
import { refreshTokenIfDue, useAuthStore } from "@/stores/auth.store";
import { selectIdleTimeout, useConfigStore } from "@/stores/config.store";
import {
    selectIsUnreachable,
    useConnectivityStore,
} from "@/stores/connectivity.store";
import { startFirstRecordSync } from "@/stores/firstRecordSync";
import {
    probeService,
    recoverSession,
    startRecordCacheSync,
} from "@/stores/localSession";
import { getLocalAuthMethod, type LocalAuthMethod } from "@/utils/localAuth";
import { recordLastActive } from "@/utils/sessionExpiry";

/**
 * Long enough for the route guard to leave the signed-in page first: lifting the lock shows that page's open dialog
 * again, and a native Modal unmounted while navigating leaves a stuck invisible window (AGENTS.md).
 */
const CLEAR_AFTER_SIGN_OUT_MS = 300;

const startNothing = () => undefined;

/** How often coming back to the app re-reads the configuration. */
const CONFIG_CHECK_INTERVAL_MS = 60 * 60 * 1000;
let lastConfigCheck = 0;

/** How often to check whether Health Gateway is back. */
const SERVICE_PROBE_INTERVAL_MS = 30000;

/** Root layout: asks again after a long time away, and drops the lock once there is no session left to protect. */
export function useAppLockLifecycle() {
    const locked = useAppLockStore((s) => s.locked);
    const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
    // a locked launch that opens the saved copy only signs in after the prompt
    const opening = useAppLockStore((s) => s.beforeUnlock !== undefined);
    const isOnline = useConnectivityStore((s) => s.isOnline);

    useEffect(() => {
        if (Platform.OS === "web") return startNothing();
        return startRecordCacheSync();
    }, []);

    useEffect(() => {
        if (Platform.OS === "web") return startNothing();
        return startFirstRecordSync();
    }, []);

    const unreachable = useConnectivityStore(selectIsUnreachable);
    const wasUnreachable = useRef(unreachable);
    useEffect(() => {
        const back = !unreachable && wasUnreachable.current;
        wasUnreachable.current = unreachable;
        if (back) void recoverSession();
    }, [unreachable]);

    // Nothing else may call the service while it is down, so ask it now and then
    const serviceUnavailable = useConnectivityStore(
        (s) => s.serviceUnavailable
    );
    useEffect(() => {
        if (!serviceUnavailable || !isOnline) return;
        const timer = setInterval(
            () => void probeService(),
            SERVICE_PROBE_INTERVAL_MS
        );
        return () => clearInterval(timer);
    }, [serviceUnavailable, isOnline]);

    useEffect(() => {
        if (Platform.OS === "web") return;
        const subscription = AppState.addEventListener("change", (state) => {
            const lock = useAppLockStore.getState();
            if (state === "background") {
                lock.appBackgrounded(Date.now());
                void recordLastActive(Date.now());
            } else if (state === "active") {
                void refreshTokenIfDue();
                lock.appActivated(
                    useAuthStore.getState().isAuthenticated,
                    Date.now(),
                    selectIdleTimeout(useConfigStore.getState())
                );
                if (useConnectivityStore.getState().serviceUnavailable)
                    void probeService();
                // a running app must hear about a required update too
                if (Date.now() - lastConfigCheck >= CONFIG_CHECK_INTERVAL_MS) {
                    lastConfigCheck = Date.now();
                    void useConfigStore.getState().refresh();
                }
            }
        });
        return () => subscription.remove();
    }, []);

    useEffect(() => {
        if (!locked || isAuthenticated || opening) return;
        const timer = setTimeout(
            () => useAppLockStore.getState().clearLock(),
            CLEAR_AFTER_SIGN_OUT_MS
        );
        return () => clearTimeout(timer);
    }, [locked, isAuthenticated, opening]);
}

/** The device's unlock method, re-read on return from Settings, where the user may just have set one up. */
export function useLocalAuthMethod(): LocalAuthMethod | undefined {
    const [method, setMethod] = useState<LocalAuthMethod>();

    useEffect(() => {
        let active = true;
        const read = () =>
            void getLocalAuthMethod().then((m) => active && setMethod(m));
        read();
        const subscription = AppState.addEventListener("change", (state) => {
            if (state === "active") read();
        });
        return () => {
            active = false;
            subscription.remove();
        };
    }, []);

    return method;
}
