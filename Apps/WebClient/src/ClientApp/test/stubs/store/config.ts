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
import {
    LaboratoryModule,
    LaboratoryState,
} from "@/store/modules/laboratory/types";

const laboratoryState: LaboratoryState = {
    laboratoryOrders: [],
    statusMessage: "",
    status: LoadStatus.NONE,
};

const configGetters: ConfigGetters = {
    identityProviders(): IdentityProviderConfiguration[] {
        return [];
    },
    openIdConnect(): OpenIdConnectConfiguration {
        return {
            authority: "",
            audience: "",
            clientId: "",
            responseType: "",
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
    initialize(): Promise<ExternalConfiguration> {
        return new Promise(() => {});
    },
};

const configMutations: ConfigMutations = {
    configurationRequest(): void {},
    configurationLoaded(): void {},
    configurationError(): void {},
};

const laboratoryStub: LaboratoryModule = {
    state: laboratoryState,
    namespaced: true,
    getters: configGetters,
    actions: configActions,
    mutations: configMutations,
};

export default laboratoryStub;
