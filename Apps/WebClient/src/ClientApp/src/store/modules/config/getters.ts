import { GetterTree } from "vuex";
import { ConfigState, RootState } from "@/models/storeState";
import {
    IdentityProviderConfiguration,
    OpenIdConnectConfiguration,
    WebClientConfiguration,
} from "@/models/configData";
import { DateWrapper } from "@/models/dateWrapper";

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
        const clientIP = webclientConfig.clientIP;
        const offlineConfig = webclientConfig.OfflineModeConfiguration;

        if (offlineConfig !== undefined) {
            let startTime = new DateWrapper(offlineConfig.StartDateTime);
            let endTime =
                offlineConfig.EndDateTime === undefined
                    ? DateWrapper.fromNumerical(2050, 12, 31)
                    : new DateWrapper(offlineConfig.EndDateTime);

            let now = new DateWrapper();

            if (
                now.isAfterOrSame(startTime) &&
                now.isBeforeOrSame(endTime) &&
                !offlineConfig.IPWhitelist.includes(clientIP)
            ) {
                return true;
            }
        }
        return false;
    },
};
