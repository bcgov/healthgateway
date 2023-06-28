<script setup lang="ts">
import { computed, ref } from "vue";

import type { WebClientConfiguration } from "@/models/configData";
import { ILogger, IUserRatingService } from "@/services/interfaces";
import SnowPlow from "@/utility/snowPlow";
import HgButtonComponent from "@/components/shared/HgButtonComponent.vue";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { useConfigStore } from "@/stores/config";

const emit = defineEmits<{
    (e: "on-close"): void;
}>();

defineExpose({
    showModal,
    hideModal,
});

const question =
    "Did the Health Gateway improve your access to health information today? Please provide a rating.";

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const ratingService = container.get<IUserRatingService>(
    SERVICE_IDENTIFIER.UserRatingService
);

const configStore = useConfigStore();

const ratingValue = ref(0);
const isVisible = ref(false);

function showModal(): void {
    isVisible.value = true;
    setTimeout(() => {
        if (isVisible.value) {
            handleRating(0, true);
        }
    }, Number(configStore.webConfig.timeouts.logoutRedirect));
}

function hideModal(): void {
    isVisible.value = false;
}

function handleRating(value: number, skip = false): void {
    logger.debug(
        `submitting rating: ratingValue = ${value}, skip = ${skip} ...`
    );
    ratingService
        .submitRating({ ratingValue: value, skip })
        .then(() => {
            if (skip) {
                SnowPlow.trackEvent({
                    action: "submit_app_rating",
                    text: "skip",
                });
            } else {
                SnowPlow.trackEvent({
                    action: "submit_app_rating",
                    text: value.toString(),
                });
            }
            logger.debug(`submitRating with success.`);
        })
        .catch((err: unknown) =>
            logger.error(`submitRating with error: ${err}`)
        )
        .finally(() => {
            hideModal();
            emit("on-close");
        });
}
</script>

<template>
    <div>
        <v-dialog
            id="rating-modal"
            ref="rating-modal"
            v-model="isVisible"
            data-modal="rating"
            data-testid="ratingModal"
            persistent
            no-click-animation
        >
            <v-card>
                <template #title>
                    <h1 class="text-h6 font-weight-bold text-center">Rating</h1>
                </template>
                <v-row class="text-center">
                    <v-col data-testid="ratingModalQuestionText">
                        {{ question }}
                    </v-col>
                </v-row>
                <v-row class="text-center px-2 pt-3">
                    <v-col>
                        <v-rating
                            v-model="ratingValue"
                            data-testid="formRating"
                            color="orange"
                            class="mb-2"
                            no-border
                            size="large"
                            @change="handleRating(ratingValue)"
                        ></v-rating>
                    </v-col>
                </v-row>
                <template #actions>
                    <v-row>
                        <v-col>
                            <HgButtonComponent
                                id="skipButton"
                                data-testid="ratingModalSkipBtn"
                                variant="secondary"
                                @click="handleRating(0, true)"
                                text="Skip"
                            />
                        </v-col>
                    </v-row>
                </template>
            </v-card>
        </v-dialog>
    </div>
</template>
