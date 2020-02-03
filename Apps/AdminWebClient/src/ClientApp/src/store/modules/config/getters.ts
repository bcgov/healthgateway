import { GetterTree } from "vuex";
import { RootState, ConfigState } from "@/models/storeState";
import {
  IdentityProviderConfiguration,
  OpenIdConnectConfiguration,
  AdminClientConfiguration
} from "@/models/externalConfiguration";

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
  admin(state: ConfigState): AdminClientConfiguration {
    const { config } = state;
    const { admin } = config;
    return admin;
  }
};
