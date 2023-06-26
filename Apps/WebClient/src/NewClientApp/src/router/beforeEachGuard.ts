import { RouterPath, UserState } from "@/router/index";
import { container } from "@/ioc/container";
import { ILogger } from "@/services/interfaces";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { useConfigStore } from "@/stores/config";
import {
    NavigationGuard,
    NavigationGuardNext,
    RouteLocationNormalizedLoaded,
} from "vue-router";
import { useAuthStore } from "@/stores/auth";
import { useUserStore } from "@/stores/user";

function getDefaultPath(
    currentUserState: UserState,
    requiredFeaturesEnabled: boolean
): string {
    switch (currentUserState) {
        case UserState.offline:
            return RouterPath.ROOT_PATH;
        case UserState.pendingDeletion:
            return RouterPath.PROFILE_PATH;
        case UserState.registered:
            return requiredFeaturesEnabled
                ? RouterPath.HOME_PATH
                : RouterPath.UNAUTHORIZED_PATH;
        case UserState.notRegistered:
            return RouterPath.REGISTRATION_PATH;
        case UserState.invalidIdentityProvider:
            return RouterPath.IDIR_LOGGED_IN_PATH;
        case UserState.noPatient:
            return RouterPath.PATIENT_RETRIEVAL_ERROR_PATH;
        case UserState.unauthenticated:
            return requiredFeaturesEnabled
                ? RouterPath.LOGIN_PATH
                : RouterPath.UNAUTHORIZED_PATH;
        case UserState.acceptTermsOfService:
            return RouterPath.ACCEPT_TERMS_OF_SERVICE_PATH;
        default:
            return RouterPath.UNAUTHORIZED_PATH;
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
        requiresProcessedWaitlistTicket?: boolean;
        validStates?: UserState[];
        requiredFeaturesEnabled?: (featureToggleConfig: any) => boolean;
    } = to.meta;
    if (meta === undefined) {
        next(Error("Route meta property is undefined"));
        return;
    }

    if (meta.routeIsOidcCallback || meta.stateless) {
        next();
        return;
    }

    const waitlistIsEnabled =
        webClientConfig?.featureToggleConfiguration?.waitingQueue?.enabled ??
        false;
    const isAuthenticated: boolean = authStore.oidcIsAuthenticated;
    let metaRequiresProcessedWaitlistTicket =
        meta.requiresProcessedWaitlistTicket;

    if (
        from.fullPath === RouterPath.QUEUE_FULL_PATH ||
        from.fullPath === RouterPath.QUEUE_PATH
    ) {
        metaRequiresProcessedWaitlistTicket = true;
    }

    logger.debug(
        `Before guard - user is authenticated: ${isAuthenticated}, waitlist enabled: ${waitlistIsEnabled} and meta requires processed waitlist ticket: ${metaRequiresProcessedWaitlistTicket}`
    );

    if (waitlistIsEnabled && metaRequiresProcessedWaitlistTicket) {
        // redirectWhenTicketIsInvalid(to, next); // TODO: implement
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

    if (defaultPath === RouterPath.LOGIN_PATH) {
        next({ path: defaultPath, query: { redirect: to.path } });
        return;
    }

    next({ path: defaultPath });
};
