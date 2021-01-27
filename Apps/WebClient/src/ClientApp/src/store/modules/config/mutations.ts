import { MutationTree } from "vuex";

import { ExternalConfiguration } from "@/models/configData";
import { ConfigState, StateType } from "@/models/storeState";

export const mutations: MutationTree<ConfigState> = {
    configurationRequest(state: ConfigState) {
        state.error = false;
        state.statusMessage = "loading";
        state.stateType = StateType.REQUESTED;
    },
    configurationLoaded(state: ConfigState, configData: ExternalConfiguration) {
        state.config = configData;
        state.error = false;
        state.statusMessage = "success";
        state.stateType = StateType.INITIALIZED;
    },
    configurationError(state: ConfigState, errorMessage: string) {
        state.error = true;
        state.config = new ExternalConfiguration();
        state.statusMessage = errorMessage;
        state.stateType = StateType.ERROR;
    },
};
