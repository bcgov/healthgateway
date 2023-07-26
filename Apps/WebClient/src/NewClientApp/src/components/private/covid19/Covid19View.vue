<script setup lang="ts">
import { saveAs } from "file-saver";
import html2canvas from "html2canvas";
import { computed, ref, watch } from "vue";

import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import LoadingComponent from "@/components/common/LoadingComponent.vue";
import MessageModalComponent from "@/components/common/MessageModalComponent.vue";
import VaccineCardComponent from "@/components/public/vaccine-card/VaccineCardComponent.vue";
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
    <BreadcrumbComponent :items="breadcrumbItems" />
    <LoadingComponent :is-loading="isLoading" :text="loadingStatusMessage" />
    <div class="d-flex flex-column align-center">
        <div
            v-if="!isVaccinationStatusLoading && !vaccinationStatusError"
            v-show="!isImmunizationHistoryShown"
            class="vaccine-card w-100 rounded elevation-6"
        >
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
                class="pa-4 d-flex d-print-none justify-center"
            >
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
                <HgButtonComponent
                    v-else
                    data-testid="save-card-btn"
                    variant="primary"
                    text="Save a Copy"
                    @click="showVaccineCardMessageModal()"
                />
            </div>
            <div
                v-if="
                    vaccinationStatusAuthenticatedStore.isPartiallyVaccinated ||
                    vaccinationStatusAuthenticatedStore.isVaccinationNotFound
                "
                class="d-print-none px-4 pb-4"
                :class="{ 'pt-4': !downloadButtonShown }"
            >
                <p class="text-body-1">
                    To learn more, visit
                    <a
                        href="https://www2.gov.bc.ca/gov/content/covid-19/vaccine/proof"
                        rel="noopener"
                        target="_blank"
                        class="text-link"
                        >BC Proof of Vaccination</a
                    >.
                </p>
            </div>
        </div>
        <v-row
            v-if="!isHistoryLoading && !immunizationsError"
            v-show="isImmunizationHistoryShown"
            no-gutters
            class="immunization-history w-100 rounded elevation-6"
        >
            <v-col cols="auto" class="d-print-none">
                <HgButtonComponent
                    data-testid="vr-chevron-left-btn"
                    class="rounded-s rounded-e-0 h-100"
                    color="primary"
                    variant="link"
                    size="x-small"
                    @click="showImmunizationHistory(false)"
                >
                    <v-icon icon="chevron-left" size="x-large" />
                </HgButtonComponent>
            </v-col>
            <v-col class="pt-4 pb-6 px-2">
                <v-row no-gutters class="d-flex align-center justify-center">
                    <v-col cols="auto">
                        <img
                            class="img-fluid my-4"
                            src="@/assets/images/gov/bcid-logo-en.svg"
                            width="152"
                            alt="BC Mark"
                        />
                    </v-col>
                    <v-col cols="auto">
                        <h3 class="text-h6 font-weight-bold text-center">
                            COVID‑19 Vaccination Record
                        </h3>
                    </v-col>
                </v-row>
                <div class="my-4">
                    <div class="text-body-2 text-medium-emphasis">Name:</div>
                    <div class="text-body-1 font-weight-bold">
                        {{ patientName }}
                    </div>
                </div>
                <div class="my-4">
                    <div class="text-body-2 text-medium-emphasis">
                        Date of Birth:
                    </div>
                    <div
                        data-testid="patientBirthdate"
                        class="text-body-1 font-weight-bold"
                    >
                        {{ patientBirthdate }}
                    </div>
                </div>
                <div v-for="(dose, index) in doses" :key="index" class="my-4">
                    <v-row dense class="d-flex align-center">
                        <v-col
                            cols="auto"
                            :data-testid="'dose-' + (index + 1)"
                            class="text-body-1 font-weight-bold text-medium-emphasis"
                        >
                            Dose {{ index + 1 }}
                        </v-col>
                        <v-col><v-divider role="presentation" /></v-col>
                    </v-row>
                    <v-row dense>
                        <v-col>
                            <v-row dense class="align-baseline">
                                <v-col
                                    cols="auto"
                                    class="text-body-1 font-weight-bold"
                                >
                                    {{ dose.product }}
                                </v-col>
                                <v-col
                                    v-if="dose.lot"
                                    class="text-body-2 text-medium-emphasis"
                                >
                                    Lot {{ dose.lot }}
                                </v-col>
                            </v-row>
                            <div class="text-body-2 text-medium-emphasis">
                                {{ dose.provider }}
                            </div>
                        </v-col>
                        <v-col
                            cols="auto"
                            data-testid="doseDate"
                            class="text-body-1 font-weight-bold"
                        >
                            {{ dose.date }}
                        </v-col>
                    </v-row>
                </div>
            </v-col>
            <v-col cols="auto" class="d-print-none">
                <HgButtonComponent
                    data-testid="vr-chevron-right-btn"
                    class="rounded-e rounded-s-0 h-100"
                    color="primary"
                    variant="link"
                    size="x-small"
                    @click="showImmunizationHistory(false)"
                >
                    <v-icon icon="chevron-right" size="x-large" />
                </HgButtonComponent>
            </v-col>
        </v-row>
    </div>
    <MessageModalComponent
        ref="vaccineCardMessageModal"
        title="Vaccine Card Download"
        message="Next, you'll see an image of your card. Depending on your
                    browser, you may need to manually save the image to your files
                    or photos. If you want to print, we recommend you use the print
                    function in your browser."
        @submit="download"
    />
    <MessageModalComponent
        ref="downloadConfirmationModal"
        title="Sensitive Document Download"
        message="The file that you are downloading contains personal information.
                    If you are on a public computer, please ensure that the file is
                    deleted before you log off."
        @submit="retrieveVaccinePdf"
    />
</template>

<style lang="scss" scoped>
.vaccine-card {
    max-width: 438px;
    print-color-adjust: exact;
}

.immunization-history {
    max-width: 700px;
}
</style>
