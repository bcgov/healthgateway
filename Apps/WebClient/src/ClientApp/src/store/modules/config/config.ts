import { ExternalConfiguration } from "@/models/configData";
import { LoadStatus } from "@/models/storeOperations";

import { actions } from "./actions";
import { getters } from "./getters";
import { mutations } from "./mutations";
import { ConfigModule, ConfigState } from "./types";

const state: ConfigState = {
    statusMessage: "",
    config: new ExternalConfiguration(),
    error: false,
    status: LoadStatus.NONE,
};

const namespaced = true;

export const config: ConfigModule = {
    namespaced,
    state,
    getters,
    actions,
    mutations,
};
