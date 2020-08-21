import Vue from "vue";
import { MutationTree } from "vuex";
import { AuthState } from "@/models/storeState";
import container from "@/plugins/inversify.config";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger } from "@/services/interfaces";
import { User as OidcUser } from "oidc-client";

const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);

export const mutations: MutationTree<AuthState> = {
    setOidcAuth(state: AuthState, user: OidcUser) {
        logger.verbose("setOidcAuth");
        Vue.set(state.authentication, "accessToken", user.access_token);
        Vue.set(state.authentication, "scopes", user.scopes);
        Vue.set(state.authentication, "idToken", user.id_token);

        const isAuthenticated =
            user.id_token === undefined ? false : user.id_token.length > 0;

        Vue.set(state, "isAuthenticated", isAuthenticated);

        state.error = null;
    },
    unsetOidcAuth(state: AuthState) {
        logger.verbose("unsetOidcAuth");
        Vue.set(state.authentication, "accessToken", undefined);
        Vue.set(state.authentication, "scopes", undefined);
        Vue.set(state.authentication, "idToken", undefined);
        state.isAuthenticated = false;
    },
    setOidcAuthIsChecked(state: AuthState) {
        logger.verbose("setOidcAuthIsChecked");
        Vue.set(state.authentication, "isChecked", true);
    },
    setOidcError(state: AuthState, error: any) {
        logger.verbose("setOidcError...");
        state.error = error && error.message ? error.message : error;
    },
};
