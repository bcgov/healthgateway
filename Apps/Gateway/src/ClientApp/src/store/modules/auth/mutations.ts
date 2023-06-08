import { OidcTokenDetails } from "@/models/user";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger } from "@/services/interfaces";

import { AuthMutations, AuthState } from "./types";

export const mutations: AuthMutations = {
    setAuthenticated(state: AuthState, tokenDetails: OidcTokenDetails) {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        logger.verbose("setAuthenticated");

        state.tokenDetails = tokenDetails;
        state.error = null;
    },
    setUnauthenticated(state: AuthState) {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        logger.verbose("setUnauthenticated");

        state.tokenDetails = undefined;
    },
    setError(state: AuthState, error: unknown) {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        logger.verbose("setError");

        state.error = error;
    },
};
