import { GetterTree } from "vuex";
import { RootState, ConfigState } from "@/models/storeState";
import { IdentityProviderConfiguration } from "@/models/ConfigData";

export const getters: GetterTree<ConfigState, RootState> = {
  identityProviders(state: ConfigState): IdentityProviderConfiguration[] {
    const { config } = state;
    const { identityProviders = [] } = config;
    return identityProviders;
  }
};
