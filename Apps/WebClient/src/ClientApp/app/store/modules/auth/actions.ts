import { ActionTree, Commit, ActionContext } from "vuex";
import { RootState, AuthState } from "@/models/storeState";
import { Route } from "vue-router";

import { IAuthenticationService, IHttpDelegate } from "@/services/interfaces";
import SERVICE_IDENTIFIER, {
  DELEGATE_IDENTIFIER
} from "@/constants/serviceIdentifiers";
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
const httpDelegate: IHttpDelegate = container.get<IHttpDelegate>(
  DELEGATE_IDENTIFIER.HttpDelegate
);

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

      authService.getUser().then(oidcUser => {
        if (!oidcUser || oidcUser.expired) {
          context.dispatch("clearStorage");
          hasAccess = false;
        } else {
          context.dispatch("oidcWasAuthenticated", oidcUser);
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
        .then(oidcUser => {
          context.dispatch("oidcWasAuthenticated", oidcUser).then(() => {
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
      .then(oidcUser => {
        context.dispatch("oidcWasAuthenticated", oidcUser);
      })
      .catch(err => {
        context.commit("setOidcError", err);
        context.commit("setOidcAuthIsChecked");
      });
  },
  oidcWasAuthenticated(context, oidcUser) {
    context.commit("setOidcAuth", oidcUser);
    context.commit("user/setOidcUserData", oidcUser, { root: true });
    context.commit("setOidcAuthIsChecked");
    context.dispatch("setHttpToken", oidcUser.access_token);
  },
  getOidcUser(context) {
    authService
      .getUser()
      .then(oidcUser => {
        if (!oidcUser || oidcUser.expired) {
          context.dispatch("clearStorage");
        } else {
          context.commit("setOidcAuth", oidcUser);
        }
      })
      .finally(() => {
        // Clear any stale information from previous login attempts
        authService.clearStaleState();
      });
  },
  signOutOidc(context) {
    authService.logout().then(() => {
      context.dispatch("clearStorage");
    });
  },
  clearStorage(context) {
    authService.removeUser().finally(() => {
      httpDelegate.unsetAuthorizationHeader();
      context.commit("unsetOidcAuth");
      context.commit("user/clearUserData", null, { root: true });
    });
  },
  setHttpToken(context, token) {
    httpDelegate.setAuthorizationHeader(token);
  }
};
