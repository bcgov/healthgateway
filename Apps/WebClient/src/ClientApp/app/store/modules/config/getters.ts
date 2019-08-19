import { GetterTree } from "vuex";
import { RootState } from "@/models/rootState";
import { IdentityProviderConfiguration } from "@/models/ConfigData";
import { ConfigState } from "@/models/configState";

export const getters: GetterTree<ConfigState, RootState> = {
  identityProviders(state: ConfigState): IdentityProviderConfiguration[] {
    const { config } = state;
    const { identityProviders = [] } = config;
    return identityProviders;
  }
};
