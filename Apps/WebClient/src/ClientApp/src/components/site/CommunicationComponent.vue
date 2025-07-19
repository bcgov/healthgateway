<script setup lang="ts">
import { computed, ref, watch } from "vue";
import { useRoute } from "vue-router";

import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import Communication, { CommunicationType } from "@/models/communication";
import { ResultError } from "@/models/errors";
import RequestResult from "@/models/requestResult";
import { ICommunicationService, ILogger } from "@/services/interfaces";
import { useAuthStore } from "@/stores/auth";
import { useConfigStore } from "@/stores/config";
import { useUserStore } from "@/stores/user";

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const communicationService = container.get<ICommunicationService>(
    SERVICE_IDENTIFIER.CommunicationService
);
const route = useRoute();
const authStore = useAuthStore();
const configStore = useConfigStore();
const userStore = useUserStore();

const bannerCommunication = ref<Communication | null>(null);
const inAppCommunication = ref<Communication | null>(null);

const displayInAppBanner = computed(() => {
    return (
        authStore.oidcIsAuthenticated &&
        userStore.userIsRegistered &&
        userStore.isValidIdentityProvider &&
        !configStore.isOffline
    );
});
const hasCommunication = computed(() =>
    displayInAppBanner.value
        ? inAppCommunication.value != null
        : bannerCommunication.value != null
);
const text = computed(() =>
    displayInAppBanner.value
        ? inAppCommunication.value?.text
        : bannerCommunication.value?.text
);

function setCommunication(
    type: CommunicationType,
    communication: Communication | null
) {
    if (type === CommunicationType.Banner) {
        bannerCommunication.value = communication;
    } else {
        inAppCommunication.value = communication;
    }
}

function fetchCommunication(type: CommunicationType): void {
    communicationService
        .getActive(type)
        .then((requestResult: RequestResult<Communication | null>) =>
            setCommunication(type, requestResult.resourcePayload)
        )
        .catch((err: ResultError) => {
            if (err.statusCode === 429) {
                setCommunication(type, {
                    text: "We are experiencing higher than usual site traffic, which may cause delays in accessing your health records. Please try again later.",
                    communicationTypeCode: type,
                });
            }
            logger.error(JSON.stringify(err));
        });
}

watch(
    () => route.path,
    () => fetchCommunication(CommunicationType.InApp)
);

fetchCommunication(CommunicationType.Banner);
fetchCommunication(CommunicationType.InApp);
</script>

<template>
    <v-banner v-if="hasCommunication" class="bg-info-light justify-center">
        <v-banner-text>
            <!-- eslint-disable vue/no-v-html -->
            <div data-testid="communicationBanner" v-html="text" />
            <!-- eslint-enable vue/no-v-html -->
        </v-banner-text>
    </v-banner>
</template>
