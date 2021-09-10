<script lang="ts">
import html2canvas from "html2canvas";
import Vue from "vue";
import { Component, Ref, Watch } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import LoadingComponent from "@/components/loading.vue";
import MessageModalComponent from "@/components/modal/genericMessage.vue";
import VaccineCardComponent from "@/components/vaccineCard.vue";
import EventBus, { EventMessageName } from "@/eventbus";
import type { WebClientConfiguration } from "@/models/configData";
import { DateWrapper } from "@/models/dateWrapper";
import { ImmunizationEvent } from "@/models/immunizationModel";
import PatientData from "@/models/patientData";
import User from "@/models/user";
import VaccinationStatus from "@/models/vaccinationStatus";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import { ILogger } from "@/services/interfaces";
import SnowPlow from "@/utility/snowPlow";

interface Dose {
    product: string;
    date: string;
    agent: string;
    lot: string;
    provider: string;
}

@Component({
    components: {
        "message-modal": MessageModalComponent,
        loading: LoadingComponent,
        "vaccine-card": VaccineCardComponent,
    },
})
export default class VaccineCardModalComponent extends Vue {
    @Action("getPatientData", { namespace: "user" })
    getPatientData!: () => Promise<void>;

    @Action("retrieve", { namespace: "immunization" })
    retrieveImmunizations!: (params: { hdid: string }) => Promise<void>;

    @Action("retrieveAuthenticatedVaccineStatus", {
        namespace: "vaccinationStatus",
    })
    retrieveVaccineStatus!: (params: { hdid: string }) => Promise<void>;

    @Getter("webClient", { namespace: "config" })
    config!: WebClientConfiguration;

    @Getter("isLoading", { namespace: "user" })
    isPatientLoading!: boolean;

    @Getter("isLoading", { namespace: "immunization" })
    isImmunizationLoading!: boolean;

    @Getter("patientData", { namespace: "user" })
    patientData!: PatientData;

    @Getter("user", { namespace: "user" })
    user!: User;

    @Getter("covidImmunizations", { namespace: "immunization" })
    covidImmunizations!: ImmunizationEvent[];

    @Getter("authenticatedVaccinationStatus", {
        namespace: "vaccinationStatus",
    })
    status!: VaccinationStatus | undefined;

    @Getter("authenticatedIsLoading", { namespace: "vaccinationStatus" })
    isVaccinationStatusLoading!: boolean;

    @Getter("authenticatedStatusMessage", { namespace: "vaccinationStatus" })
    statusMessage!: string;

    private eventBus = EventBus;

    private logger!: ILogger;
    private readonly modalId: string = "covid-card-modal";
    private isVisible = false;
    private isDownloading = false;

    private doses: Dose[] = [];

    private get isLoading(): boolean {
        return (
            this.isPatientLoading ||
            this.isImmunizationLoading ||
            this.isVaccinationStatusLoading
        );
    }

    private get downloadButtonShown(): boolean {
        return this.config.modules["VaccinationStatusPdf"];
    }

    @Ref("messageModal")
    readonly messageModal!: MessageModalComponent;

    @Ref("cardModal")
    readonly cardModal!: Vue;

    @Watch("covidImmunizations", { deep: true })
    private onImmunizationsChange() {
        this.doses = [];

        for (let index = 0; index < this.covidImmunizations.length; index++) {
            const element = this.covidImmunizations[index];
            const agent =
                this.covidImmunizations[index].immunization
                    .immunizationAgents[0];
            this.doses.push({
                product: agent.productName,
                date: DateWrapper.format(element.dateOfImmunization),
                agent: agent.name,
                lot: agent.lotNumber,
                provider: element.providerOrClinic,
            });
        }
    }

    private get userName(): string {
        return this.patientData.firstname + " " + this.patientData.lastname;
    }

    private get birthdate(): string {
        return this.formatDate(this.patientData.birthdate);
    }

    private get loadingStatusMessage(): string {
        return this.isDownloading ? "Downloading...." : this.statusMessage;
    }

    private created() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        this.eventBus.$on(EventMessageName.TimelineCovidCard, this.showModal);
        this.onImmunizationsChange();
    }

    public showModal(): void {
        this.getPatientData();
        this.retrieveImmunizations({ hdid: this.user.hdid });
        this.retrieveVaccineStatus({ hdid: this.user.hdid });
        this.isVisible = true;
    }

    public hideModal(): void {
        this.isVisible = false;
    }

    private formatDate(date: string | undefined): string {
        return date === undefined
            ? ""
            : DateWrapper.format(date, "yyyy-MMM-dd");
    }

    private showConfirmationModal() {
        this.messageModal.showModal();
    }

    private close() {
        this.$bvModal.hide("covidImmunizationCard");
    }

    private download() {
        const printingArea: HTMLElement | null =
            document.querySelector(".vaccine-card");

        if (printingArea !== null) {
            this.isDownloading = true;

            SnowPlow.trackEvent({
                action: "download_card",
                text: "COVID Card PDF",
            });

            html2canvas(printingArea, {
                scale: 2,
                ignoreElements: (element) =>
                    element.classList.contains("d-print-none"),
            })
                .then((canvas) => {
                    const dataUrl = canvas.toDataURL();
                    fetch(dataUrl).then((res) => {
                        res.blob().then((blob) => {
                            saveAs(blob, "BCVaccineCard.png");
                        });
                    });
                })
                .finally(() => {
                    this.isDownloading = false;
                });
        }
    }
}
</script>

<template>
    <b-modal
        id="covidImmunizationCard"
        ref="cardModal"
        v-model="isVisible"
        data-testid="covidImmunizationCard"
        header-text-variant="light"
        content-class="immunization-covid-card-modal-content"
        header-class="immunization-covid-card-modal-header d-print-none"
        footer-class="d-print-none"
        body-class="p-0"
        :no-close-on-backdrop="true"
        scrollable
        centered
        :hide-footer="!downloadButtonShown"
    >
        <template #modal-header="{ close }">
            <b-row class="w-100 h-100">
                <b-col class="image-container">
                    <img
                        class="img-fluid"
                        src="@/assets/images/gov/bcid-logo-rev-en.svg"
                        width="190px"
                        alt="BC Health Gateway"
                    />
                </b-col>
                <b-col cols="auto" class="align-self-center">
                    <!-- Emulate built in modal header close button action -->
                    <hg-button
                        class="close"
                        aria-label="Close"
                        variant="icon"
                        @click="close()"
                        >×</hg-button
                    >
                </b-col>
            </b-row>
        </template>
        <template v-if="downloadButtonShown" #modal-footer>
            <div class="w-100 d-flex justify-content-center">
                <hg-button
                    data-testid="exportCardBtn"
                    aria-label="Save a Copy"
                    variant="primary"
                    @click="showConfirmationModal()"
                >
                    Save a Copy
                </hg-button>
            </div>
        </template>
        <b-container fluid class="d-flex flex-column p-0">
            <loading
                :is-loading="isLoading || isDownloading"
                :text="loadingStatusMessage"
            />
            <vaccine-card
                :status="status"
                class="vaccine-card align-self-center w-100 p-3 rounded"
            />
            <div class="d-print-none p-3">
                <b-row class="mb-3 title">
                    <b-col class="ml-1 label col-4 d-flex justify-content-end">
                        Name
                    </b-col>
                    <b-col class="value text-wrap text-break">
                        {{ userName }}
                    </b-col>
                </b-row>
                <b-row class="mb-3 title">
                    <b-col class="ml-1 label col-4 d-flex justify-content-end">
                        Date of Birth
                    </b-col>
                    <b-col class="value" data-testid="patientBirthdate">
                        {{ birthdate }}
                    </b-col>
                </b-row>
                <b-row class="mb-3 title">
                    <b-col class="ml-1 label col-4 d-flex justify-content-end">
                        Immunization
                    </b-col>
                    <b-col class="value">COVID-19</b-col>
                </b-row>
                <b-row
                    v-for="(dose, index) in doses"
                    :key="index"
                    class="dose-wrapper mt-4"
                >
                    <b-col
                        class="
                            left-pane
                            text-center
                            justify-content-center
                            align-self-center
                        "
                    >
                        <span class="dose-label">Dose {{ index + 1 }}</span>
                    </b-col>
                    <b-col class="right-pane">
                        <b-row class="pb-2">
                            <b-col class="field">
                                <b-row class="value" align-h="between">
                                    <b-col cols="6">{{ dose.product }}</b-col>
                                    <b-col cols="4" data-testid="doseDate">
                                        {{ dose.date }}
                                    </b-col>
                                </b-row>
                                <b-row class="text-muted" align-h="between">
                                    <b-col class="description" cols="6">
                                        Product
                                    </b-col>
                                    <b-col class="description" cols="4">
                                        Date
                                    </b-col>
                                </b-row>
                            </b-col>
                        </b-row>
                        <b-row class="pb-2">
                            <b-col class="field">
                                <b-row class="value" align-h="between">
                                    <b-col cols="6">{{ dose.agent }}</b-col>
                                    <b-col cols="4">{{ dose.lot }}</b-col>
                                </b-row>
                                <b-row class="text-muted" align-h="between">
                                    <b-col class="description" cols="6">
                                        Immunizing agent
                                    </b-col>
                                    <b-col class="description" cols="4">
                                        Lot Number
                                    </b-col>
                                </b-row>
                            </b-col>
                        </b-row>
                        <b-row>
                            <b-col class="field" cols="10">
                                <b-row class="value" align-h="between">
                                    <b-col>{{ dose.provider }}</b-col>
                                </b-row>
                                <b-row class="text-muted" align-h="between"
                                    ><b-col class="description"
                                        >Provider or Clinic</b-col
                                    >
                                </b-row>
                            </b-col>
                        </b-row>
                    </b-col>
                </b-row>
            </div>
            <message-modal
                ref="messageModal"
                title="Sensitive Document Download"
                message="The file that you are downloading contains personal information. If you are on a public computer, please ensure that the file is deleted before you log off."
                @submit="download"
            />
        </b-container>
    </b-modal>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

$muted-color: #6c757d;

.vaccine-card {
    max-width: 438px;
    color-adjust: exact;
}

div[class^="col"],
div[class*=" col"] {
    padding: 0px;
    margin: 0px;
}

div[class^="row"],
div[class*=" row"] {
    padding: 0px;
    margin: 0px;
}

.title {
    color: $primary;

    .label {
        min-width: 75px;
        max-width: 90px;
        font-size: 0.9em;
    }
    .value {
        margin-left: 10px;
        border-bottom: 1px solid $muted-color;
        font-weight: bold;
    }
}

.dose-wrapper {
    background-color: $primary;
    border-radius: 15px 0px 0px 15px;

    .left-pane {
        max-width: 55px;
        padding: 5px;
        @media (max-width: 360px) {
            padding: 5px;
            max-width: 55px;
        }
        .dose-label {
            color: white;
            font-size: 1.2em;
            font-weight: bold;
        }
    }
    .right-pane {
        background-color: white;
        padding-left: 15px;

        .field {
            padding-top: 5px;
            padding-bottom: 5px;
            .value {
                min-height: 1em;
                color: $primary;
            }
            .description {
                border-top: 1px solid$muted-color;
            }
        }
    }
}
</style>

<style lang="scss">
@import "@/assets/scss/_variables.scss";
.immunization-covid-card-modal-header {
    background-color: $primary;
    .close {
        color: white;
        text-shadow: none;
        opacity: 1;
        font-size: 1.5em;
    }
    .image-container {
        min-height: 50px;
    }
}
.immunization-covid-card-modal-content {
    @media print {
        border: none;
    }
}
.modal-backdrop {
    @media print {
        display: none;
    }
}
.vld-overlay {
    .vld-background {
        opacity: 0.75;
    }

    .vld-icon {
        text-align: center;
    }
}
</style>
