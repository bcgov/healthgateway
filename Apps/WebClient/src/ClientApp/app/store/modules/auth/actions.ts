import { ActionTree, Commit, ActionContext } from "vuex";
import { RootState, AuthState } from "@/models/storeState";
import { Route } from "vue-router";

import { IAuthenticationService } from "@/services/interfaces";
import SERVICE_IDENTIFIER from "@/constants/serviceIdentifiers";
import container from "@/inversify.config";

function routeIsOidcCallback(route: Route): boolean {
  if (route.meta.isOidcCallback) {
    return true;
  }
  return false;
}

const authService: IAuthenticationService = container.get<
  IAuthenticationService
>(SERVICE_IDENTIFIER.AuthenticationService);

export const actions: ActionTree<AuthState, RootState> = {
  oidcCheckAccess(context, route) {
    return new Promise<boolean>(resolve => {
      if (routeIsOidcCallback(route)) {
        resolve(true);
        return;
      }
      let hasAccess: boolean = true;
      let isAuthenticatedInStore =
        context.state.authentication.idToken !== undefined;

      authService.getUser().then(user => {
        if (!user || user.expired) {
          context.commit("unsetOidcAuth");
          hasAccess = false;
        } else {
          context.dispatch("oidcWasAuthenticated", user);
          if (!isAuthenticatedInStore) {
            // We can add events when the user wasnt authenticated and now it is.
            console.log("The user was previously unauthenticated, now it is!");
          }
        }
        resolve(hasAccess);
      });
    });
  },
  authenticateOidc({ commit }, { idpHint, redirectPath }) {
    authService.signinRedirect(idpHint, redirectPath).catch(err => {
      commit("setOidcError", err);
    });
  },
  oidcSignInCallback(context) {
    return new Promise((resolve, reject) => {
      authService
        .signinRedirectCallback()
        .then(user => {
          context.dispatch("oidcWasAuthenticated", user).then(() => {
            resolve(sessionStorage.getItem("vuex_oidc_active_route") || "/");
          });
        })
        .catch(err => {
          context.commit("setOidcError", err);
          context.commit("setOidcAuthIsChecked");
          reject(err);
        });
    });
  },
  authenticateOidcSilent(context) {
    return authService
      .signinSilent()
      .then(user => {
        context.dispatch("oidcWasAuthenticated", user);
      })
      .catch(err => {
        context.commit("setOidcError", err);
        context.commit("setOidcAuthIsChecked");
      });
  },
  oidcWasAuthenticated(context, user) {
    context.commit("setOidcAuth", user);
    context.commit("setOidcAuthIsChecked");
  },
  getOidcUser(context) {
    authService.getUser().then(user => {
      if (user) {
        context.commit("setOidcUser", user);
      } else {
        context.commit("unsetOidcAuth");
      }
    });
  },
  signOutOidc(context) {
    authService.logout().then(() => {
      context.commit("unsetOidcAuth");
    });
  }
};
