<script setup lang="ts">
import { ref } from "vue";

import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { ILogger, IUserRatingService } from "@/services/interfaces";
import { useConfigStore } from "@/stores/config";
import SnowPlow from "@/utility/snowPlow";

const emit = defineEmits<{
    (e: "on-close"): void;
}>();

defineExpose({
    showDialog,
    hideDialog,
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

function showDialog(): void {
    isVisible.value = true;
    const autoSkipConfigValue = Number(
        configStore.webConfig?.timeouts.logoutRedirect
    );
    const autoSkip =
        isNaN(autoSkipConfigValue) || autoSkipConfigValue <= 0
            ? 10000
            : autoSkipConfigValue;
    logger.info(
        `autoSkipConfigValue: ${autoSkipConfigValue}, autoSkip: ${autoSkip}`
    );
    setTimeout(() => {
        if (isVisible.value) {
            handleRating(0, true);
        }
    }, autoSkip);
}

function hideDialog(): void {
    isVisible.value = false;
}

function handleRating(value: number | string, skip = false): void {
    logger.debug(
        `submitting rating: ratingValue = ${value}, skip = ${skip} ...`
    );
    ratingService
        .submitRating({ ratingValue: Number(value), skip })
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
            hideDialog();
            emit("on-close");
        });
}
</script>

<template>
    <v-dialog
        id="rating-modal"
        v-model="isVisible"
        data-modal="rating"
        data-testid="ratingModal"
        persistent
        no-click-animation
    >
        <div class="d-flex justify-center">
            <v-card max-width="600px">
                <v-card-title class="bg-primary text-white px-0">
                    <v-toolbar
                        title="Rating"
                        density="compact"
                        color="primary"
                    />
                </v-card-title>
                <v-card-text class="pa-4">
                    <p
                        class="text-body-1 text-center"
                        data-testid="ratingModalQuestionText"
                    >
                        {{ question }}
                    </p>
                    <div class="d-flex justify-center">
                        <v-rating
                            v-model="ratingValue"
                            data-testid="formRating"
                            color="orange"
                            hover
                            size="large"
                            @update:model-value="handleRating"
                        />
                    </div>
                </v-card-text>
                <v-card-actions class="justify-end border-t-sm pa-4">
                    <HgButtonComponent
                        id="skipButton"
                        data-testid="ratingModalSkipBtn"
                        variant="secondary"
                        text="Skip"
                        @click="handleRating(0, true)"
                    />
                </v-card-actions>
            </v-card>
        </div>
    </v-dialog>
</template>
