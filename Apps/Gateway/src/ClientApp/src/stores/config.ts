import {
    ExternalConfiguration,
    IdentityProviderConfiguration,
    OpenIdConnectConfiguration,
    WebClientConfiguration,
} from "@/models/configData";
import { computed, ref } from "vue";
import { defineStore } from "pinia";
import { DateWrapper } from "@/models/dateWrapper";
import { LoadStatus } from "@/models/storeOperations";
import { HttpDelegate } from "@/services/httpDelegate";
import { RestConfigService } from "@/services/restConfigService";
import { LoglevelLogger } from "@/services/loglevelLogger";

export const useConfigStore = defineStore("config", () => {
    const config = ref<ExternalConfiguration>(new ExternalConfiguration());
    const error = ref<boolean>(false);
    const statusMessage = ref<string>("");
    const status = ref<LoadStatus>(LoadStatus.NONE);

    const webConfig = computed<WebClientConfiguration>(
        () => config.value.webClient
    );

    const identityProviders = computed<IdentityProviderConfiguration[]>(
        () => config.value.identityProviders || []
    );

    const openIdConnect = computed<OpenIdConnectConfiguration>(
        () => config.value.openIdConnect
    );

    const isOffline = computed<boolean>(() => {
        if (!webConfig.value) {
            return true;
        }

        const clientIP = webConfig.value.clientIP || "";
        const offlineConfig = webConfig.value.offlineMode;

        if (offlineConfig) {
            const startTime = new DateWrapper(offlineConfig.startDateTime, {
                hasTime: true,
            });
            const endTime = offlineConfig.endDateTime
                ? new DateWrapper(offlineConfig.endDateTime, { hasTime: true })
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
    });

    function setConfigurationRequested() {
        status.value = LoadStatus.REQUESTED;
        error.value = false;
        statusMessage.value = "loading";
    }

    function setConfigurationLoaded(configData: ExternalConfiguration) {
        config.value = configData;
        error.value = false;
        statusMessage.value = "success";
        status.value = LoadStatus.LOADED;
    }

    function setConfigurationError(errorMessage: string) {
        error.value = true;
        config.value = {} as ExternalConfiguration;
        statusMessage.value = errorMessage;
        status.value = LoadStatus.ERROR;
    }

    async function retrieve(): Promise<void> {
        // IoC container hasn't yet been initialized at this point
        const logger = new LoglevelLogger();
        const httpDelegate = new HttpDelegate(logger);
        const configService = new RestConfigService(logger, httpDelegate);

        setConfigurationRequested();
        try {
            const config = await configService.getConfiguration();
            setConfigurationLoaded(config);
        } catch (error: any) {
            setConfigurationError(error?.toString());
            throw error;
        }
    }

    return {
        config,
        error,
        statusMessage,
        status,
        webConfig,
        identityProviders,
        openIdConnect,
        isOffline,
        retrieve,
    };
});
