import Vue from "vue";

import { OidcTokenDetails } from "@/models/user";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger } from "@/services/interfaces";

import { AuthMutations, AuthState } from "./types";

export const mutations: AuthMutations = {
    setOidcAuth(state: AuthState, tokenDetails: OidcTokenDetails) {
        const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
        logger.verbose("setOidcAuth");

        Vue.set(state.authentication, "accessToken", tokenDetails.accessToken);
        Vue.set(state.authentication, "idToken", tokenDetails.idToken);

        state.isAuthenticated = tokenDetails.idToken.length > 0;
        state.error = null;
    },
    unsetOidcAuth(state: AuthState) {
        const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
        logger.verbose("unsetOidcAuth");

        Vue.set(state.authentication, "accessToken", undefined);
        Vue.set(state.authentication, "idToken", undefined);

        state.isAuthenticated = false;
    },
    setOidcAuthIsChecked(state: AuthState) {
        const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
        logger.verbose("setOidcAuthIsChecked");

        Vue.set(state.authentication, "isChecked", true);
    },
    setOidcError(state: AuthState, error: unknown) {
        const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
        logger.verbose("setOidcError");

        state.error = error;
    },
};
