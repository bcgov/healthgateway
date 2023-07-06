import { defineStore } from "pinia";
import { computed, ref } from "vue";

import { container } from "@/ioc/container";
import { DELEGATE_IDENTIFIER, SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { OidcTokenDetails } from "@/models/user";
import {
    IAuthenticationService,
    IHttpDelegate,
    ILogger,
} from "@/services/interfaces";
import { useUserStore } from "@/stores/user";

export const useAuthStore = defineStore("auth", () => {
    const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
    const authService = container.get<IAuthenticationService>(
        SERVICE_IDENTIFIER.AuthenticationService
    );
    const httpDelegate = container.get<IHttpDelegate>(
        DELEGATE_IDENTIFIER.HttpDelegate
    );

    const userStore = useUserStore();

    const tokenDetails = ref<OidcTokenDetails>();
    const error = ref<unknown>();
    let refreshTimeout: NodeJS.Timeout | undefined;

    const oidcIsAuthenticated = computed(
        () =>
            tokenDetails.value !== undefined &&
            tokenDetails.value.idToken.length > 0
    );

    const oidcError = computed(() => error.value);

    function setAuthenticated(incomingTokenDetails: OidcTokenDetails) {
        logger.verbose("setAuthenticated");
        tokenDetails.value = incomingTokenDetails;
        error.value = null;
    }

    function setUnauthenticated() {
        logger.verbose("setUnauthenticated");
        tokenDetails.value = undefined;
    }

    function setError(errorRaised: unknown) {
        logger.error(`setError: ${errorRaised}`);
        error.value = errorRaised;
    }

    async function checkStatus(): Promise<boolean> {
        const tokenDetailsFromService = authService.getOidcTokenDetails();
        if (!tokenDetailsFromService || tokenDetailsFromService.expired) {
            if (oidcIsAuthenticated.value) {
                logger.verbose("Authentication status was not in sync");
                clearStorage();
            }
            return false;
        }
        const userInfo = await authService.getOidcUserInfo();
        userStore.setOidcUserInfo(userInfo);
        handleSuccessfulAuthentication(tokenDetailsFromService);
        return true;
    }

    async function signIn(
        redirectPath: string,
        idpHint?: string
    ): Promise<void> {
        try {
            // EventBus.$emit(
            //     EventMessageName.UnregisterOnBeforeUnloadWaitlistListener
            // );
            const tokenDetails = await authService.signIn(
                redirectPath,
                idpHint
            );
            const userInfo = await authService.getOidcUserInfo();
            userStore.setOidcUserInfo(userInfo);
            handleSuccessfulAuthentication(tokenDetails);
            logger.verbose("Successfully signed in");
        } catch (errorRaised) {
            logger.verbose("Failed to sign in");
            setError(errorRaised);
            throw errorRaised;
        } finally {
            // EventBus.$emit(EventMessageName.RegisterOnBeforeUnloadWaitlistListener);
        }
    }

    function signOut(): void {
        if (refreshTimeout !== undefined) {
            clearTimeout(refreshTimeout);
            refreshTimeout = undefined;
        }
        userStore.clearUserData();
        authService.signOut().then(() => {
            logger.verbose("Successfully signed out");
        });
    }

    function clearStorage() {
        if (refreshTimeout !== undefined) {
            clearTimeout(refreshTimeout);
            refreshTimeout = undefined;
        }
        authService.clearState();
        httpDelegate.unsetAuthorizationHeader();
        setUnauthenticated();
        userStore.clearUserData();
    }

    function handleSuccessfulAuthentication(tokenDetails: OidcTokenDetails) {
        httpDelegate.setAuthorizationHeader(tokenDetails.accessToken);
        setAuthenticated(tokenDetails);
        const refreshTimeMilliseconds = tokenDetails.refreshTokenTime * 1000;
        const currentTimeMilliseconds = new Date().getTime();
        const accessTokenDuration =
            refreshTimeMilliseconds - currentTimeMilliseconds;
        if (accessTokenDuration > 0) {
            if (refreshTimeout !== undefined) {
                clearTimeout(refreshTimeout);
            }
            refreshTimeout = setTimeout(
                () => refreshToken(),
                accessTokenDuration
            );
        }
    }

    async function refreshToken() {
        logger.info("Refreshing access token");
        try {
            const refreshed = await authService.refreshToken();
            if (refreshed) {
                logger.verbose("Refreshed access token");
                const tokenDetails = authService.getOidcTokenDetails();
                if (tokenDetails !== null) {
                    handleSuccessfulAuthentication(tokenDetails);
                }
            } else {
                logger.verbose("Access token refresh not required");
            }
        } catch (err) {
            logger.warn("Access token expired unexpectedly");
            clearStorage();
        }
    }

    return {
        oidcIsAuthenticated,
        oidcError,
        checkStatus,
        clearStorage,
        signIn,
        signOut,
    };
});
