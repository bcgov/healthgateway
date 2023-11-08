<script setup lang="ts">
import { useVuelidate } from "@vuelidate/core";
import { helpers, required } from "@vuelidate/validators";
import { saveAs } from "file-saver";
import html2canvas from "html2canvas";
import { vMaska } from "maska";
import { computed, ref, watch } from "vue";

import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import HgDatePickerComponent from "@/components/common/HgDatePickerComponent.vue";
import InfoPopoverComponent from "@/components/common/InfoPopoverComponent.vue";
import LoadingComponent from "@/components/common/LoadingComponent.vue";
import MessageModalComponent from "@/components/common/MessageModalComponent.vue";
import TooManyRequestsComponent from "@/components/error/TooManyRequestsComponent.vue";
import VaccineCardComponent from "@/components/public/vaccine-card/VaccineCardComponent.vue";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import CovidVaccineRecord from "@/models/covidVaccineRecord";
import { DateWrapper, StringISODate } from "@/models/dateWrapper";
import VaccinationStatus from "@/models/vaccinationStatus";
import { Action, Actor, Format, Text, Type } from "@/plugins/extensions";
import { ILogger, ITrackingService } from "@/services/interfaces";
import { useAppStore } from "@/stores/app";
import { useConfigStore } from "@/stores/config";
import { useVaccinationStatusPublicStore } from "@/stores/vaccinationStatusPublic";
import { phnMask } from "@/utility/masks";
import SnowPlow from "@/utility/snowPlow";
import ValidationUtil from "@/utility/validationUtil";

const phnMaskaOptions = {
    mask: phnMask,
    eager: true,
};

const currentDate = DateWrapper.today();

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const trackingService = container.get<ITrackingService>(
    SERVICE_IDENTIFIER.TrackingService
);

const appStore = useAppStore();
const configStore = useConfigStore();
const vaccinationStatusPublicStore = useVaccinationStatusPublicStore();

const displayResult = ref(false);
const isDownloading = ref(false);
const phn = ref("");
const dateOfBirth = ref("");
const dateOfVaccine = ref("");

const downloadImageModal = ref<InstanceType<typeof MessageModalComponent>>();
const downloadPdfModal = ref<InstanceType<typeof MessageModalComponent>>();

const vaccinationStatus = computed<VaccinationStatus | undefined>(
    () => vaccinationStatusPublicStore.vaccinationStatus
);

const vaccineRecord = computed<CovidVaccineRecord | undefined>(
    () => vaccinationStatusPublicStore.vaccineRecord
);

const bannerError = computed(
    () =>
        vaccinationStatusPublicStore.vaccinationStatusError ??
        vaccinationStatusPublicStore.vaccineRecordError
);

const loadingStatusMessage = computed(() => {
    if (isDownloading.value) {
        return "Downloading";
    } else if (vaccinationStatusPublicStore.vaccineRecordIsLoading) {
        return vaccinationStatusPublicStore.vaccineRecordStatusMessage;
    } else {
        return vaccinationStatusPublicStore.vaccinationStatusStatusMessage;
    }
});

const downloadButtonShown = computed(
    () =>
        vaccinationStatusPublicStore.isPartiallyVaccinated ||
        vaccinationStatusPublicStore.isFullyVaccinated
);

const saveExportPdfShown = computed(
    () =>
        configStore.webConfig.featureToggleConfiguration.covid19.publicCovid19
            .showFederalProofOfVaccination
);

const isLoading = computed(
    () =>
        vaccinationStatusPublicStore.vaccinationStatusIsLoading ||
        vaccinationStatusPublicStore.vaccineRecordIsLoading ||
        isDownloading.value
);

const validations = computed(() => ({
    phn: {
        required: helpers.withMessage(
            "Personal Health Number is required",
            required
        ),
        formatted: helpers.withMessage(
            "Personal Health Number must be valid.",
            ValidationUtil.validatePhn
        ),
    },
    dateOfBirth: {
        required: helpers.withMessage(
            "A valid date of birth is required.",
            required
        ),
        maxValue: helpers.withMessage(
            "Invalid Date of Birth",
            (value: string) =>
                DateWrapper.fromIsoDate(value).isBeforeOrSame(
                    DateWrapper.today()
                )
        ),
    },
    dateOfVaccine: {
        required: helpers.withMessage(
            "A valid date of vaccine is required.",
            required
        ),
        maxValue: helpers.withMessage(
            "Invalid Date of Vaccine",
            (value: string) =>
                DateWrapper.fromIsoDate(value).isBeforeOrSame(
                    DateWrapper.today()
                )
        ),
    },
}));

const v$ = useVuelidate(validations, { phn, dateOfBirth, dateOfVaccine });

function retrieveVaccineStatus(
    phn: string,
    dateOfBirth: StringISODate,
    dateOfVaccine: StringISODate
): Promise<void> {
    return vaccinationStatusPublicStore.retrieveVaccinationStatus(
        phn,
        dateOfBirth,
        dateOfVaccine
    );
}

function retrievePublicVaccineRecord(
    phn: string,
    dateOfBirth: StringISODate,
    dateOfVaccine: StringISODate
): Promise<void> {
    return vaccinationStatusPublicStore.retrieveVaccinationRecord(
        phn,
        dateOfBirth,
        dateOfVaccine
    );
}

function handleSubmit(): void {
    logger.debug("Vaccine card submit started.");
    v$.value.$touch();

    if (!v$.value.$invalid) {
        trackingService.track({
            action: Action.Submit,
            text: Text.Request,
            type: Type.PublicCovid19ProofOfVaccination,
        });
        logger.debug("Vaccine card calling retrieve vaccine status.");
        retrieveVaccineStatus(
            phn.value.replace(/\s/g, ""),
            dateOfBirth.value,
            dateOfVaccine.value
        )
            .then(() => logger.debug("Vaccine card retrieved"))
            .catch((err) =>
                logger.error(`Error retrieving vaccine card: ${err}`)
            );
    }
    logger.debug("Vaccine card submit ended.");
}

function downloadImage(): void {
    const printingArea = document.querySelector<HTMLElement>(".vaccine-card");

    if (printingArea !== null) {
        isDownloading.value = true;
        trackingService.track({
            action: Action.Download,
            text: Text.Document,
            type: Type.PublicCovid19ProofOfVaccination,
            format: Format.Image,
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
                isDownloading.value = false;
            });
    }
}

function downloadPdf(): void {
    retrievePublicVaccineRecord(
        phn.value.replace(/\s/g, ""),
        dateOfBirth.value,
        dateOfVaccine.value
    ).catch((err) => logger.error(`Error loading public record data: ${err}`));
}

function showDownloadImageModal(): void {
    downloadImageModal.value?.showModal();
}

function showDownloadPdfModal(): void {
    downloadPdfModal.value?.showModal();
}

watch(vaccinationStatus, (value) => {
    if (value?.loaded) {
        displayResult.value = true;
    }
});

watch(vaccineRecord, (value) => {
    if (value) {
        const mimeType = value.document.mediaType;
        const downloadLink = `data:${mimeType};base64,${value.document.data}`;
        fetch(downloadLink).then((res) => {
            trackingService.track({
                action: Action.Download,
                text: Text.Document,
                type: Type.PublicCovid19ProofOfVaccination,
                format: Format.Pdf,
                actor: Actor.User,
            });
            SnowPlow.trackEvent({
                action: "download_card",
                text: "Public COVID Card PDF",
            });
            res.blob().then((blob) => saveAs(blob, "VaccineProof.pdf"));
        });
    }
});
</script>

<template>
    <LoadingComponent :is-loading="isLoading" :text="loadingStatusMessage" />
    <div class="h-100 bg-grey-lighten-4 flex-grow-1 d-flex flex-column">
        <div class="bg-primary d-print-none">
            <router-link id="homeLink" to="/" aria-label="Return to home page">
                <img
                    class="img-fluid ma-3"
                    src="@/assets/images/gov/bcid-logo-rev-en.svg"
                    width="152"
                    alt="BC Mark"
                />
            </router-link>
        </div>
        <div
            v-if="displayResult"
            class="vaccine-card align-self-center w-100 ma-4 pa-4 rounded elevation-6"
        >
            <VaccineCardComponent
                :status="vaccinationStatus"
                :error="bannerError"
                :show-generic-save-instructions="!downloadButtonShown"
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
                                title="Image (BC proof)"
                                data-testid="save-as-image-dropdown-item"
                                @click="showDownloadImageModal()"
                            />
                            <v-list-item
                                v-if="saveExportPdfShown"
                                title="PDF (Federal proof)"
                                data-testid="save-as-pdf-dropdown-item"
                                @click="showDownloadPdfModal()"
                            />
                        </v-list>
                    </v-menu>
                </HgButtonComponent>
                <HgButtonComponent
                    v-else
                    data-testid="save-a-copy-btn"
                    variant="primary"
                    text="Save a Copy"
                    @click="showDownloadImageModal()"
                />
            </div>
            <div
                class="d-print-none px-4 pb-4 text-body-1"
                :class="{ 'pt-4': !downloadButtonShown }"
            >
                <p
                    v-if="
                        saveExportPdfShown &&
                        (vaccinationStatusPublicStore.isFullyVaccinated ||
                            vaccinationStatusPublicStore.isPartiallyVaccinated)
                    "
                >
                    Note: Your
                    <span class="font-weight-bold"
                        >federal proof of vaccination</span
                    >
                    can be downloaded using the
                    <span class="font-weight-bold">"Save as"</span> button.
                </p>
                <p
                    v-if="
                        vaccinationStatusPublicStore.isPartiallyVaccinated ||
                        vaccinationStatusPublicStore.isVaccinationNotFound
                    "
                >
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
        <div v-else class="d-flex flex-column">
            <div
                class="vaccine-card-form bg-white rounded elevation-6 ma-2 ma-sm-4 py-6 py-sm-16 px-4 px-sm-16 align-self-center"
            >
                <TooManyRequestsComponent location="publicVaccineCard" />
                <div v-if="bannerError !== undefined">
                    <v-alert
                        variant="outlined"
                        border
                        closable
                        type="error"
                        :title="bannerError.title"
                    >
                        <div data-testid="errorTextDescription">
                            {{ bannerError.description }}
                        </div>
                    </v-alert>
                </div>
                <h1
                    data-testid="vaccineCardFormTitle"
                    class="vaccine-card-form-title text-center font-weight-bold"
                    :class="{
                        'text-h6': appStore.isMobile,
                        'text-h5': !appStore.isMobile,
                    }"
                >
                    Get your proof of vaccination
                </h1>
                <v-divider
                    class="border-opacity-100 mt-4 mb-6"
                    color="accent"
                    thickness="3"
                    role="presentation"
                />
                <p class="mb-6">
                    To get your BC Vaccine Card and Federal Proof of
                    Vaccination, please provide:
                </p>
                <v-row no-gutters class="mb-4">
                    <v-col>
                        <label for="phn">Personal Health Number</label>
                        <div>
                            <v-text-field
                                id="phn"
                                v-model="phn"
                                v-maska:[phnMaskaOptions]
                                label="Personal Health Number"
                                autofocus
                                clearable
                                type="text"
                                data-testid="phnInput"
                                :error-messages="
                                    ValidationUtil.getErrorMessages(v$.phn)
                                "
                                :blur="v$.phn.$touch()"
                            />
                        </div>
                    </v-col>
                </v-row>
                <v-row no-gutters class="mb-4">
                    <v-col>
                        <label for="dateOfBirth">Date of Birth</label>
                        <div>
                            <HgDatePickerComponent
                                id="dateOfBirth"
                                v-model="dateOfBirth"
                                label="Date of Birth"
                                data-testid="dateOfBirthInput"
                                :max-date="currentDate"
                                :error-messages="
                                    ValidationUtil.getErrorMessages(
                                        v$.dateOfBirth
                                    )
                                "
                                @blur="v$.dateOfBirth.$touch()"
                            />
                        </div>
                    </v-col>
                </v-row>
                <v-row no-gutters class="mb-4">
                    <v-col>
                        <label for="dateOfVaccine"
                            >Date of Vaccine (Any Dose)</label
                        >
                        <div>
                            <HgDatePickerComponent
                                id="dateOfVaccine"
                                v-model="dateOfVaccine"
                                :max-date="currentDate"
                                label="Date of Vaccine (Any Dose)"
                                data-testid="dateOfVaccineInput"
                                :error-messages="
                                    ValidationUtil.getErrorMessages(
                                        v$.dateOfVaccine
                                    )
                                "
                                @blur="v$.dateOfVaccine.$touch()"
                            />
                        </div>
                    </v-col>
                </v-row>
                <v-row class="mb-4">
                    <v-col cols="6">
                        <HgButtonComponent
                            data-testid="btnCancel"
                            variant="secondary"
                            text="Cancel"
                            to="/"
                            block
                        />
                    </v-col>
                    <v-col cols="6">
                        <HgButtonComponent
                            data-testid="btnEnter"
                            variant="primary"
                            text="Enter"
                            :loading="
                                vaccinationStatusPublicStore.vaccinationStatusIsLoading
                            "
                            block
                            @click="handleSubmit"
                        />
                    </v-col>
                </v-row>
                <InfoPopoverComponent
                    button-text="Privacy Statement"
                    button-test-id="privacy-statement-button"
                    popover-test-id="privacy-statement-popover"
                >
                    Your information is being collected to provide you with your
                    COVID‑19 vaccination status under s. 26(c) of the
                    <em>Freedom of Information and Protection of Privacy Act</em
                    >. Contact the Ministry Privacy Officer at
                    <a
                        href="mailto:MOH.Privacy.Officer@gov.bc.ca"
                        class="text-link"
                    >
                        MOH.Privacy.Officer@gov.bc.ca
                    </a>
                    if you have any questions about this collection.
                </InfoPopoverComponent>
                <div class="text-center">
                    <v-row class="my-4 no-gutters d-flex align-center">
                        <v-col><v-divider role="presentation" /></v-col>
                        <v-col cols="auto">
                            <h2
                                class="text-h6 font-weight-bold text-medium-emphasis"
                            >
                                OR
                            </h2>
                        </v-col>
                        <v-col><v-divider role="presentation" /></v-col>
                    </v-row>
                    <p>Already a Health Gateway user?</p>
                    <HgButtonComponent
                        aria-label="BC Services Card Login"
                        data-testid="btnLogin"
                        variant="primary"
                        text="Log in with BC Services Card"
                        to="/login"
                    />
                </div>
            </div>
            <div class="bg-white mt-6 px-4 px-sm-12 py-6">
                <h3 class="text-h5 font-weight-bold mb-4">
                    Help in other languages
                </h3>
                <p class="text-body-1">
                    Talk to someone on the phone. Get support in 140+ languages,
                    including:
                </p>
                <p class="text-body-1">
                    <span lang="zh">國粵語</span> |
                    <span lang="pa">ਅਨੁਵਾਦ ਸਰਵਿਸਿਜ਼</span> |
                    <span lang="ar">خدمات-ت-رج-م-ه؟</span> |
                    <span lang="fr">Français</span> |
                    <span lang="es">Español</span>
                </p>
                <p class="text-body-1 font-weight-bold">
                    Service is available every day: 7 am to 7 pm or 9 am to 5 pm
                    on holidays.
                </p>
                <div class="my-4">
                    <HgButtonComponent
                        variant="secondary"
                        text="Call: 1‑833‑838‑2323 (Toll‑Free)"
                        href="tel:+18338382323"
                    />
                </div>
                <div class="my-4">
                    <HgButtonComponent
                        variant="secondary"
                        text="Telephone for the Deaf: Dial 711"
                        href="tel:711"
                    />
                </div>
                <p class="text-body-1 text-medium-emphasis">
                    Standard message and data rates may apply.
                </p>
            </div>
        </div>
    </div>
    <MessageModalComponent
        ref="downloadImageModal"
        title="Vaccine Card Download"
        message="Next, you'll see an image of your card. Depending on your browser, you
            may need to manually save the image to your files or photos. If you want
            to print, we recommend you use the print function in your browser."
        @submit="downloadImage"
    />
    <MessageModalComponent
        ref="downloadPdfModal"
        title="Sensitive Document"
        message="The file that you are downloading contains personal information. If you
            are on a public computer, please ensure that the file is deleted before
            you log off."
        @submit="downloadPdf"
    />
</template>

<style lang="scss" scoped>
.vaccine-card-form {
    max-width: 600px;
}

.vaccine-card {
    max-width: 438px;
    print-color-adjust: exact;
}
</style>
