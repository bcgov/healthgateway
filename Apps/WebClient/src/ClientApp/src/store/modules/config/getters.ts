import { GetterTree } from "vuex";

import {
    IdentityProviderConfiguration,
    OpenIdConnectConfiguration,
    WebClientConfiguration,
} from "@/models/configData";
import { DateWrapper } from "@/models/dateWrapper";
import { ConfigState, RootState } from "@/models/storeState";

export const getters: GetterTree<ConfigState, RootState> = {
    identityProviders(state: ConfigState): IdentityProviderConfiguration[] {
        const { config } = state;
        const { identityProviders = [] } = config;
        return identityProviders;
    },
    openIdConnect(state: ConfigState): OpenIdConnectConfiguration {
        const { config } = state;
        const { openIdConnect } = config;
        return openIdConnect;
    },
    webClient(state: ConfigState): WebClientConfiguration {
        const { config } = state;
        const { webClient } = config;
        return webClient;
    },
    isOffline(state: ConfigState): boolean {
        const webclientConfig = state.config.webClient;
        const clientIP = webclientConfig.clientIP || "";
        const offlineConfig = webclientConfig.offlineMode;

        if (offlineConfig) {
            const startTime = new DateWrapper(offlineConfig.startDateTime);
            const endTime = offlineConfig.endDateTime
                ? new DateWrapper(offlineConfig.endDateTime)
                : DateWrapper.fromNumerical(2050, 12, 31);

            const now = new DateWrapper();
            if (
                now.isAfterOrSame(startTime) &&
                now.isBeforeOrSame(endTime) &&
                !offlineConfig.whitelist.includes(clientIP)
            ) {
                return true;
            }
        }
        return false;
    },
};
