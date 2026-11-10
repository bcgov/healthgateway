import AsyncStorage from "@react-native-async-storage/async-storage";
import { Platform } from "react-native";

import { services } from "@/services/container";
import { describeError, logger } from "@/services/logger";
import { useAppLockStore } from "@/stores/appLock.store";
import { useAuthStore } from "@/stores/auth.store";
import { MAX_IDLE_DIALOG_COUNTDOWN } from "@/utils/idleDetector";

const LAST_ACTIVE_KEY = "hg.session.lastActive";

/** Native: called as the app goes to the background, the last moment it runs before it may be force-quit. */
export async function recordLastActive(now: number): Promise<void> {
    try {
        await AsyncStorage.setItem(LAST_ACTIVE_KEY, String(now));
    } catch (error) {
        logger.warn(
            `Could not save the last active time: ${describeError(error)}`
        );
    }
}

/**
 * Cold start: the idle timer does not run while the app is closed, so a saved session is checked here against the same
 * limit (idle time plus the dialog's countdown). Past it, signing out removes the access and refresh tokens, unless the
 * user has device security on: then the tokens stay, and the lock screen stands in for a full login. An unknown last
 * active time counts as past it. Call after the auth service has loaded the session.
 */
export async function expireSessionIfIdle(
    timeBeforeIdle: number,
    now: number,
    isStale: () => boolean = () => false
): Promise<void> {
    if (Platform.OS === "web" || timeBeforeIdle <= 0) return;
    if (useAppLockStore.getState().enabled) return;
    if (!services.auth.getOidcTokenDetails()) return;
    let stored: string | null = null;
    try {
        stored = await AsyncStorage.getItem(LAST_ACTIVE_KEY);
    } catch (error) {
        logger.warn(
            `Could not read the last active time: ${describeError(error)}`
        );
    }
    // replaced by a newer start-up while reading: this login may no longer be the one to judge
    if (isStale()) return;
    const lastActive = Number(stored);
    const away = now - lastActive;
    const limit = timeBeforeIdle + MAX_IDLE_DIALOG_COUNTDOWN;
    if (stored !== null && lastActive > 0 && away >= 0 && away <= limit) return;
    logger.verbose("Session idle past the limit: signing out");
    await useAuthStore.getState().signOut();
}
