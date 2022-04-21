import container from "@/plugins/container";
import { DELEGATE_IDENTIFIER, SERVICE_IDENTIFIER } from "@/plugins/inversify";
import {
    IAuthenticationService,
    IHttpDelegate,
    ILogger,
} from "@/services/interfaces";

import { AuthActions } from "./types";

export const actions: AuthActions = {
    async signIn(
        context,
        params: { idpHint: string; redirectPath: string }
    ): Promise<void> {
        const authService: IAuthenticationService =
            container.get<IAuthenticationService>(
                SERVICE_IDENTIFIER.AuthenticationService
            );
        const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);

        try {
            const tokenDetails = await authService.signIn(
                params.redirectPath,
                params.idpHint
            );
            const userInfo = await authService.getOidcUserInfo();

            context.dispatch("oidcWasAuthenticated", {
                tokenDetails,
                userInfo,
            });

            logger.verbose("Signed in successfully");
        } catch (err) {
            context.commit("setOidcError", err);
            context.commit("setOidcAuthIsChecked");

            logger.verbose("Failed to sign in");
            logger.error(`setOidcError: ${err}`);

            throw err;
        }
    },
    signOut(): void {
        const authService = container.get<IAuthenticationService>(
            SERVICE_IDENTIFIER.AuthenticationService
        );

        authService.signOut();
    },
    async getOidcUser(context): Promise<void> {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        const authService = container.get<IAuthenticationService>(
            SERVICE_IDENTIFIER.AuthenticationService
        );

        const tokenDetails = authService.getOidcTokenDetails();
        if (!tokenDetails || tokenDetails.expired) {
            logger.verbose("User is invalid");
        } else {
            logger.verbose("User is valid");

            const userInfo = await authService.getOidcUserInfo();
            context.dispatch("oidcWasAuthenticated", {
                tokenDetails,
                userInfo,
            });
        }
    },
    async oidcCheckUser(context): Promise<boolean> {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        const authService = container.get<IAuthenticationService>(
            SERVICE_IDENTIFIER.AuthenticationService
        );

        const previouslyUnauthenticated =
            context.state.authentication.idToken === undefined;

        const tokenDetails = authService.getOidcTokenDetails();
        if (!tokenDetails || tokenDetails.expired) {
            logger.warn("Could not get the user!");
            context.dispatch("clearStorage");
            return false;
        } else {
            const userInfo = await authService.getOidcUserInfo();
            context.dispatch("oidcWasAuthenticated", {
                tokenDetails,
                userInfo,
            });
            if (previouslyUnauthenticated) {
                logger.verbose(
                    "The user was previously unauthenticated but is now authenticated"
                );
            }
            return true;
        }
    },
    oidcWasAuthenticated(context, { tokenDetails, userInfo }): void {
        const httpDelegate = container.get<IHttpDelegate>(
            DELEGATE_IDENTIFIER.HttpDelegate
        );

        httpDelegate.setAuthorizationHeader(tokenDetails.accessToken);
        context.commit("setOidcAuth", tokenDetails);
        context.commit("user/setOidcUserInfo", userInfo, { root: true });
        context.commit("setOidcAuthIsChecked");
    },
    clearStorage(context): void {
        const authService = container.get<IAuthenticationService>(
            SERVICE_IDENTIFIER.AuthenticationService
        );
        const httpDelegate = container.get<IHttpDelegate>(
            DELEGATE_IDENTIFIER.HttpDelegate
        );

        authService.clearState();
        httpDelegate.unsetAuthorizationHeader();
        context.commit("unsetOidcAuth");
        context.commit("user/clearUserData", null, { root: true });
    },
};
