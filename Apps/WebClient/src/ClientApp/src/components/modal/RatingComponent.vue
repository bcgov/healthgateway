<script lang="ts">
import Vue from "vue";
import { Component, Emit } from "vue-property-decorator";
import { Getter } from "vuex-class";

import type { WebClientConfiguration } from "@/models/configData";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger, IUserRatingService } from "@/services/interfaces";

@Component
export default class RatingComponent extends Vue {
    @Getter("webClient", { namespace: "config" })
    config!: WebClientConfiguration;

    private question =
        "Did the Health Gateway improve your access to health information today? Please provide a rating.";
    private ratingValue = 0;
    private isVisible = false;
    private logger!: ILogger;

    private created(): void {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
    }

    public showModal(): void {
        this.isVisible = true;
        setTimeout(() => {
            if (this.isVisible) {
                this.handleRating(0, true);
            }
        }, Number(this.config.timeouts.logoutRedirect));
    }

    public hideModal(): void {
        this.isVisible = false;
    }

    private handleRating(value: number, skip = false): void {
        const ratingService = container.get<IUserRatingService>(
            SERVICE_IDENTIFIER.UserRatingService
        );
        this.logger.debug(
            `submitting rating: ratingValue = ${value}, skip = ${skip} ...`
        );
        ratingService
            .submitRating({ ratingValue: value, skip })
            .then(() => this.logger.debug(`submitRating with success.`))
            .catch((err) =>
                this.logger.error(`submitRating with error: ${err}`)
            )
            .finally(() => {
                this.hideModal();
                this.onClose();
            });
    }

    @Emit()
    private onClose(): void {
        return;
    }
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
