import { GetterTree } from "vuex";

import {
    AdminClientConfiguration,
    OpenIdConnectConfiguration,
} from "@/models/externalConfiguration";
import { ConfigState, RootState } from "@/models/storeState";

export const getters: GetterTree<ConfigState, RootState> = {
    serviceEndpoints(state: ConfigState): { [id: string]: string } {
        const { config } = state;
        return config.serviceEndpoints;
    },
    openIdConnect(state: ConfigState): OpenIdConnectConfiguration {
        const { config } = state;
        const { openIdConnect } = config;
        return openIdConnect;
    },
    admin(state: ConfigState): AdminClientConfiguration {
        const { config } = state;
        const { admin } = config;
        return admin;
    },
    features(state: ConfigState): { [id: string]: boolean } {
        const { config } = state;
        const { admin } = config;
        const { features } = admin;
        return features ?? {};
    },
};
