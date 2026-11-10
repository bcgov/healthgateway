import type { EnvironmentId } from "@/config/environments";
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
