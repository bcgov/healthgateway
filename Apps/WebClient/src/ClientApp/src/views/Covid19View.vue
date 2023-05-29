<script setup lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import {
    faCheckCircle,
    faChevronLeft,
    faChevronRight,
} from "@fortawesome/free-solid-svg-icons";
import { saveAs } from "file-saver";
import html2canvas from "html2canvas";
import { computed, ref, watch } from "vue";
import { useStore } from "vue-composition-wrapper";

import LoadingComponent from "@/components/LoadingComponent.vue";
import MessageModalComponent from "@/components/modal/MessageModalComponent.vue";
import BreadcrumbComponent from "@/components/navmenu/BreadcrumbComponent.vue";
import VaccineCardComponent from "@/components/VaccineCardComponent.vue";
import { VaccinationState } from "@/constants/vaccinationState";
import BreadcrumbItem from "@/models/breadcrumbItem";
import type { WebClientConfiguration } from "@/models/configData";
import { DateWrapper } from "@/models/dateWrapper";
import { ResultError } from "@/models/errors";
import { ImmunizationEvent } from "@/models/immunizationModel";
import { LoadStatus } from "@/models/storeOperations";
import User from "@/models/user";
import VaccinationStatus from "@/models/vaccinationStatus";
import VaccineRecordState from "@/models/vaccineRecordState";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger } from "@/services/interfaces";
import SnowPlow from "@/utility/snowPlow";

library.add(faCheckCircle, faChevronLeft, faChevronRight);

const breadcrumbItems: BreadcrumbItem[] = [
    {
        text: "COVID‑19",
        to: "/covid19",
        active: true,
        dataTestId: "breadcrumb-covid-19",
    },
];

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const store = useStore();

const isDownloading = ref(false);
const isImmunizationHistoryShown = ref(false);

const vaccineCardMessageModal = ref<MessageModalComponent>();
const downloadConfirmationModal = ref<MessageModalComponent>();

const config = computed<WebClientConfiguration>(
    () => store.getters["config/webClient"]
);
const user = computed<User>(() => store.getters["user/user"]);
const isVaccinationStatusLoading = computed<boolean>(
    () => store.getters["vaccinationStatus/authenticatedIsLoading"]
);
const vaccinationStatus = computed<VaccinationStatus | undefined>(
    () => store.getters["vaccinationStatus/authenticatedVaccinationStatus"]
);
const vaccinationStatusError = computed<ResultError | undefined>(
    () => store.getters["vaccinationStatus/authenticatedError"]
);
const vaccineRecordState = computed<VaccineRecordState>(() =>
    store.getters["vaccinationStatus/authenticatedVaccineRecordState"](
        user.value.hdid
    )
);
const immunizationsAreLoading = computed<boolean>(() =>
    store.getters["immunization/immunizationsAreLoading"](user.value.hdid)
);
const immunizationsAreDeferred = computed<boolean>(() =>
    store.getters["immunization/immunizationsAreDeferred"](user.value.hdid)
);
const covidImmunizations = computed<ImmunizationEvent[]>(() =>
    store.getters["immunization/covidImmunizations"](user.value.hdid)
);
const immunizationsError = computed<ResultError | undefined>(() =>
    store.getters["immunization/immunizationsError"](user.value.hdid)
);

const doses = computed(() =>
    covidImmunizations.value.map((element) => {
        const agent = element.immunization.immunizationAgents[0];
        return {
            product: agent.productName,
            date: DateWrapper.format(element.dateOfImmunization),
            agent: agent.name,
            lot: agent.lotNumber,
            provider: element.providerOrClinic,
        };
    })
);
const vaccinationState = computed(() => vaccinationStatus.value?.state);
const isPartiallyVaccinated = computed(
    () => vaccinationState.value === VaccinationState.PartiallyVaccinated
);
const isVaccinationNotFound = computed(
    () => vaccinationState.value === VaccinationState.NotFound
);
const isVaccineRecordDownloading = computed(
    () => vaccineRecordState.value.status === LoadStatus.REQUESTED
);
const loadingStatusMessage = computed(() => {
    let message = "";
    if (isDownloading.value) {
        message = "Downloading....";
    } else if (isVaccineRecordDownloading.value) {
        message = vaccineRecordState.value.statusMessage;
    }

    return message;
});
const downloadButtonShown = computed(
    () =>
        vaccinationStatus.value?.state ===
            VaccinationState.PartiallyVaccinated ||
        vaccinationStatus.value?.state === VaccinationState.FullyVaccinated
);
const saveExportPdfShown = computed(
    () =>
        config.value.featureToggleConfiguration.covid19.proofOfVaccination
            .exportPdf
);
const isHistoryLoading = computed(
    () => immunizationsAreLoading.value || immunizationsAreDeferred.value
);
const isLoading = computed(
    () =>
        isVaccinationStatusLoading.value ||
        isHistoryLoading.value ||
        isVaccineRecordDownloading.value ||
        isDownloading.value
);
const patientName = computed(() => {
    if (vaccinationStatus.value) {
        return `${vaccinationStatus.value.firstname} ${vaccinationStatus.value.lastname}`;
    }

    return undefined;
});
const patientBirthdate = computed(() =>
    formatDate(vaccinationStatus.value?.birthdate ?? undefined)
);

function retrieveImmunizations(hdid: string): Promise<void> {
    return store.dispatch("immunization/retrieveImmunizations", { hdid });
}

function retrieveVaccineStatus(hdid: string): Promise<void> {
    return store.dispatch(
        "vaccinationStatus/retrieveAuthenticatedVaccineStatus",
        { hdid }
    );
}

function retrieveAuthenticatedVaccineRecord(hdid: string): Promise<void> {
    return store.dispatch(
        "vaccinationStatus/retrieveAuthenticatedVaccineRecord",
        { hdid }
    );
}

function stopAuthenticatedVaccineRecordDownload(hdid: string): void {
    store.dispatch("vaccinationStatus/stopAuthenticatedVaccineRecordDownload", {
        hdid,
    });
}

function formatDate(date: string | undefined): string {
    return date === undefined ? "" : DateWrapper.format(date, "yyyy-MMM-dd");
}

function showImmunizationHistory(show: boolean): void {
    if (show) {
        fetchHistoryData();
    }
    isImmunizationHistoryShown.value = show;
}

function fetchVaccineCardData(): void {
    retrieveVaccineStatus(user.value.hdid)
        .then(() =>
            SnowPlow.trackEvent({
                action: "view_card",
                text: "COVID Card",
            })
        )
        .catch((err) => logger.error(`Error loading COVID-19 data: ${err}`));
}

function fetchHistoryData(): void {
    retrieveImmunizations(user.value.hdid).catch((err) =>
        logger.error(`Error loading immunization data: ${err}`)
    );
}

function retrieveVaccinePdf(): void {
    retrieveAuthenticatedVaccineRecord(user.value.hdid).catch((err) =>
        logger.error(`Error loading authenticated record data: ${err}`)
    );
}

function download(): void {
    const printingArea = document.querySelector<HTMLElement>(".vaccine-card");
    if (printingArea !== null) {
        isDownloading.value = true;
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
            .finally(() => (isDownloading.value = false));
    }
}

function showVaccineCardMessageModal(): void {
    vaccineCardMessageModal.value?.showModal();
}

function showConfirmationModal(): void {
    downloadConfirmationModal.value?.showModal();
}

watch(vaccineRecordState, () => {
    if (
        vaccineRecordState.value.record !== undefined &&
        vaccineRecordState.value.status === LoadStatus.LOADED &&
        vaccineRecordState.value.download
    ) {
        logger.info(`Downloading PDF for hdid: ${user.value.hdid}`);
        const mimeType = vaccineRecordState.value.record.document.mediaType;
        const downloadLink = `data:${mimeType};base64,${vaccineRecordState.value.record.document.data}`;
        fetch(downloadLink).then((res) => {
            SnowPlow.trackEvent({
                action: "download_card",
                text: "COVID Card PDF",
            });
            res.blob().then((blob) =>
                saveAs(blob, "ProvincialVaccineProof.pdf")
            );
        });
        stopAuthenticatedVaccineRecordDownload(user.value.hdid);
    }
});

fetchVaccineCardData();
</script>

<template>
    <div class="background flex-grow-1 d-flex flex-column">
        <BreadcrumbComponent :items="breadcrumbItems" />
        <LoadingComponent
            :is-loading="isLoading"
            :text="loadingStatusMessage"
        />
        <div
            v-if="!isVaccinationStatusLoading && !vaccinationStatusError"
            v-show="!isImmunizationHistoryShown"
            class="vaccine-card align-self-center w-100 p-3"
        >
            <div class="bg-white rounded shadow">
                <VaccineCardComponent
                    :status="vaccinationStatus"
                    :show-generic-save-instructions="!downloadButtonShown"
                    include-previous-button
                    include-next-button
                    @click-previous-button="showImmunizationHistory(true)"
                    @click-next-button="showImmunizationHistory(true)"
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
            <MessageModalComponent
                ref="vaccineCardMessageModal"
                title="Vaccine Card Download"
                message="Next, you'll see an image of your card.
                                Depending on your browser, you may need to
                                manually save the image to your files or photos.
                                If you want to print, we recommend you use the print function in
                                your browser."
                @submit="download"
            />
            <MessageModalComponent
                ref="downloadConfirmationModal"
                title="Sensitive Document Download"
                message="The file that you are downloading contains personal information. If you are on a public computer, please ensure that the file is deleted before you log off."
                @submit="retrieveVaccinePdf"
            />
        </div>
        <div
            v-if="!isHistoryLoading && !immunizationsError"
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
                                    COVID‑19 Vaccination Record
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
    print-color-adjust: exact;

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
