import { ExternalConfiguration } from "@/models/configData";
import { LoadStatus } from "@/models/storeOperations";

import { ConfigMutations, ConfigState } from "./types";

export const mutations: ConfigMutations = {
    configurationRequest(state: ConfigState) {
        state.error = false;
        state.statusMessage = "loading";
        state.status = LoadStatus.REQUESTED;
    },
    configurationLoaded(state: ConfigState, configData: ExternalConfiguration) {
        state.config = configData;
        state.error = false;
        state.statusMessage = "success";
        state.status = LoadStatus.LOADED;
    },
    configurationError(state: ConfigState, errorMessage: string) {
        state.error = true;
        state.config = new ExternalConfiguration();
        state.statusMessage = errorMessage;
        state.status = LoadStatus.ERROR;
    },
};
