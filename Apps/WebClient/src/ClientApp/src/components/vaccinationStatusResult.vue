<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import {
    faCheckCircle,
    faEye,
    faEyeSlash,
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

library.add(faCheckCircle, faEye, faEyeSlash);

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
}
</script>

<template>
    <div class="d-flex flex-column flex-grow-1">
        <LoadingComponent :is-loading="isDownloading"></LoadingComponent>
        <div class="header text-white">
            <div class="container pb-3">
                <h3 class="text-center">COVID-19 Vaccination Check</h3>
                <hr style="border-top: 2px solid #fcba19" />
                <div v-if="name.length > 0">
                    <div class="mb-2 text-center">
                        <hg-button variant="secondary" @click="toggleDetails">
                            <span v-if="showDetails">Hide Details</span>
                            <span v-else>Show Details</span>
                            <hg-icon
                                :icon="showDetails ? 'eye-slash' : 'eye'"
                                class="ml-2"
                                square
                            />
                        </hg-button>
                    </div>
                    <b-row>
                        <b-col>
                            <b-form-group label="Name" label-for="name">
                                <b-form-input
                                    id="name"
                                    v-model="name"
                                    disabled
                                />
                            </b-form-group>
                        </b-col>
                        <b-col v-show="showDetails" cols="12" md="3">
                            <b-form-group
                                label="Date of Birth"
                                label-for="dateOfBirth"
                            >
                                <b-form-input
                                    id="dateOfBirth"
                                    v-model="dateOfBirth"
                                    disabled
                                />
                            </b-form-group>
                        </b-col>
                    </b-row>
                </div>
                <b-row v-show="showDetails" class="justify-content-center">
                    <b-col cols="auto">
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
                        <img
                            v-else
                            src="@/assets/images/vaccination-status/no-doses.svg"
                            alt="No Doses"
                            class="vaccination-stage"
                        />
                    </b-col>
                </b-row>
            </div>
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
                    <p v-if="error.traceId" data-testid="errorSupportDetails">
                        If this issue persists, contact HealthGateway@gov.bc.ca
                        and provide
                        <span class="trace-id">{{ error.traceId }}</span>
                    </p>
                </div>
            </b-alert>
        </div>
        <div
            class="
                vaccination-indicator
                p-3
                text-white
                flex-grow-1
                d-flex
                align-items-center
                justify-content-between
                flex-column
            "
            :class="{
                'fully-vaccinated': isFullyVaccinated,
                'partially-vaccinated': isPartiallyVaccinated,
                'not-found': isVaccinationNotFound,
            }"
        >
            <div class="vaccination-box m-4 p-5 d-inline-block text-center">
                <hg-icon
                    v-show="isFullyVaccinated"
                    icon="check-circle"
                    class="mb-2"
                />
                <h1 v-if="isFullyVaccinated">Vaccinated</h1>
                <h1 v-else-if="isPartiallyVaccinated">
                    <div>Partially</div>
                    <div>Vaccinated</div>
                </h1>
                <h1 v-else-if="isVaccinationNotFound">Not Found</h1>
            </div>
            <div>
                <hg-button variant="secondary" to="/">Done</hg-button>
                <hg-button
                    v-if="isPartiallyVaccinated || isFullyVaccinated"
                    variant="primary"
                    class="ml-3"
                    @click="showSensitiveDocumentDownloadModal()"
                >
                    Save for Later
                </hg-button>
            </div>
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

.header {
    background-color: $hg-brand-primary;
}

.vaccination-indicator {
    &.fully-vaccinated {
        background-color: $hg-state-success;
        .vaccination-box {
            border: 8px solid white;

            .hg-icon {
                font-size: 2.5rem;
            }
        }
    }
    &.partially-vaccinated {
        background-color: $hg-background-navigation;
        .vaccination-box {
            border: 5px dashed white;
        }
    }
    &.not-found {
        background-color: #6c757d;
        .vaccination-box {
            border: 5px dashed white;
        }
    }
}

img.vaccination-stage {
    height: 4.5em;
}
</style>
