import { OidcTokenDetails } from "@/models/user";
import container from "@/plugins/container";
import { DELEGATE_IDENTIFIER, SERVICE_IDENTIFIER } from "@/plugins/inversify";
import {
    IAuthenticationService,
    IHttpDelegate,
    ILogger,
} from "@/services/interfaces";

import { AuthActions } from "./types";

/** Timeout that fires the "refreshToken" action. */
let refreshTimeout: NodeJS.Timeout | undefined = undefined;

export const actions: AuthActions = {
    async checkStatus(context): Promise<boolean> {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        const authService = container.get<IAuthenticationService>(
            SERVICE_IDENTIFIER.AuthenticationService
        );

        const tokenDetails = authService.getOidcTokenDetails();
        if (!tokenDetails || tokenDetails.expired) {
            if (context.getters["oidcIsAuthenticated"]) {
                logger.verbose("Authentication status was not in sync");
                context.dispatch("clearStorage");
            }
            return false;
        }
        const userInfo = await authService.getOidcUserInfo();

        context.commit("user/setOidcUserInfo", userInfo, { root: true });
        context.dispatch("handleSuccessfulAuthentication", tokenDetails);

        return true;
    },
    async signIn(
        context,
        params: { idpHint: string; redirectPath: string }
    ): Promise<void> {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        const authService = container.get<IAuthenticationService>(
            SERVICE_IDENTIFIER.AuthenticationService
        );

        try {
            const tokenDetails = await authService.signIn(
                params.redirectPath,
                params.idpHint
            );
            const userInfo = await authService.getOidcUserInfo();

            context.commit("user/setOidcUserInfo", userInfo, { root: true });
            context.dispatch("handleSuccessfulAuthentication", tokenDetails);

            logger.verbose("Successfully signed in");
        } catch (err) {
            context.commit("setError", err);

            logger.verbose("Failed to sign in");
            logger.error(`setError: ${err}`);

            throw err;
        }
    },
    signOut(): void {
        const authService = container.get<IAuthenticationService>(
            SERVICE_IDENTIFIER.AuthenticationService
        );

        if (refreshTimeout !== undefined) {
            clearTimeout(refreshTimeout);
            refreshTimeout = undefined;
        }

        authService.signOut();
    },
    async refreshToken(context): Promise<void> {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        const authService = container.get<IAuthenticationService>(
            SERVICE_IDENTIFIER.AuthenticationService
        );

        logger.info("Refreshing access token");

        try {
            const refreshed = await authService.refreshToken();
            if (refreshed) {
                logger.verbose("Refreshed access token");

                const tokenDetails = authService.getOidcTokenDetails();
                if (tokenDetails !== null) {
                    context.dispatch(
                        "handleSuccessfulAuthentication",
                        tokenDetails
                    );
                }
            } else {
                logger.verbose("Access token refresh not required");
            }
        } catch (err) {
            logger.warn("Access token expired unexpectedly");
            context.dispatch("clearStorage");
        }
    },
    handleSuccessfulAuthentication(
        context,
        tokenDetails: OidcTokenDetails
    ): void {
        const httpDelegate = container.get<IHttpDelegate>(
            DELEGATE_IDENTIFIER.HttpDelegate
        );

        httpDelegate.setAuthorizationHeader(tokenDetails.accessToken);
        context.commit("setAuthenticated", tokenDetails);

        const refreshTimeMilliseconds = tokenDetails.refreshTokenTime * 1000;
        const currentTimeMilliseconds = new Date().getTime();
        const accessTokenDuration =
            refreshTimeMilliseconds - currentTimeMilliseconds;

        if (accessTokenDuration > 0) {
            if (refreshTimeout !== undefined) {
                clearTimeout(refreshTimeout);
            }
            refreshTimeout = setTimeout(
                () => context.dispatch("refreshToken"),
                accessTokenDuration
            );
        }
    },
    clearStorage(context): void {
        const authService = container.get<IAuthenticationService>(
            SERVICE_IDENTIFIER.AuthenticationService
        );
        const httpDelegate = container.get<IHttpDelegate>(
            DELEGATE_IDENTIFIER.HttpDelegate
        );

        if (refreshTimeout !== undefined) {
            clearTimeout(refreshTimeout);
            refreshTimeout = undefined;
        }

        authService.clearState();
        httpDelegate.unsetAuthorizationHeader();
        context.commit("setUnauthenticated");
        context.commit("user/clearUserData", null, { root: true });
    },
};
