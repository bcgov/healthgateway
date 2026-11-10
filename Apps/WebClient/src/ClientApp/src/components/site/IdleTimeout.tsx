import { useRouter } from "expo-router";
import { useEffect, useRef, useState } from "react";

import { Path } from "@/constants/path";
import { useAppLockStore } from "@/stores/appLock.store";
import { useAuthStore } from "@/stores/auth.store";
import { selectIdleTimeout, useConfigStore } from "@/stores/config.store";
import { useLayoutStore } from "@/stores/layout.store";
import { idleCountdown, IdleDetector } from "@/utils/idleDetector";

import { idleActivitySource } from "./idleActivity";
import { IdleDialog } from "./IdleDialog";

/**
 * Port of IdleComponent.vue: while signed in, no activity for `timeouts.idle` ms opens the countdown dialog;
 * the countdown is 60 s minus any time already idle (a suspended app may be past it → straight to logout).
 */
export function IdleTimeout() {
    const router = useRouter();
    const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
    const locked = useAppLockStore((s) => s.locked);
    const timeBeforeIdle = useConfigStore(selectIdleTimeout);
    const setIsIdle = useLayoutStore((s) => s.setIsIdle);
    const [countdown, setCountdown] = useState<number | undefined>(undefined);
    const detector = useRef<IdleDetector | undefined>(undefined);

    const logout = () => {
        setCountdown(undefined);
        setIsIdle(false);
        router.push(Path.Logout);
    };

    useEffect(() => {
        if (timeBeforeIdle <= 0) return;
        const d = new IdleDetector(
            (timeIdle) => {
                if (!useAuthStore.getState().isAuthenticated) return;
                // Device security on: keep the tokens and lock, so coming back only takes the prompt, not a full login
                const lock = useAppLockStore.getState();
                if (lock.enabled) {
                    lock.lock();
                    return;
                }
                const remaining = idleCountdown(timeIdle);
                if (remaining <= 0) {
                    logout();
                } else {
                    setIsIdle(true);
                    setCountdown(remaining);
                }
            },
            timeBeforeIdle,
            idleActivitySource
        );
        detector.current = d;
        return () => {
            d.disable();
            detector.current = undefined;
        };
        // the detector only depends on the configured idle time
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [timeBeforeIdle]);

    // enable while authenticated, disable when not (also on sign-out while the dialog is up); paused behind the lock
    useEffect(() => {
        if (isAuthenticated && !locked) detector.current?.enable();
        else detector.current?.disable();
    }, [isAuthenticated, locked, timeBeforeIdle]);

    return (
        <IdleDialog
            countdown={countdown}
            onStillHere={() => {
                setCountdown(undefined);
                setIsIdle(false);
                detector.current?.enable();
            }}
            onExpired={logout}
        />
    );
}
