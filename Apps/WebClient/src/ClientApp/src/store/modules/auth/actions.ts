import { ActionTree } from "vuex";

import { AuthState, RootState } from "@/models/storeState";
import { DELEGATE_IDENTIFIER, SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import {
    IAuthenticationService,
    IHttpDelegate,
    ILogger,
} from "@/services/interfaces";

const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);

const authService: IAuthenticationService = container.get<IAuthenticationService>(
    SERVICE_IDENTIFIER.AuthenticationService
);
const httpDelegate: IHttpDelegate = container.get<IHttpDelegate>(
    DELEGATE_IDENTIFIER.HttpDelegate
);

export const actions: ActionTree<AuthState, RootState> = {
    oidcCheckUser(context): Promise<boolean> {
        return new Promise<boolean>((resolve) => {
            const isAuthenticatedInStore =
                context.state.authentication.idToken !== undefined;

            return authService.getUser().then((oidcUser) => {
                if (!oidcUser || oidcUser.expired) {
                    logger.warn("Could not get the user!");
                    context.dispatch("clearStorage");
                    resolve(false);
                } else {
                    context.dispatch("oidcWasAuthenticated", oidcUser);
                    if (!isAuthenticatedInStore) {
                        // We can add events when the user wasnt authenticated and now it is.
                        logger.debug(
                            "The user was previously unauthenticated, now it is!"
                        );
                    }
                    resolve(true);
                }
            });
        });
    },
    authenticateOidc(
        context,
        params: { idpHint: string; redirectPath: string }
    ): Promise<void> {
        return new Promise((resolve, reject) => {
            authService
                .signinRedirect(params.idpHint, params.redirectPath)
                .then(() => {
                    logger.debug("signinRedirect done");
                    resolve();
                })
                .catch((err) => {
                    context.commit("setOidcError", err);
                    logger.error(`setOidcError: ${err}`);
                    reject();
                });
        });
    },
    oidcSignInCallback(context): Promise<string> {
        return new Promise((resolve, reject) => {
            authService
                .signinRedirectCallback()
                .then((oidcUser) => {
                    // Verify that the user info retrieved by the auth service is not large enough than a cookie can store.
                    const cookieToStoreSize = authService.checkOidcUserSize(
                        oidcUser
                    );
                    if (cookieToStoreSize > 4000) {
                        logger.warn(
                            `Warning: User info is too big: ${cookieToStoreSize}`
                        );
                    }

                    context
                        .dispatch("oidcWasAuthenticated", oidcUser)
                        .then(() => {
                            resolve(
                                sessionStorage.getItem(
                                    "vuex_oidc_active_route"
                                ) || "/"
                            );
                        });
                })
                .catch((err) => {
                    context.commit("setOidcError", err);
                    context.commit("setOidcAuthIsChecked");
                    reject(err);
                });
        });
    },
    authenticateOidcSilent(context): Promise<void> {
        return authService
            .signinSilent()
            .then((oidcUser) => {
                context.dispatch("oidcWasAuthenticated", oidcUser);
            })
            .catch((err) => {
                context.commit("setOidcError", err);
                context.commit("setOidcAuthIsChecked");
            });
    },
    oidcWasAuthenticated(context, oidcUser): void {
        httpDelegate.setAuthorizationHeader(oidcUser.access_token);
        context.commit("setOidcAuth", oidcUser);
        context.commit("user/setOidcUserData", oidcUser, { root: true });
        context.commit("setOidcAuthIsChecked");
    },
    getOidcUser(context): Promise<void> {
        return authService
            .getUser()
            .then((oidcUser) => {
                if (!oidcUser || oidcUser.expired) {
                    logger.warn(`User is invalid`);
                    context.dispatch("clearStorage");
                } else {
                    context.dispatch("oidcWasAuthenticated", oidcUser);
                }
            })
            .finally(() => {
                // Clear any stale information from previous login attempts
                authService.clearStaleState();
            });
    },
    signOutOidc(): void {
        authService.logout();
    },
    signOutOidcCallback(context): Promise<string> {
        return new Promise((resolve, reject) => {
            authService
                .signoutRedirectCallback()
                .then(() => {
                    context.dispatch("clearStorage");
                })
                .catch((err) => {
                    context.commit("setOidcError", err);
                    reject(err);
                });
        });
    },
    clearStorage(context): void {
        authService.clearStaleState();
        authService.removeUser().finally(() => {
            httpDelegate.unsetAuthorizationHeader();
            context.commit("unsetOidcAuth");
            context.commit("user/clearUserData", null, { root: true });
        });
    },
};
