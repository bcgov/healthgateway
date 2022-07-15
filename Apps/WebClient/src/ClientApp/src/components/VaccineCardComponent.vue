<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import {
    faCheckCircle,
    faChevronLeft,
    faChevronRight,
    faHandPointer,
} from "@fortawesome/free-solid-svg-icons";
import Vue from "vue";
import { Component, Emit, Prop } from "vue-property-decorator";

import TooManyRequestsComponent from "@/components/TooManyRequestsComponent.vue";
import { VaccinationState } from "@/constants/vaccinationState";
import { DateWrapper } from "@/models/dateWrapper";
import { BannerError } from "@/models/errors";
import VaccinationStatus from "@/models/vaccinationStatus";

library.add(faCheckCircle, faChevronLeft, faChevronRight, faHandPointer);

@Component({
    components: {
        TooManyRequestsComponent,
    },
})
export default class VaccineCardComponent extends Vue {
    @Prop({ required: true }) status!: VaccinationStatus | undefined;
    @Prop({ required: false, default: undefined }) previousAction!:
        | (() => void)
        | undefined;
    @Prop({ required: false, default: undefined }) nextAction!:
        | (() => void)
        | undefined;
    @Prop({ required: true }) showGenericSaveInstructions!: boolean;
    @Prop({ required: false, default: false }) isLoading!: boolean;
    @Prop({ required: false, default: undefined }) error!:
        | BannerError
        | undefined;

    @Emit("clickPreviousButton")
    private onClickPreviousButton() {
        return;
    }

    @Emit("clickNextButton")
    private onClickNextButton() {
        return;
    }

    private get vaccinationState(): VaccinationState | undefined {
        return this.status?.state;
    }

    private get isFullyVaccinated(): boolean {
        return this.vaccinationState === VaccinationState.FullyVaccinated;
    }

    private get isPartiallyVaccinated(): boolean {
        return this.vaccinationState === VaccinationState.PartiallyVaccinated;
    }

    private get isVaccinationNotFound(): boolean {
        return this.vaccinationState === VaccinationState.NotFound;
    }

    private get name(): string {
        return [this.status?.firstname ?? "", this.status?.lastname ?? ""]
            .filter((name) => name?.length > 0)
            .join(" ");
    }

    private get qrCodeUrl(): string | null {
        const qrCode = this.status?.qrCode;
        if (qrCode?.mediaType && qrCode?.encoding && qrCode?.data) {
            return `data:${qrCode.mediaType};${qrCode.encoding},${qrCode.data}`;
        } else {
            return null;
        }
    }

    private get issuedDate(): string | undefined {
        if (this.status?.issueddate) {
            return new DateWrapper(this.status?.issueddate, {
                hasTime: true,
            }).format("MMMM-dd-yyyy, HH:mm");
        }
        return undefined;
    }

    private get includePreviousButton(): boolean {
        return this.$listeners.clickPreviousButton !== undefined;
    }

    private get includeNextButton(): boolean {
        return this.$listeners.clickNextButton !== undefined;
    }

    private handleCloseQrModal() {
        this.$bvModal.hide("big-qr");
    }
}
</script>

<template>
    <div>
        <div class="header text-white">
            <div class="p-3 pt-0 pt-sm-3">
                <h3 data-testid="formTitleVaccineCard" class="text-center">
                    BC Vaccine Card
                </h3>
                <hr style="border-top: 2px solid #fcba19" />
                <div class="text-center">{{ name }}</div>
            </div>
        </div>
        <b-row
            no-gutters
            class="vaccination-result"
            :class="{
                'fully-vaccinated': isFullyVaccinated,
                'partially-vaccinated': isPartiallyVaccinated,
                'not-found': isVaccinationNotFound,
            }"
        >
            <b-col
                v-if="includePreviousButton"
                cols="auto"
                class="d-print-none"
            >
                <hg-button
                    data-testid="vc-chevron-left-btn"
                    variant="carousel"
                    class="h-100"
                    @click="onClickNextButton"
                >
                    <hg-icon icon="chevron-left" size="large" />
                </hg-button>
            </b-col>
            <b-col>
                <div class="justify-content-between">
                    <div
                        class="p-3 flex-grow-1 d-flex align-items-center justify-content-center flex-column text-center text-white"
                    >
                        <h2
                            v-if="isFullyVaccinated"
                            aria-label="Status Vaccinated"
                            data-testid="statusVaccinated"
                            class="d-flex align-items-center m-0"
                        >
                            <hg-icon
                                v-show="isFullyVaccinated"
                                icon="check-circle"
                                size="extra-large"
                                class="mr-2"
                            />
                            <span>Vaccinated</span>
                        </h2>
                        <h2
                            v-else-if="isPartiallyVaccinated"
                            aria-label="Status Partially Vaccinated"
                            data-testid="statusPartiallyVaccinated"
                            class="m-0"
                        >
                            Partially Vaccinated
                        </h2>
                        <h2
                            v-else-if="isVaccinationNotFound"
                            aria-label="Status Not Found"
                            data-testid="statusNotFound"
                            class="m-0"
                        >
                            Not Found
                        </h2>
                        <small
                            v-if="
                                issuedDate !== undefined &&
                                (isFullyVaccinated || isPartiallyVaccinated)
                            "
                            class="mt-3"
                        >
                            Issued on {{ issuedDate }}
                        </small>
                        <div
                            v-if="qrCodeUrl !== null && !isVaccinationNotFound"
                            aria-label="QR Code Image"
                            class="qr-code-container text-center"
                        >
                            <img
                                v-b-modal.big-qr
                                :src="qrCodeUrl"
                                class="d-sm-none small-qr-code img-fluid m-2"
                                alt="BC Vaccine Card QR"
                            />
                            <img
                                :src="qrCodeUrl"
                                class="d-none d-sm-block small-qr-code img-fluid m-2"
                                alt="BC Vaccine Card QR"
                            />
                            <p
                                v-b-modal.big-qr
                                class="d-sm-none d-print-none m-0"
                            >
                                <hg-icon icon="hand-pointer" class="mr-2" />
                                <span>Tap to zoom in</span>
                            </p>
                            <b-modal
                                id="big-qr"
                                centered
                                title="Present for scanning"
                                title-class="flex-grow-1 text-center"
                                body-class="p-0"
                                footer-class="justify-content-center"
                            >
                                <img
                                    :src="qrCodeUrl"
                                    class="big-qr-code img-fluid"
                                    alt="BC Vaccine Card QR"
                                />
                                <template #modal-footer>
                                    <hg-button
                                        variant="secondary"
                                        aria-label="Close"
                                        @click="handleCloseQrModal()"
                                    >
                                        Close
                                    </hg-button>
                                </template>
                            </b-modal>
                        </div>
                    </div>
                </div>
            </b-col>
            <b-col v-if="includeNextButton" cols="auto" class="d-print-none">
                <hg-button
                    data-testid="vc-chevron-right-btn"
                    variant="carousel"
                    class="h-100"
                    @click="onClickNextButton"
                >
                    <hg-icon icon="chevron-right" size="large" />
                </hg-button>
            </b-col>
        </b-row>
        <TooManyRequestsComponent location="vaccineCardComponent" />
        <div v-if="error !== undefined" class="container d-print-none">
            <b-alert
                variant="danger"
                class="no-print my-3 p-3"
                :show="error !== undefined"
                dismissible
            >
                <h4>Our Apologies</h4>
                <div data-testid="errorTextDescription" class="pl-4">
                    We've found an issue and the Health Gateway team is working
                    hard to fix it.
                </div>
            </b-alert>
        </div>
    </div>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

.header {
    background-color: $hg-brand-primary;
    border-top-left-radius: 0.25rem;
    border-top-right-radius: 0.25rem;
}

.vaccination-result {
    min-height: 348px;

    &.fully-vaccinated {
        background-color: $hg-state-success;
    }
    &.partially-vaccinated {
        background-color: $hg-background-navigation;
    }
    &.not-found {
        background-color: #6c757d;
    }
}

img.vaccination-stage {
    height: 3em;
}

.big-qr-code {
    width: 100%;
}

.callout {
    padding: 1rem;
    border-left: 0.25rem solid $hg-brand-secondary;
    border-radius: 0.25rem;
    background-color: $hg-background;
    color: $hg-text-primary;
}
</style>
