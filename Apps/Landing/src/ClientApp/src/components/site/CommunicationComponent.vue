<script setup lang="ts">
import { computed, ref } from "vue";

import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import Communication, { CommunicationType } from "@/models/communication";
import { ResultError } from "@/models/errors";
import RequestResult from "@/models/requestResult";
import { ICommunicationService, ILogger } from "@/services/interfaces";

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const communicationService = container.get<ICommunicationService>(
    SERVICE_IDENTIFIER.CommunicationService
);

const bannerCommunication = ref<Communication | null>(null);

const hasCommunication = computed(() => bannerCommunication.value != null);
const text = computed(() => bannerCommunication.value?.text);

function setCommunication(communication: Communication | null) {
    bannerCommunication.value = communication;
}

function fetchCommunication(type: CommunicationType): void {
    communicationService
        .getActive(CommunicationType.Banner)
        .then((requestResult: RequestResult<Communication | null>) =>
            setCommunication(requestResult.resourcePayload)
        )
        .catch((err: ResultError) => {
            if (err.statusCode === 429) {
                setCommunication({
                    text: "We are experiencing higher than usual site traffic, which may cause delays in accessing your health records. Please try again later.",
                    communicationTypeCode: type,
                });
            }
            logger.error(JSON.stringify(err));
        });
}

fetchCommunication(CommunicationType.Banner);
</script>

<template>
    <v-banner v-if="hasCommunication" class="bg-accent justify-center">
        <v-banner-text>
            <!-- eslint-disable vue/no-v-html -->
            <div data-testid="communicationBanner" v-html="text" />
            <!-- eslint-enable vue/no-v-html -->
        </v-banner-text>
    </v-banner>
</template>
