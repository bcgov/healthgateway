<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import {
    faCheckCircle,
    faEye,
    faEyeSlash,
    faHandPointer,
} from "@fortawesome/free-solid-svg-icons";
import { saveAs } from "file-saver";
import Vue from "vue";
import { Component, Ref } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import LoadingComponent from "@/components/loading.vue";
import MessageModalComponent from "@/components/modal/genericMessage.vue";
import { VaccinationState } from "@/constants/vaccinationState";
import BannerError from "@/models/bannerError";
import { DateWrapper, StringISODate } from "@/models/dateWrapper";
import Report from "@/models/report";
import VaccinationStatus from "@/models/vaccinationStatus";
import { ILogger } from "@/services/interfaces";

library.add(faCheckCircle, faEye, faEyeSlash, faHandPointer);

@Component({
    components: {
        LoadingComponent,
        MessageModalComponent,
    },
})
export default class VaccinationStatusResultView extends Vue {
    private isDownloading = false;
    private showDetails = false;
    private logger!: ILogger;

    @Getter("vaccinationStatus", { namespace: "vaccinationStatus" }) status!:
        | VaccinationStatus
        | undefined;

    @Getter("error", { namespace: "vaccinationStatus" })
    error!: BannerError | undefined;

    @Action("getReport", { namespace: "vaccinationStatus" })
    getReport!: (params: {
        phn: string;
        dateOfBirth: StringISODate;
    }) => Promise<Report>;

    @Ref("sensitivedocumentDownloadModal")
    readonly sensitivedocumentDownloadModal!: MessageModalComponent;

    private get vaccinationState(): VaccinationState | undefined {
        return this.status?.state;
    }

    private get vaccinationDoses(): number | undefined {
        return this.status?.doses;
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

    private get dateOfBirth(): string {
        return this.formatDate(this.status?.birthdate ?? null);
    }

    private get qrCodeUrl(): string | null {
        const qrCode = this.status?.qrCode;
        if (qrCode?.mediaType && qrCode?.encoding && qrCode?.data) {
            return `data:${qrCode.mediaType};${qrCode.encoding},${qrCode.data}`;
        } else {
            return null;
        }
    }

    private toggleDetails() {
        this.showDetails = !this.showDetails;
    }

    private formatDate(date: StringISODate | null): string {
        if (!date) {
            return "";
        }
        return new DateWrapper(date).format(DateWrapper.defaultFormat);
    }

    private showSensitiveDocumentDownloadModal() {
        this.sensitivedocumentDownloadModal.showModal();
    }

    private download() {
        this.isDownloading = true;
        this.getReport({
            phn: this.status?.personalhealthnumber || "",
            dateOfBirth: new DateWrapper(this.status?.birthdate || "").format(
                "yyyy-MM-dd"
            ),
        })
            .then((documentResult) => {
                if (documentResult) {
                    const downloadLink = `data:application/pdf;base64,${documentResult.data}`;
                    fetch(downloadLink).then((res) => {
                        res.blob().then((blob) => {
                            saveAs(blob, documentResult.fileName);
                        });
                    });
                }
            })
            .catch((err) => {
                this.logger.error(
                    `Error retrieving vaccination status pdf: ${err}`
                );
            })
            .finally(() => {
                this.isDownloading = false;
            });
    }

    private handleCloseQrModal() {
        this.$bvModal.hide("big-qr");
    }
}
</script>

<template>
    <div
        class="
            vaccination-card
            align-self-center
            flex-grow-1
            w-100
            d-flex
            flex-column
            p-3
            rounded
        "
    >
        <LoadingComponent :is-loading="isDownloading"></LoadingComponent>
        <div class="header text-white">
            <div class="container p-3 pt-0 pt-sm-3">
                <h3 class="text-center">BC Vaccine Card</h3>
                <hr style="border-top: 2px solid #fcba19" />
                <p class="text-center">{{ name }}</p>
                <div class="text-center">
                    <img
                        v-if="isFullyVaccinated"
                        src="@/assets/images/vaccination-status/fully-vaccinated.svg"
                        alt="Fully Vaccinated"
                        class="vaccination-stage"
                    />
                    <img
                        v-else-if="
                            isPartiallyVaccinated && vaccinationDoses === 2
                        "
                        src="@/assets/images/vaccination-status/dose-2.svg"
                        alt="Two Doses"
                        class="vaccination-stage"
                    />
                    <img
                        v-else-if="
                            isPartiallyVaccinated && vaccinationDoses === 1
                        "
                        src="@/assets/images/vaccination-status/dose-1.svg"
                        alt="One Dose"
                        class="vaccination-stage"
                    />
                </div>
            </div>
        </div>
        <div class="justify-content-between">
            <div
                class="
                    vaccination-result
                    p-3
                    flex-grow-1
                    d-flex
                    align-items-center
                    justify-content-center
                    flex-column
                    text-center text-white
                "
                :class="{
                    'fully-vaccinated': isFullyVaccinated,
                    'partially-vaccinated': isPartiallyVaccinated,
                    'not-found': isVaccinationNotFound,
                }"
            >
                <h2 v-if="isFullyVaccinated" class="d-flex align-items-center">
                    <hg-icon
                        v-show="isFullyVaccinated"
                        icon="check-circle"
                        size="extra-large"
                        class="mr-2"
                    />
                    <span>Vaccinated</span>
                </h2>
                <h2 v-else-if="isPartiallyVaccinated">Partially Vaccinated</h2>
                <h2 v-else-if="isVaccinationNotFound">Not Found</h2>
                <div
                    v-if="qrCodeUrl !== null && !isVaccinationNotFound"
                    class="text-center"
                >
                    <img
                        v-b-modal.big-qr
                        :src="qrCodeUrl"
                        class="d-sm-none small-qr-code img-fluid m-3"
                    />
                    <img
                        :src="qrCodeUrl"
                        class="d-none d-sm-block small-qr-code img-fluid m-3"
                    />
                    <p v-b-modal.big-qr class="d-sm-none m-0">
                        <hg-icon icon="hand-pointer" class="mr-2" />
                        <span>Tap to zoom in</span>
                    </p>
                    <b-modal
                        id="big-qr"
                        centered
                        title="Have it ready to scan"
                        title-class="flex-grow-1 text-center"
                        body-class="p-0"
                        footer-class="justify-content-center"
                    >
                        <img :src="qrCodeUrl" class="big-qr-code img-fluid" />
                        <template #modal-footer>
                            <hg-button
                                variant="secondary"
                                @click="handleCloseQrModal()"
                            >
                                Close
                            </hg-button>
                        </template>
                    </b-modal>
                </div>
            </div>
        </div>
        <div class="bg-white">
            <div
                v-if="isPartiallyVaccinated || isVaccinationNotFound"
                class="callout"
            >
                <p>You're fully vaccinated 7 days after dose 2.</p>
                <ul class="m-0">
                    <li>
                        <a
                            href="https://www2.gov.bc.ca/gov/content/covid-19/vaccine/proof#help"
                            rel="noopener"
                            target="_blank"
                        >
                            There's a mistake with my record
                        </a>
                    </li>
                    <li v-if="isVaccinationNotFound">
                        <a
                            href="https://www2.gov.bc.ca/gov/content/covid-19/vaccine/register"
                            rel="noopener"
                            target="_blank"
                        >
                            I want to get vaccinated
                        </a>
                    </li>
                    <li v-if="isPartiallyVaccinated">
                        <a
                            href="https://www2.gov.bc.ca/gov/content/covid-19/vaccine/register"
                            rel="noopener"
                            target="_blank"
                        >
                            Get dose 2
                        </a>
                    </li>
                </ul>
            </div>
            <div v-if="error !== undefined" class="container">
                <b-alert
                    variant="danger"
                    class="no-print my-3"
                    :show="error !== undefined"
                    dismissible
                >
                    <h4>{{ error.title }}</h4>
                    <h6>{{ error.errorCode }}</h6>
                    <div class="pl-4">
                        <p data-testid="errorTextDescription">
                            {{ error.description }}
                        </p>
                        <p data-testid="errorTextDetails">
                            {{ error.detail }}
                        </p>
                        <p
                            v-if="error.traceId"
                            data-testid="errorSupportDetails"
                        >
                            If this issue persists, contact
                            HealthGateway@gov.bc.ca and provide
                            <span class="trace-id">{{ error.traceId }}</span>
                        </p>
                    </div>
                </b-alert>
            </div>
        </div>
        <div class="actions p-3 d-flex justify-content-between bg-white">
            <hg-button variant="secondary" to="/">Done</hg-button>
            <hg-button
                v-if="isPartiallyVaccinated || isFullyVaccinated"
                variant="primary"
                class="ml-3"
                @click="showSensitiveDocumentDownloadModal()"
            >
                Save a Copy
            </hg-button>
        </div>
        <MessageModalComponent
            ref="sensitivedocumentDownloadModal"
            title="Sensitive Document Download"
            message="The file that you are downloading contains personal information. If you are on a public computer, please ensure that the file is deleted before you log off."
            @submit="download"
        />
    </div>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

.vaccination-card {
    max-width: 470px;
}

.header {
    background-color: $hg-brand-primary;
    border-top-left-radius: 0.25rem;
    border-top-right-radius: 0.25rem;
}

.vaccination-result {
    &.fully-vaccinated {
        background-color: $hg-state-success;
    }
    &.partially-vaccinated {
        background-color: $hg-background-navigation;
    }
    &.not-found {
        background-color: #6c757d;
        min-height: 335px;
    }
}

img.vaccination-stage {
    height: 3em;
}

.small-qr-code {
    width: 230px;
}

.big-qr-code {
    width: 100%;
}

.callout {
    margin: 1rem;
    padding: 1rem;
    border-left: 0.25rem solid $hg-brand-secondary;
    border-radius: 0.25rem;
    background-color: $hg-background;
    color: $hg-text-primary;
}

.actions {
    border-bottom-left-radius: 0.25rem;
    border-bottom-right-radius: 0.25rem;
}
</style>
