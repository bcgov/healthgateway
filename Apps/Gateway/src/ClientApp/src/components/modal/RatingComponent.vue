<script setup lang="ts">
import { computed, ref } from "vue";
import { useStore } from "vue-composition-wrapper";

import type { WebClientConfiguration } from "@/models/configData";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger, IUserRatingService } from "@/services/interfaces";
import SnowPlow from "@/utility/snowPlow";

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
const store = useStore();

const ratingValue = ref(0);
const isVisible = ref(false);

const config = computed<WebClientConfiguration>(
    () => store.getters["config/webClient"]
);

function showModal(): void {
    isVisible.value = true;
    setTimeout(() => {
        if (isVisible.value) {
            handleRating(0, true);
        }
    }, Number(config.value.timeouts.logoutRedirect));
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
        .catch((err) => logger.error(`submitRating with error: ${err}`))
        .finally(() => {
            hideModal();
            emit("on-close");
        });
}
</script>

<template>
    <div>
        <b-modal
            id="rating-modal"
            ref="rating-modal"
            v-model="isVisible"
            data-modal="rating"
            data-testid="ratingModal"
            title="Rating"
            size="md"
            header-bg-variant="primary"
            header-text-variant="light"
            footer-class="modal-footer"
            no-close-on-backdrop
            hide-header-close
            no-close-on-esc
            centered
        >
            <b-row class="text-center">
                <b-col data-testid="ratingModalQuestionText">
                    {{ question }}
                </b-col>
            </b-row>
            <b-row class="text-center px-2 pt-3">
                <b-col>
                    <b-form-rating
                        v-model="ratingValue"
                        data-testid="formRating"
                        variant="warning"
                        class="mb-2"
                        no-border
                        size="lg"
                        @change="handleRating(ratingValue)"
                    ></b-form-rating>
                </b-col>
            </b-row>
            <template #modal-footer>
                <b-row>
                    <b-col>
                        <hg-button
                            id="skipButton"
                            data-testid="ratingModalSkipBtn"
                            variant="secondary"
                            @click="handleRating(0, true)"
                            >Skip</hg-button
                        >
                    </b-col>
                </b-row>
            </template>
        </b-modal>
    </div>
</template>
<style lang="scss">
@import "@/assets/scss/_variables.scss";

[data-modal="rating"] {
    z-index: $z_application_rating !important;
}
</style>
