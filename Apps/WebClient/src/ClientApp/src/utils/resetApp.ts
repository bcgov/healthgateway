import AsyncStorage from "@react-native-async-storage/async-storage";
import { Alert } from "react-native";

import { clearStoredSession } from "@/services/auth/storedSession";
import { services } from "@/services/container";
import { describeError, logger } from "@/services/logger";
import { useAppLockStore } from "@/stores/appLock.store";
import { useAuthStore } from "@/stores/auth.store";
import { useOnboardingStore } from "@/stores/onboarding.store";
import { useThemeStore } from "@/stores/theme.store";

/**
 * Back to a fresh install: signs out (removing the tokens), forgets the app lock, and clears everything saved on the
 * device. Onboarding is reloaded last because it covers the screen; `leavePage` moves off any signed-in page beneath it.
 * Rejects, before anything else is cleared, when the stored login could not be deleted: the next start would restore it.
 */
export async function resetApp(leavePage: () => void): Promise<void> {
    // the app may never have started, on an environment that is down: the stored tokens still go
    if (services.isInitialized) await useAuthStore.getState().signOut();
    await clearStoredSession();
    leavePage();
    await useAppLockStore.getState().reset();
    try {
        await AsyncStorage.clear();
    } catch (error) {
        logger.warn(`Could not clear saved data: ${describeError(error)}`);
    }
    useThemeStore.setState({ preference: "light" });
    await useOnboardingStore.getState().hydrate(false);
}

/**
 * Runs a Reset App or an environment switch (a reset too) from a button: a failure is logged and the user is told, as
 * the app was not reset.
 */
export function runReset(reset: () => Promise<void>): void {
    reset().catch((error: unknown) => {
        logger.warn(`Reset App stopped: ${describeError(error)}`);
        Alert.alert(
            "Reset failed",
            "The app could not be reset. Please try again."
        );
    });
}
