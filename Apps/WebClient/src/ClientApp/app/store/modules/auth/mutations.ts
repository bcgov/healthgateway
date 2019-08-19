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
    Vue.set(state.authentication, "user", user.profile);
    state.isAuthenticated =
      user.id_token === undefined ? false : user.id_token.length > 0;
    state.error = null;
  },
  setOidcUser(state, user: OidcUser) {
    console.log("setOidcUser");
    state.authentication.user = user.profile;
  },
  unsetOidcAuth(state) {
    console.log("unsetOidcAuth");
    Vue.set(state.authentication, "accessToken", undefined);
    Vue.set(state.authentication, "scopes", undefined);
    Vue.set(state.authentication, "idToken", undefined);
    Vue.set(state.authentication, "user", undefined);

    state.isAuthenticated = false;
  },
  setOidcAuthIsChecked(state) {
    console.log("setOidcAuthIsChecked");
    Vue.set(state.authentication, "isChecked", true);
  },
  setOidcEventsAreBound(state) {
    console.log("setOidcEventsAreBound");
    Vue.set(state.authentication, "eventsAreBound", true);
  },
  setOidcError(state, error) {
    console.log("setOidcError");
    state.error = error && error.message ? error.message : error;
  }
};
