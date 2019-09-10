import Vue from "vue";
import { MutationTree } from "vuex";
import { AuthState } from "@/models/storeState";
import { User as OidcUser } from "oidc-client";

export const mutations: MutationTree<AuthState> = {
  setOidcAuth(state, user: OidcUser) {
    console.log("setOidcAuth");
    Vue.set(state.authentication, "accessToken", user.access_token);
    Vue.set(state.authentication, "scopes", user.scopes);
    Vue.set(state.authentication, "idToken", user.id_token);
    Vue.set(state.authentication, "oidcUser", user);

    state.isAuthenticated =
      user.id_token === undefined ? false : user.id_token.length > 0;

    if (user.profile === undefined) {
      Vue.set(state.authentication, "acceptedTermsOfService", false);
    } else {
      Vue.set(
        state.authentication,
        "acceptedTermsOfService",
        user.profile.acceptedTermsOfService
      );
    }
    state.error = null;
  },
  unsetOidcAuth(state) {
    console.log("unsetOidcAuth");
    Vue.set(state.authentication, "accessToken", undefined);
    Vue.set(state.authentication, "scopes", undefined);
    Vue.set(state.authentication, "idToken", undefined);
    Vue.set(state.authentication, "oidcUser", undefined);
    Vue.set(state.authentication, "acceptedTermsOfService", false);
    state.isAuthenticated = false;
  },
  setOidcAuthIsChecked(state) {
    console.log("setOidcAuthIsChecked");
    Vue.set(state.authentication, "isChecked", true);
  },
  setOidcError(state, error) {
    console.log("setOidcError");
    state.error = error && error.message ? error.message : error;
  }
};
