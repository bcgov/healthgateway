import { Path } from "@/constants/path";
import { describeError, logger } from "@/services/logger";
import { sessionGeneration } from "@/services/sessionFence";
import { useAppLockStore } from "@/stores/appLock.store";
import { useConnectivityStore } from "@/stores/connectivity.store";
import { useNotificationStore } from "@/stores/notification.store";
import { markTimelineDatasetsStale } from "@/stores/refreshDatasets";
import {
    selectIsValidIdentityProvider,
    selectUserIsRegistered,
    useUserStore,
} from "@/stores/user.store";

/**
 * Shared post-authentication steps (LoginCallbackView.vue on web, inline after promptAsync on native):
 * load patient + profile, then return the path to navigate to.
 */
export async function completeLogin(redirectPath: string): Promise<string> {
    const session = sessionGeneration();
    const userStore = useUserStore.getState();
    if (!selectIsValidIdentityProvider(userStore)) {
        // the route guard sends invalid identity providers to /idirLoggedIn
        return redirectPath;
    }
    await userStore.retrieveEssentialData(true);
    // signed out while loading: nothing of this login is left to route by
    if (session !== sessionGeneration()) return Path.Root;
    // records still on screen from an expired session. Device security: the saved copy's own age decides (firstRecordSync)
    if (!useAppLockStore.getState().enabled) {
        markTimelineDatasetsStale(useUserStore.getState().user.hdid);
        useConnectivityStore.getState().requestSync();
    }
    useNotificationStore
        .getState()
        .retrieve()
        .catch((error: unknown) =>
            logger.warn(
                `Notification retrieval failed: ${describeError(error)}`
            )
        );
    return selectUserIsRegistered(useUserStore.getState())
        ? redirectPath
        : Path.Registration;
}
