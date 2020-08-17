<script lang="ts">
import Vue from "vue";
import { Component, Prop } from "vue-property-decorator";
import { ILogger } from "@/services/interfaces";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";

@Component
export default class RatingComponent extends Vue {
    private logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    @Prop({
        default:
            "Did the Health Gateway improve your access to health information today? Please provide a rating.",
        required: false,
    })
    question!: string;
    private ratingValue: number = 0;
    private isVisible: boolean = false;
    public showModal() {
        this.isVisible = true;
    }
    public hideModal() {
        this.isVisible = false;
    }
    private handleRating(value: number, skip: boolean = false) {
        this.logger.debug(
            `Emitting the selected rating value: ${value} to the modal's caller ...`
        );
        this.$emit("selected-rating", { ratingValue: value, skip: skip });
        this.hideModal();
    }
    private skip() {
        this.logger.debug(`clicked on skip button (value: 0)...`);
        this.handleRating(0, true);
    }
}
</script>

<template>
    <div>
        <b-modal
            id="rating-modal"
            ref="rating-modal"
            v-model="isVisible"
            hide-footer
            hide-header
            size="md sm"
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
            <b-row class="text-center row-skip">
                <b-col>
                    <b-button id="skipButton" @click="skip()">Skip</b-button>
                </b-col>
            </b-row>
        </b-modal>
    </div>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";
.modal-content {
    max-width: 500px;
    .modal-body {
        padding-bottom: 0px !important;
        font-family: Myriad-Pro, Calibri, Arial, sans serif !important;

        .row-answer {
            margin-top: 10px;
            padding-left: 50px;
            padding-right: 50px;
        }

        .row-skip {
            .btn {
                width: 100%;
                background-color: #f0f0f0;
                margin-top: 10px;
                border: none;
                color: #000;
            }
        }
    }
}
</style>