<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";
.modal-content {
    max-width: 500px;
    .modal-body {
        padding-bottom: 0px !important;
        .row-answer {
            margin-top: 10px;
            padding-left: 50px;
            padding-right: 50px;
        }
    }
}
</style>
<template>
    <div>
        <b-modal
            id="rating-modal"
            ref="rating-modal"
            v-model="isVisible"
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
                <b-col>
                    {{ question }}
                </b-col>
            </b-row>
            <b-row class="text-center row-answer">
                <b-col>
                    <b-form-rating
                        v-model="ratingValue"
                        variant="warning"
                        class="mb-2"
                        no-border
                        size="lg"
                        @change="handleRating(ratingValue)"
                    ></b-form-rating>
                </b-col>
            </b-row>
            <template v-slot:modal-footer>
                <b-row>
                    <b-col>
                        <b-button
                            id="skipButton"
                            variant="outline-primary"
                            @click="skip(0, true)"
                            >Skip</b-button
                        >
                    </b-col>
                </b-row>
            </template>
        </b-modal>
    </div>
</template>
<script lang="ts">
import Vue from "vue";
import { Component, Prop } from "vue-property-decorator";
import { ILogger } from "@/services/interfaces";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";

@Component
export default class RatingComponent extends Vue {
    private logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    question: string =
        "Did the Health Gateway improve your access to health information today? Please provide a rating.";
    private ratingValue: number = 0;
    private isVisible: boolean = false;
    public showModal() {
        this.isVisible = true;
    }
    public hideModal() {
        this.isVisible = false;
    }
    private handleRating(value: number, skip: boolean = false) {
        // Todo: call rating vue service
        this.hideModal();
    }
}
</script>