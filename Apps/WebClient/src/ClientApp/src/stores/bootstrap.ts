import { Platform } from "react-native";

import { Path } from "@/constants/path";
import { createAuthService } from "@/services/auth";
import { services } from "@/services/container";
import { describeError, logger } from "@/services/logger";
import { expireSessionIfIdle } from "@/utils/sessionExpiry";

import { useAppLockStore } from "./appLock.store";
import { useAuthStore } from "./auth.store";
import {
    selectIdleTimeout,
    selectUpdateRequired,
    useConfigStore,
} from "./config.store";
import { useConnectivityStore } from "./connectivity.store";
import { selectEnvironment, useEnvironmentStore } from "./environment.store";
import { prepareLocalSession } from "./localSession";
import { useNotificationStore } from "./notification.store";
import { useOnboardingStore } from "./onboarding.store";
import { useThemeStore } from "./theme.store";
import { selectIsValidIdentityProvider, useUserStore } from "./user.store";

/** Port of main.ts initializeApp(): config → services → auth status → essential user data. */
async function boot(pathname: string, stale: () => boolean): Promise<void> {
    await useThemeStore.getState().hydrate();
    if (stale()) return;
    await useEnvironmentStore.getState().hydrate();
    if (stale()) return;
    if (Platform.OS !== "web")
        services.tracking.init(
            selectEnvironment(useEnvironmentStore.getState()).tracking
        );
    const appLock = useAppLockStore.getState();
    await appLock.hydrate();
    if (stale()) return;
    await useOnboardingStore
        .getState()
        .hydrate(useAppLockStore.getState().enabled);
    if (stale()) return;
    await useConnectivityStore.getState().start();
    if (stale()) return;
    const { enabled: lockEnabled } = useAppLockStore.getState();
    const config = await useConfigStore.getState().retrieve(lockEnabled);
    if (stale()) return;
    services.initialize(config, createAuthService(config.openIdConnect));
    const configState = useConfigStore.getState();
    if (selectUpdateRequired(configState)) return; // nothing else runs until the app is updated
    await services.auth.init({ keepSessionOnFailure: lockEnabled });
    if (stale()) return;
    await expireSessionIfIdle(
        selectIdleTimeout(configState),
        Date.now(),
        stale
    );

    const isCallback =
        Platform.OS === "web" &&
        pathname.toLowerCase() === Path.LoginCallback.toLowerCase();
    if (stale()) return;
    if (isCallback) return; // LoginCallbackView completes the sign-in
    if (prepareLocalSession()) return; // the saved copy opens after the lock's prompt

    const auth = useAuthStore.getState();
    const signedIn = await auth.checkStatus(stale);
    if (stale()) return;
    logger.verbose(signedIn ? "User is signed in" : "User is not signed in");
    appLock.lockOnLaunch(signedIn);
    const userStore = useUserStore.getState();
    if (userStore.user.hdid && selectIsValidIdentityProvider(userStore)) {
        await userStore.retrieveEssentialData();
        if (stale()) return;
        useNotificationStore
            .getState()
            .retrieve()
            .catch((error: unknown) =>
                logger.warn(
                    `Notification retrieval failed: ${describeError(error)}`
                )
            );
    }
}

let latestBoot = 0;

/**
 * Only the newest start-up may change anything: a retry or an environment switch can begin while an older one still waits
 * on the network. Resolves to whether this one is still the newest.
 */
export async function bootstrap(pathname: string): Promise<boolean> {
    latestBoot += 1;
    const run = latestBoot;
    const stale = () => run !== latestBoot;
    try {
        await boot(pathname, stale);
        return !stale();
    } catch (error) {
        if (stale()) return false;
        throw error;
    }
}
