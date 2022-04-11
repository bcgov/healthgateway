import { User as OidcUser } from "oidc-client";
import Vue from "vue";

import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger } from "@/services/interfaces";

import { AuthMutations, AuthState } from "./types";

export const mutations: AuthMutations = {
    setOidcAuth(state: AuthState, user: OidcUser) {
        const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
        logger.verbose("setOidcAuth");
        Vue.set(state.authentication, "accessToken", user.access_token);
        Vue.set(state.authentication, "scopes", user.scopes);
        Vue.set(state.authentication, "idToken", user.id_token);
        Vue.set(state.authentication, "identityProvider", user.profile.idp);

        const isAuthenticated =
            user.id_token === undefined ? false : user.id_token.length > 0;

        Vue.set(state, "isAuthenticated", isAuthenticated);

        state.error = null;
    },
    unsetOidcAuth(state: AuthState) {
        const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
        logger.verbose("unsetOidcAuth");
        Vue.set(state.authentication, "accessToken", undefined);
        Vue.set(state.authentication, "scopes", undefined);
        Vue.set(state.authentication, "idToken", undefined);
        Vue.set(state.authentication, "identityProvider", "");
        state.isAuthenticated = false;
    },
    setOidcAuthIsChecked(state: AuthState) {
        const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
        logger.verbose("setOidcAuthIsChecked");
        Vue.set(state.authentication, "isChecked", true);
    },
    setOidcError(state: AuthState, error: unknown) {
        const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
        logger.verbose("setOidcError...");
        state.error = error;
    },
};
