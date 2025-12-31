import {
    NavigationGuard,
    NavigationGuardNext,
    RouteLocationNormalizedLoaded,
} from "vue-router";

import { Path } from "@/constants/path";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { FeatureToggleConfiguration } from "@/models/configData";
import { UserState } from "@/router/index";
import { ILogger } from "@/services/interfaces";
import { useAuthStore } from "@/stores/auth";
import { useConfigStore } from "@/stores/config";
import { useUserStore } from "@/stores/user";

function getDefaultPath(
    currentUserState: UserState,
    requiredFeaturesEnabled: boolean
): string {
    switch (currentUserState) {
        case UserState.offline:
            return Path.Root;
        case UserState.pendingDeletion:
            return Path.Profile;
        case UserState.registered:
            return requiredFeaturesEnabled ? Path.Home : Path.Unauthorized;
        case UserState.notRegistered:
            return Path.Registration;
        case UserState.invalidIdentityProvider:
            return Path.IdirLoggedIn;
        case UserState.noPatient:
            return Path.PatientRetrievalError;
        case UserState.unauthenticated:
            return requiredFeaturesEnabled ? Path.Login : Path.Unauthorized;
        case UserState.acceptTermsOfService:
            return Path.AcceptTermsOfService;
        default:
            return Path.Unauthorized;
    }
}

function calculateUserState(): UserState {
    const configStore = useConfigStore();
    const authStore = useAuthStore();
    const userStore = useUserStore();

    const isOffline = configStore.isOffline;
    const isAuthenticated: boolean = authStore.oidcIsAuthenticated;
    const isValidIdentityProvider: boolean = userStore.isValidIdentityProvider;
    const patientRetrievalFailed: boolean = userStore.patientRetrievalFailed;
    const isRegistered: boolean = userStore.userIsRegistered;
    const userIsActive: boolean = userStore.userIsActive;
    const hasTermsOfServiceUpdated: boolean =
        userStore.hasTermsOfServiceUpdated;

    if (isOffline) {
        return UserState.offline;
    } else if (!isAuthenticated) {
        return UserState.unauthenticated;
    } else if (!isValidIdentityProvider) {
        return UserState.invalidIdentityProvider;
    } else if (patientRetrievalFailed) {
        return UserState.noPatient;
    } else if (!isRegistered) {
        return UserState.notRegistered;
    } else if (!userIsActive) {
        return UserState.pendingDeletion;
    } else if (hasTermsOfServiceUpdated) {
        return UserState.acceptTermsOfService;
    } else {
        return UserState.registered;
    }
}

export const beforeEachGuard: NavigationGuard = async (
    to: RouteLocationNormalizedLoaded,
    from: RouteLocationNormalizedLoaded,
    next: NavigationGuardNext
) => {
    const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
    const configStore = useConfigStore();
    const authStore = useAuthStore();

    const webClientConfig = configStore.config.webClient;

    logger.debug(
        `from.fullPath: ${JSON.stringify(
            from.fullPath
        )}; to.fullPath: ${JSON.stringify(to.fullPath)}`
    );

    const meta: {
        routeIsOidcCallback?: boolean;
        stateless?: boolean;
        validStates?: UserState[];
        requiredFeaturesEnabled?: (
            featureToggleConfig: FeatureToggleConfiguration
        ) => boolean;
    } = to.meta;
    if (meta === undefined) {
        next(Error("Route meta property is undefined"));
        return;
    }

    if (meta.routeIsOidcCallback || meta.stateless) {
        next();
        return;
    }

    await authStore.checkStatus();

    // Make sure that the route accepts the current state
    const currentUserState = calculateUserState();
    logger.debug(`current state: ${currentUserState}`);

    const isValidState = (meta.validStates ?? []).includes(currentUserState);
    const requiredFeaturesEnabled =
        meta.requiredFeaturesEnabled === undefined ||
        (webClientConfig !== undefined &&
            meta.requiredFeaturesEnabled(
                webClientConfig.featureToggleConfiguration
            ));

    if (isValidState && requiredFeaturesEnabled) {
        next();
        return;
    }

    // If the route does not accept the state, go to one of the default locations
    const defaultPath = getDefaultPath(
        currentUserState,
        requiredFeaturesEnabled
    );

    if (defaultPath === Path.Login) {
        next({ path: defaultPath, query: { redirect: to.path } });
        return;
    }

    next({ path: defaultPath });
};
