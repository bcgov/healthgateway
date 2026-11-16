import { Platform } from "react-native";

import type { EnvironmentId } from "@/config/environments";
import { logger } from "@/services/logger";
import { resetApp } from "@/utils/resetApp";

import { selectEnvironment, useEnvironmentStore } from "./environment.store";

/**
 * Moves the app to another environment, as a Reset App: logins, saved records, settings and the intro all start fresh,
 * so nothing from one environment carries into the next. The choice is saved after the reset, which clears storage.
 */
export async function switchEnvironment(
    id: EnvironmentId,
    leavePage: () => void
): Promise<void> {
    const environment = useEnvironmentStore.getState();
    if (selectEnvironment(environment).id === id) return;
    await resetApp(leavePage);
    await environment.select(id);
    environment.requestRestart();
}

/**
 * Start-up, native, after the environment store hydrates and before anything else loads: when this launch runs on
 * another environment than the last one (a build was just released and moved from Test to Production, say), the app is
 * reset first, as a switch would, so nothing from one environment carries into the next. Then the environment is saved
 * as the last used. A reset that fails stops start-up (the error page), rather than carry a login across.
 */
export async function settleEnvironment(): Promise<void> {
    if (Platform.OS === "web") return;
    const { effective, lastUsed } = useEnvironmentStore.getState();
    if (effective !== lastUsed) {
        logger.info(
            "The environment differs from the last run's: resetting the app"
        );
        // nothing is on screen yet to leave
        await resetApp(() => undefined);
    }
    await useEnvironmentStore.getState().remember();
}
