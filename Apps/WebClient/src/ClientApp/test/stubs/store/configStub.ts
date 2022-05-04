import { voidMethod, voidPromise } from "@test/stubs/util";

import { RegistrationStatus } from "@/constants/registrationStatus";
import {
    ExternalConfiguration,
    IdentityProviderConfiguration,
    OpenIdConnectConfiguration,
    WebClientConfiguration,
} from "@/models/configData";
import { LoadStatus } from "@/models/storeOperations";
import {
    ConfigActions,
    ConfigGetters,
    ConfigModule,
    ConfigMutations,
    ConfigState,
} from "@/store/modules/config/types";

const configState: ConfigState = {
    statusMessage: "",
    config: new ExternalConfiguration(),
    error: false,
    status: LoadStatus.NONE,
};

const configGetters: ConfigGetters = {
    identityProviders(): IdentityProviderConfiguration[] {
        return [];
    },
    openIdConnect(): OpenIdConnectConfiguration {
        return {
            authority: "",
            clientId: "",
            scope: "",
            callbacks: {},
        };
    },
    webClient(): WebClientConfiguration {
        return {
            logLevel: "",
            timeouts: { idle: 0, logoutRedirect: "", resendSMS: 1 },
            registrationStatus: RegistrationStatus.Open,
            externalURLs: {},
            modules: { Note: true },
            hoursForDeletion: 1,
            minPatientAge: 16,
            maxDependentAge: 12,
        };
    },
    isOffline(): boolean {
        return false;
    },
};

const configActions: ConfigActions = {
    initialize: voidPromise,
};

const configMutations: ConfigMutations = {
    configurationRequest: voidMethod,
    configurationLoaded: voidMethod,
    configurationError: voidMethod,
};

const configStub: ConfigModule = {
    state: configState,
    namespaced: true,
    getters: configGetters,
    actions: configActions,
    mutations: configMutations,
};

export default configStub;
