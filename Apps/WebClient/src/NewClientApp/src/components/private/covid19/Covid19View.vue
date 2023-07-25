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

import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import LoadingComponent from "@/components/common/LoadingComponent.vue";
import MessageModalComponent from "@/components/common/MessageModalComponent.vue";
import VaccineCardComponent from "@/components/common/VaccineCardComponent.vue";
import BreadcrumbComponent from "@/components/site/BreadcrumbComponent.vue";
import { VaccinationState } from "@/constants/vaccinationState";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import BreadcrumbItem from "@/models/breadcrumbItem";
import { DateWrapper } from "@/models/dateWrapper";
import { ResultError } from "@/models/errors";
import { ImmunizationEvent } from "@/models/immunizationModel";
import { LoadStatus } from "@/models/storeOperations";
import VaccinationStatus from "@/models/vaccinationStatus";
import VaccineRecordState from "@/models/vaccineRecordState";
import { ILogger } from "@/services/interfaces";
import { useConfigStore } from "@/stores/config";
import { useImmunizationStore } from "@/stores/immunization";
import { useUserStore } from "@/stores/user";
import { useVaccinationStatusAuthenticatedStore } from "@/stores/vaccinationStatusAuthenticated";
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

const configStore = useConfigStore();
const immunizationStore = useImmunizationStore();
const userStore = useUserStore();
const vaccinationStatusAuthenticatedStore =
    useVaccinationStatusAuthenticatedStore();

const isDownloading = ref(false);
const isImmunizationHistoryShown = ref(false);

const vaccineCardMessageModal =
    ref<InstanceType<typeof MessageModalComponent>>();

const downloadConfirmationModal =
    ref<InstanceType<typeof MessageModalComponent>>();

const isVaccinationStatusLoading = computed<boolean>(
    () => vaccinationStatusAuthenticatedStore.vaccinationStatusIsLoading
);

const vaccinationStatus = computed<VaccinationStatus | undefined>(
    () => vaccinationStatusAuthenticatedStore.vaccinationStatus
);

const vaccinationStatusError = computed<ResultError | undefined>(
    () => vaccinationStatusAuthenticatedStore.vaccinationStatusError
);

const vaccineRecordState = computed<VaccineRecordState>(() =>
    vaccinationStatusAuthenticatedStore.vaccineRecordState(userStore.user.hdid)
);

const covidImmunizations = computed<ImmunizationEvent[]>(() =>
    immunizationStore.covidImmunizations(userStore.user.hdid)
);

const immunizationsError = computed<ResultError | undefined>(() =>
    immunizationStore.immunizationsError(userStore.user.hdid)
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
        configStore.webConfig.featureToggleConfiguration.covid19
            .proofOfVaccination.exportPdf
);
const isHistoryLoading = computed(
    () =>
        immunizationStore.immunizationsAreLoading(userStore.user.hdid) ||
        immunizationStore.immunizationsAreDeferred(userStore.user.hdid)
);
const isLoading = computed(
    () =>
        vaccinationStatusAuthenticatedStore.vaccinationStatusIsLoading ||
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
    return immunizationStore.retrieveImmunizations(hdid);
}

function retrieveVaccineStatus(hdid: string): Promise<void> {
    return vaccinationStatusAuthenticatedStore.retrieveVaccinationStatus(hdid);
}

function retrieveAuthenticatedVaccineRecord(hdid: string): Promise<void> {
    return vaccinationStatusAuthenticatedStore.retrieveVaccineRecord(hdid);
}

function stopAuthenticatedVaccineRecordDownload(hdid: string): void {
    vaccinationStatusAuthenticatedStore.stopVaccineRecordDownload(hdid);
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
    retrieveVaccineStatus(userStore.user.hdid)
        .then(() =>
            SnowPlow.trackEvent({
                action: "view_card",
                text: "COVID Card",
            })
        )
        .catch((err) => logger.error(`Error loading COVID-19 data: ${err}`));
}

function fetchHistoryData(): void {
    retrieveImmunizations(userStore.user.hdid).catch((err) =>
        logger.error(`Error loading immunization data: ${err}`)
    );
}

function retrieveVaccinePdf(): void {
    retrieveAuthenticatedVaccineRecord(userStore.user.hdid).catch((err) =>
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
        logger.info(`Downloading PDF for hdid: ${userStore.user.hdid}`);
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
        stopAuthenticatedVaccineRecordDownload(userStore.user.hdid);
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
            class="vaccine-card align-self-center w-100 pa-4"
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
                    class="actions pa-4 d-flex d-print-none justify-center"
                >
                    <HgButtonComponent
                        v-if="!saveExportPdfShown"
                        data-testid="save-card-btn"
                        aria-label="Save a Copy"
                        variant="primary"
                        text="Save a Copy"
                        @click="showVaccineCardMessageModal()"
                    >
                    </HgButtonComponent>
                    <HgButtonComponent
                        v-if="saveExportPdfShown"
                        variant="primary"
                        append-icon="fas fa-caret-down"
                        data-testid="save-dropdown-btn"
                    >
                        <span>Save as</span>
                        <v-menu activator="parent">
                            <v-list>
                                <v-list-item
                                    title="PDF"
                                    data-testid="save-as-pdf-dropdown-item"
                                    @click="showConfirmationModal()"
                                />
                                <v-list-item
                                    title="Image"
                                    data-testid="save-as-image-dropdown-item"
                                    @click="showVaccineCardMessageModal()"
                                />
                            </v-list>
                        </v-menu>
                    </HgButtonComponent>
                </div>
                <div
                    v-if="
                        vaccinationStatusAuthenticatedStore.isPartiallyVaccinated ||
                        vaccinationStatusAuthenticatedStore.isVaccinationNotFound
                    "
                    class="d-print-none px-4 pb-4"
                    :class="{ 'pt-4': !downloadButtonShown }"
                >
                    <div class="callout">
                        <p class="ma-0">
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
            class="immunization-history flex-grow-1 align-self-center w-100 pa-4"
        >
            <div class="flex-grow-1 bg-white rounded shadow">
                <v-row no-gutters>
                    <v-col cols="auto" class="d-print-none">
                        <HgButtonComponent
                            color="primary"
                            variant="link"
                            data-testid="vr-chevron-left-btn"
                            class="rounded-left h-100"
                            prepend-icon="chevron-left"
                            @click="showImmunizationHistory(false)"
                        />
                    </v-col>
                    <v-col class="pt-4 pb-6 px-2">
                        <v-row
                            no-gutters
                            class="d-flex align-center justify-center"
                        >
                            <v-col cols="auto">
                                <img
                                    class="img-fluid my-4"
                                    src="@/assets/images/gov/bcid-logo-en.svg"
                                    width="152"
                                    alt="BC Mark"
                                />
                            </v-col>
                            <v-col cols="auto">
                                <h3 class="text-center ma-0">
                                    COVID‑19 Vaccination Record
                                </h3>
                            </v-col>
                        </v-row>
                        <div class="my-4">
                            <div class="text-muted small">Name:</div>
                            <div class="font-weight-bold">
                                {{ patientName }}
                            </div>
                        </div>
                        <div class="my-4">
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
                            class="my-4"
                        >
                            <v-row no-gutters class="mb-2 d-flex align-center">
                                <v-col cols="auto">
                                    <div
                                        class="text-muted font-weight-bold mr-2"
                                        :data-testid="'dose-' + (index + 1)"
                                    >
                                        Dose {{ index + 1 }}
                                    </div>
                                </v-col>
                                <v-col>
                                    <hr />
                                </v-col>
                            </v-row>
                            <v-row no-gutters class="justify-content-end">
                                <v-col>
                                    <v-row no-gutters align-v="baseline">
                                        <v-col
                                            cols="auto"
                                            class="mr-4 font-weight-bold"
                                        >
                                            {{ dose.product }}
                                        </v-col>
                                        <v-col
                                            v-if="dose.lot"
                                            class="text-muted small"
                                        >
                                            Lot {{ dose.lot }}
                                        </v-col>
                                    </v-row>
                                    <div class="text-muted small">
                                        {{ dose.provider }}
                                    </div>
                                </v-col>
                                <v-col
                                    cols="auto"
                                    data-testid="doseDate"
                                    class="ml-4 font-weight-bold"
                                >
                                    {{ dose.date }}
                                </v-col>
                            </v-row>
                        </div>
                    </v-col>
                    <v-col cols="auto" class="d-print-none">
                        <HgButtonComponent
                            color="primary"
                            variant="link"
                            data-testid="vr-chevron-right-btn"
                            class="rounded-right h-100"
                            prepend-icon="chevron-right"
                            @click="showImmunizationHistory(false)"
                        />
                    </v-col>
                </v-row>
            </div>
        </div>
    </div>
</template>

<style lang="scss" scoped>
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

a {
    cursor: pointer !important;
}
</style>
