import { MutationTree } from "vuex";

import { ExternalConfiguration } from "@/models/configData";
import { ConfigState, LoadStatus } from "@/models/storeState";

export const mutations: MutationTree<ConfigState> = {
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
