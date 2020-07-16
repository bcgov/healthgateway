import { ActionTree } from "vuex";
import { AuthState, RootState } from "@/models/storeState";
import { Route } from "vue-router";
import { IAuthenticationService, IHttpDelegate } from "@/services/interfaces";
import { DELEGATE_IDENTIFIER, SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";

function routeIsOidcCallback(route: Route): boolean {
    if (route.meta.isOidcCallback) {
        return true;
    }
    return false;
}

const authService: IAuthenticationService = container.get<
    IAuthenticationService
>(SERVICE_IDENTIFIER.AuthenticationService);
const httpDelegate: IHttpDelegate = container.get<IHttpDelegate>(
    DELEGATE_IDENTIFIER.HttpDelegate
);

export const actions: ActionTree<AuthState, RootState> = {
    oidcCheckAccess(context, route: Route): Promise<boolean> {
        return new Promise<boolean>((resolve) => {
            if (routeIsOidcCallback(route)) {
                resolve(true);
                return;
            }
            let hasAccess: boolean = true;
            const isAuthenticatedInStore =
                context.state.authentication.idToken !== undefined;

            authService.getUser().then((oidcUser) => {
                if (!oidcUser || oidcUser.expired) {
                    console.log("Could not get the user!");
                    context.dispatch("clearStorage");
                    hasAccess = false;
                } else {
                    context.dispatch("oidcWasAuthenticated", oidcUser);
                    if (!isAuthenticatedInStore) {
                        // We can add events when the user wasnt authenticated and now it is.
                        console.log(
                            "The user was previously unauthenticated, now it is!"
                        );
                    }
                }
                resolve(hasAccess);
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
                    console.log("signinRedirect done");
                    resolve();
                })
                .catch((err) => {
                    context.commit("setOidcError", err);
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
                        console.log(
                            "Warning: User info is too big:",
                            cookieToStoreSize
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
                    console.log("User is invalid.");
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
    signOutOidc(context): void {
        authService.logout().then(() => {
            context.dispatch("clearStorage");
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
