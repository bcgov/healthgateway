<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import {
    faCheckCircle,
    faChevronLeft,
    faChevronRight,
} from "@fortawesome/free-solid-svg-icons";
import { saveAs } from "file-saver";
import html2canvas from "html2canvas";
import Vue from "vue";
import { Component, Ref, Watch } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import LoadingComponent from "@/components/LoadingComponent.vue";
import MessageModalComponent from "@/components/modal/MessageModalComponent.vue";
import BreadcrumbComponent from "@/components/navmenu/BreadcrumbComponent.vue";
import VaccineCardComponent from "@/components/VaccineCardComponent.vue";
import { VaccinationState } from "@/constants/vaccinationState";
import BreadcrumbItem from "@/models/breadcrumbItem";
import type { WebClientConfiguration } from "@/models/configData";
import CovidVaccineRecord from "@/models/covidVaccineRecord";
import { DateWrapper } from "@/models/dateWrapper";
import { ResultError } from "@/models/errors";
import { ImmunizationEvent } from "@/models/immunizationModel";
import { LoadStatus } from "@/models/storeOperations";
import TimelineEntry from "@/models/timelineEntry";
import User from "@/models/user";
import VaccinationRecord from "@/models/vaccinationRecord";
import VaccinationStatus from "@/models/vaccinationStatus";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger } from "@/services/interfaces";
import SnowPlow from "@/utility/snowPlow";

library.add(faCheckCircle, faChevronLeft, faChevronRight);

interface Dose {
    product: string;
    date: string;
    agent: string;
    lot: string;
    provider: string;
}

@Component({
    components: {
        BreadcrumbComponent,
        loading: LoadingComponent,
        "vaccine-card": VaccineCardComponent,
        "message-modal": MessageModalComponent,
    },
})
export default class Covid19View extends Vue {
    @Action("retrieve", { namespace: "immunization" })
    retrieveImmunizations!: (params: { hdid: string }) => Promise<void>;

    @Action("retrieveAuthenticatedVaccineStatus", {
        namespace: "vaccinationStatus",
    })
    retrieveVaccineStatus!: (params: { hdid: string }) => Promise<void>;

    @Action("retrieveAuthenticatedVaccineRecord", {
        namespace: "vaccinationStatus",
    })
    retrieveAuthenticatedVaccineRecord!: (params: {
        hdid: string;
    }) => Promise<CovidVaccineRecord>;

    @Action("stopAuthenticatedVaccineRecordDownload", {
        namespace: "vaccinationStatus",
    })
    stopAuthenticatedVaccineRecordDownload!: (params: { hdid: string }) => void;

    @Getter("webClient", { namespace: "config" })
    config!: WebClientConfiguration;

    @Getter("user", { namespace: "user" })
    user!: User;

    @Getter("authenticatedIsLoading", { namespace: "vaccinationStatus" })
    isVaccinationStatusLoading!: boolean;

    @Getter("authenticatedVaccinationStatus", {
        namespace: "vaccinationStatus",
    })
    vaccinationStatus!: VaccinationStatus | undefined;

    @Getter("authenticatedError", { namespace: "vaccinationStatus" })
    vaccinationStatusError!: ResultError | undefined;

    @Getter("isLoading", { namespace: "immunization" })
    isImmunizationLoading!: boolean;

    @Getter("isDeferredLoad", { namespace: "immunization" })
    immunizationIsDeferred!: boolean;

    @Getter("covidImmunizations", { namespace: "immunization" })
    covidImmunizations!: ImmunizationEvent[];

    @Getter("error", { namespace: "immunization" })
    immunizationError!: ResultError | undefined;

    @Getter("authenticatedVaccineRecords", { namespace: "vaccinationStatus" })
    vaccineRecords!: Map<string, VaccinationRecord>;

    @Getter("authenticatedVaccineRecordStatusChanges", {
        namespace: "vaccinationStatus",
    })
    vaccineRecordStatusChanges!: number;

    @Ref("vaccineCardMessageModal")
    readonly vaccineCardMessageModal!: MessageModalComponent;

    @Ref("messageModal")
    readonly messageModal!: MessageModalComponent;

    private logger!: ILogger;
    private isDownloading = false;
    private isImmunizationHistoryShown = false;

    private breadcrumbItems: BreadcrumbItem[] = [
        {
            text: "COVID-19",
            to: "/covid19",
            active: true,
            dataTestId: "breadcrumb-covid-19",
        },
    ];

    private get doses(): Dose[] {
        return this.covidImmunizations.map((element) => {
            const agent = element.immunization.immunizationAgents[0];
            return {
                product: agent.productName,
                date: DateWrapper.format(element.dateOfImmunization),
                agent: agent.name,
                lot: agent.lotNumber,
                provider: element.providerOrClinic,
            };
        });
    }

    private get vaccinationState(): VaccinationState | undefined {
        return this.vaccinationStatus?.state;
    }

    private get isPartiallyVaccinated(): boolean {
        return this.vaccinationState === VaccinationState.PartiallyVaccinated;
    }

    private get isVaccineRecordDownloading(): boolean {
        if (this.vaccineRecordStatusChanges > 0) {
            const vaccinationRecord: VaccinationRecord | undefined =
                this.getVaccinationRecord();
            if (vaccinationRecord !== undefined) {
                return vaccinationRecord.status === LoadStatus.REQUESTED;
            }
        }
        return false;
    }

    private get isVaccinationNotFound(): boolean {
        return this.vaccinationState === VaccinationState.NotFound;
    }

    private get loadingStatusMessage(): string {
        if (this.isDownloading) {
            return "Downloading....";
        }

        const vaccinationRecord: VaccinationRecord | undefined =
            this.getVaccinationRecord();

        if (
            this.isVaccineRecordDownloading &&
            vaccinationRecord !== undefined
        ) {
            return vaccinationRecord.statusMessage;
        }

        return "";
    }

    private get downloadButtonShown(): boolean {
        return (
            this.config.modules["VaccinationStatusPdf"] &&
            (this.vaccinationStatus?.state ===
                VaccinationState.PartiallyVaccinated ||
                this.vaccinationStatus?.state ===
                    VaccinationState.FullyVaccinated)
        );
    }

    private get saveExportPdfShown(): boolean {
        return this.config.modules["VaccinationExportPdf"];
    }

    private get isVaccineCardLoading(): boolean {
        return this.isVaccinationStatusLoading;
    }

    private get isHistoryLoading(): boolean {
        return this.isImmunizationLoading || this.immunizationIsDeferred;
    }

    private get isLoading(): boolean {
        return (
            this.isVaccinationStatusLoading ||
            this.isHistoryLoading ||
            this.isVaccineRecordDownloading ||
            this.isDownloading
        );
    }

    private get patientName(): string | undefined {
        if (this.vaccinationStatus) {
            return `${this.vaccinationStatus.firstname} ${this.vaccinationStatus.lastname}`;
        }
        return undefined;
    }

    private get patientBirthdate(): string {
        return this.formatDate(this.vaccinationStatus?.birthdate ?? undefined);
    }

    private formatDate(date: string | undefined): string {
        return date === undefined
            ? ""
            : DateWrapper.format(date, "yyyy-MMM-dd");
    }

    private showImmunizationHistory(show: boolean): void {
        if (show) {
            this.fetchHistoryData();
        }
        this.isImmunizationHistoryShown = show;
    }

    private sortEntries(timelineEntries: TimelineEntry[]): TimelineEntry[] {
        return timelineEntries.sort((a, b) => {
            if (a.date.isBefore(b.date)) {
                return 1;
            }
            if (a.date.isAfter(b.date)) {
                return -1;
            }
            return 0;
        });
    }

    private fetchVaccineCardData(): void {
        this.retrieveVaccineStatus({ hdid: this.user.hdid })
            .then(() =>
                SnowPlow.trackEvent({
                    action: "view_card",
                    text: "COVID Card",
                })
            )
            .catch((err) =>
                this.logger.error(`Error loading COVID-19 data: ${err}`)
            );
    }

    private fetchHistoryData(): void {
        this.retrieveImmunizations({ hdid: this.user.hdid }).catch((err) =>
            this.logger.error(`Error loading immunization data: ${err}`)
        );
    }

    private getVaccinationRecord(): VaccinationRecord | undefined {
        return this.vaccineRecords.get(this.user.hdid);
    }

    @Watch("vaccineRecordStatusChanges")
    private saveVaccinePdf(): void {
        this.logger.info(`Downloading PDF for hdid: ${this.user.hdid}`);
        const vaccinationRecord: VaccinationRecord | undefined =
            this.getVaccinationRecord();
        if (
            vaccinationRecord !== undefined &&
            vaccinationRecord.hdid === this.user.hdid &&
            vaccinationRecord.status === LoadStatus.LOADED &&
            vaccinationRecord.download
        ) {
            const mimeType = vaccinationRecord.record.document.mediaType;
            const downloadLink = `data:${mimeType};base64,${vaccinationRecord.record.document.data}`;
            fetch(downloadLink).then((res) => {
                SnowPlow.trackEvent({
                    action: "download_card",
                    text: "COVID Card PDF",
                });
                res.blob().then((blob) =>
                    saveAs(blob, "ProvincialVaccineProof.pdf")
                );
            });
            this.stopAuthenticatedVaccineRecordDownload({
                hdid: this.user.hdid,
            });
        }
    }

    private retrieveVaccinePdf(): void {
        this.retrieveAuthenticatedVaccineRecord({
            hdid: this.user.hdid,
        }).catch((err) =>
            this.logger.error(`Error loading authenticated record data: ${err}`)
        );
    }

    private download(): void {
        const printingArea =
            document.querySelector<HTMLElement>(".vaccine-card");
        if (printingArea !== null) {
            this.isDownloading = true;
            SnowPlow.trackEvent({
                action: "download_card",
                text: "COVID Card Image",
            });
            html2canvas(printingArea, {
                scale: 2,
                ignoreElements: (element) =>
                    element.classList.contains("d-print-none"),
            })
                .then((canvas) => {
                    const dataUrl = canvas.toDataURL();
                    fetch(dataUrl).then((res) =>
                        res
                            .blob()
                            .then((blob) =>
                                saveAs(blob, "ProvincialVaccineProof.png")
                            )
                    );
                })
                .finally(() => {
                    this.isDownloading = false;
                });
        }
    }

    private showVaccineCardMessageModal(): void {
        this.vaccineCardMessageModal.showModal();
    }

    private showConfirmationModal(): void {
        this.messageModal.showModal();
    }

    private created(): void {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        this.fetchVaccineCardData();
    }
}
</script>

<template>
    <div class="background flex-grow-1 d-flex flex-column">
        <BreadcrumbComponent :items="breadcrumbItems" />
        <loading :is-loading="isLoading" :text="loadingStatusMessage" />
        <div
            v-if="!isVaccineCardLoading && !vaccinationStatusError"
            v-show="!isImmunizationHistoryShown"
            class="vaccine-card align-self-center w-100 p-3"
        >
            <div class="bg-white rounded shadow">
                <vaccine-card
                    :status="vaccinationStatus"
                    :show-generic-save-instructions="!downloadButtonShown"
                    @clickPreviousButton="showImmunizationHistory(true)"
                    @clickNextButton="showImmunizationHistory(true)"
                />
                <div
                    v-if="downloadButtonShown"
                    class="actions p-3 d-flex d-print-none justify-content-center"
                >
                    <hg-button
                        v-if="!saveExportPdfShown"
                        data-testid="save-card-btn"
                        aria-label="Save a Copy"
                        variant="primary"
                        @click="showVaccineCardMessageModal()"
                    >
                        Save a Copy
                    </hg-button>
                    <hg-dropdown
                        v-if="saveExportPdfShown"
                        text="Save as"
                        variant="primary"
                        data-testid="save-dropdown-btn"
                    >
                        <b-dropdown-item
                            data-testid="save-as-pdf-dropdown-item"
                            @click="showConfirmationModal()"
                            >PDF</b-dropdown-item
                        >
                        <b-dropdown-item
                            data-testid="save-as-image-dropdown-item"
                            @click="showVaccineCardMessageModal()"
                            >Image</b-dropdown-item
                        >
                    </hg-dropdown>
                </div>
                <div
                    v-if="isPartiallyVaccinated || isVaccinationNotFound"
                    class="d-print-none px-3 pb-3"
                    :class="{ 'pt-3': !downloadButtonShown }"
                >
                    <div class="callout">
                        <p class="m-0">
                            To learn more, visit
                            <a
                                href="https://www2.gov.bc.ca/gov/content/covid-19/vaccine/proof"
                                rel="noopener"
                                target="_blank"
                                >BC Proof of Vaccination</a
                            >.
                        </p>
                    </div>
                </div>
            </div>
            <message-modal
                ref="vaccineCardMessageModal"
                title="Vaccine Card Download"
                message="Next, you'll see an image of your card.
                                Depending on your browser, you may need to
                                manually save the image to your files or photos.
                                If you want to print, we recommend you use the print function in
                                your browser."
                @submit="download"
            />
            <message-modal
                ref="messageModal"
                title="Sensitive Document Download"
                message="The file that you are downloading contains personal information. If you are on a public computer, please ensure that the file is deleted before you log off."
                @submit="retrieveVaccinePdf"
            />
        </div>
        <div
            v-if="!isHistoryLoading && !immunizationError"
            v-show="isImmunizationHistoryShown"
            class="immunization-history flex-grow-1 align-self-center w-100 p-3"
        >
            <div class="flex-grow-1 bg-white rounded shadow">
                <b-row no-gutters>
                    <b-col cols="auto" class="d-print-none">
                        <hg-button
                            data-testid="vr-chevron-left-btn"
                            variant="carousel"
                            class="primary rounded-left h-100"
                            @click="showImmunizationHistory(false)"
                        >
                            <hg-icon icon="chevron-left" size="large" />
                        </hg-button>
                    </b-col>
                    <b-col class="pt-3 pb-4 px-2">
                        <b-row
                            no-gutters
                            class="justify-content-center align-items-center"
                        >
                            <b-col cols="auto">
                                <img
                                    class="img-fluid my-3"
                                    src="@/assets/images/gov/bcid-logo-en.svg"
                                    width="152"
                                    alt="BC Mark"
                                />
                            </b-col>
                            <b-col cols="auto">
                                <h3 class="text-center m-0">
                                    COVID-19 Vaccination Record
                                </h3>
                            </b-col>
                        </b-row>
                        <div class="my-3">
                            <div class="text-muted small">Name:</div>
                            <div class="font-weight-bold">
                                {{ patientName }}
                            </div>
                        </div>
                        <div class="my-3">
                            <div class="text-muted small">Date of Birth:</div>
                            <div
                                class="font-weight-bold"
                                data-testid="patientBirthdate"
                            >
                                {{ patientBirthdate }}
                            </div>
                        </div>
                        <div
                            v-for="(dose, index) in doses"
                            :key="index"
                            class="my-3"
                        >
                            <b-row no-gutters class="mb-2 align-items-center">
                                <b-col cols="auto">
                                    <div
                                        class="text-muted font-weight-bold mr-2"
                                        :data-testid="'dose-' + (index + 1)"
                                    >
                                        Dose {{ index + 1 }}
                                    </div>
                                </b-col>
                                <b-col>
                                    <hr />
                                </b-col>
                            </b-row>
                            <b-row no-gutters class="justify-content-end">
                                <b-col>
                                    <b-row no-gutters align-v="baseline">
                                        <b-col
                                            cols="auto"
                                            class="mr-3 font-weight-bold"
                                        >
                                            {{ dose.product }}
                                        </b-col>
                                        <b-col
                                            v-if="dose.lot"
                                            class="text-muted small"
                                        >
                                            Lot {{ dose.lot }}
                                        </b-col>
                                    </b-row>
                                    <div class="text-muted small">
                                        {{ dose.provider }}
                                    </div>
                                </b-col>
                                <b-col
                                    cols="auto"
                                    data-testid="doseDate"
                                    class="ml-3 font-weight-bold"
                                >
                                    {{ dose.date }}
                                </b-col>
                            </b-row>
                        </div>
                    </b-col>
                    <b-col cols="auto" class="d-print-none">
                        <hg-button
                            data-testid="vr-chevron-right-btn"
                            variant="carousel"
                            class="primary rounded-right h-100"
                            @click="showImmunizationHistory(false)"
                        >
                            <hg-icon icon="chevron-right" size="large" />
                        </hg-button>
                    </b-col>
                </b-row>
            </div>
        </div>
    </div>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

.vaccine-card {
    max-width: 438px;
    color-adjust: exact;

    .actions {
        border-bottom-left-radius: 0.25rem;
        border-bottom-right-radius: 0.25rem;
    }
}

.immunization-history {
    max-width: 700px;
}

.primary {
    color: $primary;
}

a {
    cursor: pointer !important;
}
</style>
<style lang="scss">
@import "@/assets/scss/_variables.scss";

.vld-overlay {
    .vld-background {
        opacity: 0.75;
    }

    .vld-icon {
        text-align: center;
    }
}
</style>
