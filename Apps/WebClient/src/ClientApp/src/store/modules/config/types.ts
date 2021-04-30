import {
    ActionContext,
    ActionTree,
    GetterTree,
    Module,
    MutationTree,
} from "vuex";

import {
    ExternalConfiguration,
    IdentityProviderConfiguration,
    OpenIdConnectConfiguration,
    WebClientConfiguration,
} from "@/models/configData";
import { LoadStatus } from "@/models/storeOperations";
import { RootState } from "@/store/types";

export interface ConfigState {
    config: ExternalConfiguration;
    statusMessage: string;
    error: boolean;
    status: LoadStatus;
}

export interface ConfigGetters extends GetterTree<ConfigState, RootState> {
    identityProviders(state: ConfigState): IdentityProviderConfiguration[];
    openIdConnect(state: ConfigState): OpenIdConnectConfiguration;
    webClient(state: ConfigState): WebClientConfiguration;
    isOffline(state: ConfigState): boolean;
}

type StoreContext = ActionContext<ConfigState, RootState>;
export interface ConfigActions extends ActionTree<ConfigState, RootState> {
    initialize(context: StoreContext, config: ExternalConfiguration): void;
}

export interface ConfigMutations extends MutationTree<ConfigState> {
    configurationRequest(state: ConfigState): void;
    configurationLoaded(
        state: ConfigState,
        configData: ExternalConfiguration
    ): void;
    configurationError(state: ConfigState, errorMessage: string): void;
}

export interface ConfigModule extends Module<ConfigState, RootState> {
    namespaced: boolean;
    state: ConfigState;
    getters: ConfigGetters;
    actions: ConfigActions;
    mutations: ConfigMutations;
}
