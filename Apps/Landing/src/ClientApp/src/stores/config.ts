﻿import { defineStore } from "pinia";
import { computed, ref } from "vue";

import {
    ExternalConfiguration,
    IdentityProviderConfiguration,
} from "@/models/configData";
import { LoadStatus } from "@/models/storeOperations";
import { HttpDelegate } from "@/services/httpDelegate";
import { LoglevelLogger } from "@/services/loglevelLogger";
import { RestConfigService } from "@/services/restConfigService";

export const useConfigStore = defineStore("config", () => {
    const config = ref(new ExternalConfiguration());
    const error = ref(false);
    const statusMessage = ref("");
    const status = ref(LoadStatus.NONE);

    const webConfig = computed(() => config.value.webClient);

    const identityProviders = computed<IdentityProviderConfiguration[]>(
        () => config.value.identityProviders || []
    );

    const openIdConnect = computed(() => config.value.openIdConnect);

    const isOffline = computed(() => {
        if (!webConfig.value) {
            return true;
        }

        const clientIP = webConfig.value.clientIP ?? "";
        const offlineConfig = webConfig.value.offlineMode;

        if (offlineConfig) {
            const startTime = new Date(offlineConfig.startDateTime);
            const endTime = offlineConfig.endDateTime
                ? new Date(offlineConfig.endDateTime)
                : new Date(2050, 11, 31);

            const now = new Date();
            if (
                now >= startTime &&
                now <= endTime &&
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
