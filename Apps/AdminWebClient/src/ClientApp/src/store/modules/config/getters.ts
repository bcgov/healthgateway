import { GetterTree } from "vuex";
import { RootState, ConfigState } from "@/models/storeState";
import {
    OpenIdConnectConfiguration,
    AdminClientConfiguration
} from "@/models/externalConfiguration";

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
    }
};
