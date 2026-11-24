import { Platform } from "react-native";

import { Path } from "@/constants/path";
import { createAuthService } from "@/services/auth";
import { classifyAuthFailure } from "@/services/auth/authFailure";
import { keepsSealed } from "@/services/auth/authService";
import { services } from "@/services/container";
import { describeError, logger } from "@/services/logger";
import { expireSessionIfIdle } from "@/utils/sessionExpiry";

import { useAppLockStore } from "./appLock.store";
import { useAuthStore } from "./auth.store";
import {
    nativeOidcConfig,
    selectIdleTimeout,
    selectUpdateRequired,
    useConfigStore,
} from "./config.store";
import { useConnectivityStore } from "./connectivity.store";
import { selectEnvironment, useEnvironmentStore } from "./environment.store";
import {
    openLocalSession,
    prepareLocalSession,
    restoreLocalSession,
} from "./localSession";
import { useNotificationStore } from "./notification.store";
import { useOnboardingStore } from "./onboarding.store";
import { settleEnvironment } from "./switchEnvironment";
import { useThemeStore } from "./theme.store";
import { selectIsValidIdentityProvider, useUserStore } from "./user.store";

/** Port of main.ts initializeApp(): config → services → auth status → essential user data. */
async function boot(pathname: string, stale: () => boolean): Promise<void> {
    await useThemeStore.getState().hydrate();
    if (stale()) return;
    // network state first: offline, the environment store doesn't wait for the released build
    await useConnectivityStore.getState().start();
    if (stale()) return;
    await useEnvironmentStore.getState().hydrate();
    if (stale()) return;
    // a reset when this launch runs on another environment than the last (environmentSelector.md, step 5)
    await settleEnvironment();
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
    const { enabled: lockEnabled } = useAppLockStore.getState();
    const config = await useConfigStore.getState().retrieve(lockEnabled);
    if (stale()) return;
    // native: its own Keycloak client, or a MobileConfigurationError that stops start-up (see nativeOidcConfig)
    const oidcConfig =
        Platform.OS === "web"
            ? config.openIdConnect
            : nativeOidcConfig(config, Platform.OS);
    services.initialize(config, createAuthService(oidcConfig));
    const configState = useConfigStore.getState();
    if (selectUpdateRequired(configState)) return; // nothing else runs until the app is updated
    await services.auth.init({
        keepSessionOnFailure: lockEnabled,
        sealed: lockEnabled,
    });
    if (stale()) return;
    // device security: the login is sealed, and opens after the lock's prompt, with everything it loads
    if (services.auth.isSealed?.()) {
        appLock.lockOnLaunch(true);
        useAppLockStore.setState({
            beforeUnlock: () => openSealedSession(stale),
        });
        return;
    }
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
    await loadSignedInData(stale);
}

/**
 * Passing the lock's prompt at launch with a sealed login: it is unsealed, and refreshed if it ran out. One that can't
 * work right now opens the saved copy instead. False: the key's prompt was turned down or the login could not be read this
 * time, and the lock stays.
 */
async function openSealedSession(stale: () => boolean): Promise<boolean> {
    const result =
        (await services.auth.unseal?.({ restore: true })) ?? "unreadable";
    if (stale()) return true;
    if (keepsSealed(result)) return false;
    // gone for good: the lock lifts onto the login
    if (result === "unreadable") return true;
    if (await openLocalSession()) return true;
    let signedIn: boolean;
    try {
        signedIn = await useAuthStore.getState().checkStatus(stale);
    } catch (error) {
        const details = services.auth.getOidcTokenDetails();
        if (classifyAuthFailure(error) !== "connection" || !details)
            throw error;
        // lost the connection after the refresh: as though it had gone before it
        logger.warn(`Could not load the login: ${describeError(error)}`);
        useConnectivityStore.getState().setServiceUnavailable(true);
        await restoreLocalSession(details.hdid, "offline");
        return true;
    }
    if (stale() || !signedIn) return true;
    logger.verbose("User is signed in");
    await loadSignedInData(stale);
    return true;
}

async function loadSignedInData(stale: () => boolean): Promise<void> {
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
