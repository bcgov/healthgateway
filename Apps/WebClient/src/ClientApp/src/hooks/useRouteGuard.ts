import { usePathname, useRouter } from "expo-router";
import { useEffect } from "react";
import { Platform } from "react-native";

import { Path } from "@/constants/path";
import { toHref } from "@/navigation/href";
import { getRouteMeta, routeFeaturesEnabled } from "@/navigation/routeMeta";
import {
    calculateUserState,
    getDefaultPath,
    UserState,
} from "@/navigation/userState";
import { services } from "@/services/container";
import { logger } from "@/services/logger";
import { selectOpensOnUnlock, useAppLockStore } from "@/stores/appLock.store";
import { useAuthStore } from "@/stores/auth.store";
import { selectFeatureToggles, useConfigStore } from "@/stores/config.store";
import {
    selectIsValidIdentityProvider,
    useUserStore,
} from "@/stores/user.store";

/**
 * Port of router/beforeEachGuard.ts. Runs whenever the path or the auth/user state changes,
 * and redirects when the current route does not accept the current UserState.
 */
export function useRouteGuard(ready: boolean) {
    const pathname = usePathname();
    const router = useRouter();
    const toggles = useConfigStore(selectFeatureToggles);
    const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
    const user = useUserStore((s) => s.user);
    const patientRetrievalFailed = useUserStore(
        (s) => s.patientRetrievalFailed
    );
    const isLoadingUser = useUserStore((s) => s.isLoading);
    const essentialDataLoaded = useUserStore((s) => s.essentialDataLoaded);
    const isValidIdp = useUserStore(selectIsValidIdentityProvider);
    // Device security: a launch's login opens only after the lock's prompt. Judged before then, a link that started the
    // app on a signed-in page would look signed out and be sent to the landing page, so the guard waits. The page stays
    // under the lock meanwhile, with no person to load anything for.
    const opening = useAppLockStore((s) => s.locked && selectOpensOnUnlock(s));

    // afterEachHook on the web client: a page view per route change
    useEffect(() => {
        services.tracking.trackPageView(pathname);
    }, [pathname]);

    useEffect(() => {
        if (!ready || isLoadingUser || opening) return;
        // Signed in but patient/profile not loaded yet (login just completed or app just started):
        // wait, otherwise the user briefly looks "not registered" and bounces via /registration.
        if (isAuthenticated && isValidIdp && !essentialDataLoaded) return;
        const { loginRequest } = useAuthStore.getState();
        if (loginRequest && !isAuthenticated) {
            useAuthStore.setState({ loginRequest: undefined });
            router.replace({
                pathname: Path.Login,
                params: {
                    ...(loginRequest.idp ? { idp: loginRequest.idp } : {}),
                    redirect: pathname,
                },
            });
            return;
        }
        const meta = getRouteMeta(pathname);
        if (!meta) {
            router.replace(Path.NotFound);
            return;
        }
        if (meta.stateless) return;

        const state = calculateUserState();

        // Native only: a signed-in, registered user opening the app lands on Home (as the old iOS app did),
        // not on the public marketing page. Web keeps the landing page reachable while signed in, like the site.
        if (
            Platform.OS !== "web" &&
            pathname === Path.Root &&
            state === UserState.registered
        ) {
            router.replace(Path.Home);
            return;
        }

        const requiredFeaturesEnabled = routeFeaturesEnabled(pathname, toggles);
        const isValidState = (meta.validStates ?? []).includes(state);
        logger.debug(`route guard: state=${state} valid=${isValidState}`);
        if (isValidState && requiredFeaturesEnabled) return;

        const target = getDefaultPath(state, requiredFeaturesEnabled);
        if (target.toLowerCase() === pathname.toLowerCase()) return;
        // Native: the landing page is the way in, and /login starts the provider by itself with only one, so a session
        // that ended (Reset App, a refused token) must not land there (stated deviation).
        if (target === Path.Login && Platform.OS !== "web") {
            router.replace(Path.Root);
        } else if (target === Path.Login) {
            router.replace({
                pathname: Path.Login,
                params: { redirect: pathname },
            });
        } else {
            router.replace(toHref(target));
        }
    }, [
        ready,
        opening,
        pathname,
        isAuthenticated,
        user,
        patientRetrievalFailed,
        isLoadingUser,
        essentialDataLoaded,
        isValidIdp,
        toggles,
        router,
    ]);
}
