<script setup lang="ts">
import { computed, ref, watch } from "vue";
import { useRoute, useStore } from "vue-composition-wrapper";

import Communication, { CommunicationType } from "@/models/communication";
import { ResultError } from "@/models/errors";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ICommunicationService, ILogger } from "@/services/interfaces";

const store = useStore();
const route = useRoute();
const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const communicationService = container.get<ICommunicationService>(
    SERVICE_IDENTIFIER.CommunicationService
);

const bannerCommunication = ref<Communication | null>(null);
const inAppCommunication = ref<Communication | null>(null);

const oidcIsAuthenticated = computed<boolean>(
    () => store.getters["auth/oidcIsAuthenticated"]
);

const userIsRegistered = computed<boolean>(
    () => store.getters["user/userIsRegistered"]
);

const isValidIdentityProvider = computed<boolean>(
    () => store.getters["user/isValidIdentityProvider"]
);

const isOffline = computed<boolean>(() => store.getters["config/isOffline"]);

const displayInAppBanner = computed<boolean>(() => {
    return (
        oidcIsAuthenticated.value &&
        userIsRegistered.value &&
        isValidIdentityProvider.value &&
        !isOffline.value
    );
});

const hasCommunication = computed<boolean>(() => {
    if (displayInAppBanner.value) {
        return inAppCommunication.value != null;
    } else {
        return bannerCommunication.value != null;
    }
});

const text = computed<string>(() => {
    if (displayInAppBanner.value) {
        return inAppCommunication.value ? inAppCommunication.value.text : "";
    } else {
        return bannerCommunication.value ? bannerCommunication.value.text : "";
    }
});

function fetchCommunication(type: CommunicationType): void {
    communicationService
        .getActive(type)
        .then((requestResult) => {
            if (type === CommunicationType.Banner) {
                bannerCommunication.value = requestResult.resourcePayload;
            } else {
                inAppCommunication.value = requestResult.resourcePayload;
            }
        })
        .catch((err: ResultError) => {
            if (err.statusCode === 429) {
                const communication: Communication = {
                    text: "We are experiencing higher than usual site traffic, which may cause delays in accessing your health records. Please try again later.",
                    communicationTypeCode: type,
                };

                if (type === CommunicationType.Banner) {
                    bannerCommunication.value = communication;
                } else {
                    inAppCommunication.value = communication;
                }
            }
            logger.error(JSON.stringify(err));
        });
}

watch(
    () => route.value.path,
    () => fetchCommunication(CommunicationType.InApp)
);

// Created Hook
fetchCommunication(CommunicationType.Banner);
fetchCommunication(CommunicationType.InApp);
</script>

<template>
    <b-row v-if="hasCommunication">
        <b-col class="p-0 m-0">
            <div
                data-testid="communicationBanner"
                class="text-center communication p-2"
                v-html="text"
            />
        </b-col>
    </b-row>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

.communication {
    background-color: $bcgold;
    color: black;

    :last-child {
        margin-bottom: 0;
    }
}
</style>
