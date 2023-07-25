<script setup lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faInfoCircle } from "@fortawesome/free-solid-svg-icons";
import { useVuelidate } from "@vuelidate/core";
import { helpers } from "@vuelidate/validators";
import { required } from "@vuelidate/validators";
import { saveAs } from "file-saver";
import html2canvas from "html2canvas";
import { vMaska } from "maska";
import { computed, ref, watch } from "vue";

import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import HgDatePickerComponent from "@/components/common/HgDatePickerComponent.vue";
import LoadingComponent from "@/components/common/LoadingComponent.vue";
import MessageModalComponent from "@/components/common/MessageModalComponent.vue";
import VaccineCardComponent from "@/components/common/VaccineCardComponent.vue";
import TooManyRequestsComponent from "@/components/error/TooManyRequestsComponent.vue";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import CovidVaccineRecord from "@/models/covidVaccineRecord";
import { DateWrapper, StringISODate } from "@/models/dateWrapper";
import VaccinationStatus from "@/models/vaccinationStatus";
import { ILogger } from "@/services/interfaces";
import { useConfigStore } from "@/stores/config";
import { useVaccinationStatusPublicStore } from "@/stores/vaccinationStatusPublic";
import { phnMask } from "@/utility/masks";
import SnowPlow from "@/utility/snowPlow";
import ValidationUtil from "@/utility/validationUtil";

library.add(faInfoCircle);

const phnMaskaOptions = {
    mask: phnMask,
    eager: true,
};

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);

const configStore = useConfigStore();
const vccinationStatusPublicStore = useVaccinationStatusPublicStore();

const displayResult = ref(false);
const isDownloading = ref(false);
const phn = ref("");
const dateOfBirth = ref("");
const dateOfVaccine = ref("");

const downloadImageModal = ref<InstanceType<typeof MessageModalComponent>>();
const downloadPdfModal = ref<InstanceType<typeof MessageModalComponent>>();

const vaccinationStatus = computed<VaccinationStatus | undefined>(
    () => vccinationStatusPublicStore.vaccinationStatus
);

const vaccineRecord = computed<CovidVaccineRecord | undefined>(
    () => vccinationStatusPublicStore.vaccineRecord
);

const bannerError = computed(
    () =>
        vccinationStatusPublicStore.vaccinationStatusError ??
        vccinationStatusPublicStore.vaccineRecordError
);

const loadingStatusMessage = computed(() =>
    isDownloading.value
        ? "Downloading..."
        : vccinationStatusPublicStore.vaccineRecordIsLoading
        ? vccinationStatusPublicStore.vaccineRecordStatusMessage
        : vccinationStatusPublicStore.vaccinationStatusStatusMessage
);

const downloadButtonShown = computed(
    () =>
        vccinationStatusPublicStore.isPartiallyVaccinated ||
        vccinationStatusPublicStore.isFullyVaccinated
);

const saveExportPdfShown = computed(
    () =>
        configStore.webConfig.featureToggleConfiguration.covid19.publicCovid19
            .showFederalProofOfVaccination
);

const isLoading = computed(
    () =>
        vccinationStatusPublicStore.vaccinationStatusIsLoading ||
        vccinationStatusPublicStore.vaccineRecordIsLoading ||
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
                new DateWrapper(value).isBefore(new DateWrapper())
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
                new DateWrapper(value).isBefore(new DateWrapper())
        ),
    },
}));

const v$ = useVuelidate(validations, { phn, dateOfBirth, dateOfVaccine });

function retrieveVaccineStatus(
    phn: string,
    dateOfBirth: StringISODate,
    dateOfVaccine: StringISODate
): Promise<void> {
    return vccinationStatusPublicStore.retrieveVaccinationStatus(
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
    return vccinationStatusPublicStore.retrieveVaccinationRecord(
        phn,
        dateOfBirth,
        dateOfVaccine
    );
}

function handleSubmit(): void {
    logger.debug("Vaccine card submit started.");
    v$.value.$touch();

    if (!v$.value.$invalid) {
        SnowPlow.trackEvent({
            action: "view_qr",
            text: "vaxcard",
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

        SnowPlow.trackEvent({
            action: "save_qr",
            text: "vaxcard",
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
    <div class="h-100 bg-grey-lighten-4 flex-grow-1 d-flex flex-column">
        <LoadingComponent
            :is-loading="isLoading"
            :text="loadingStatusMessage"
        />
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
            class="vaccine-card align-self-center w-100 pa-4"
        >
            <div class="bg-white rounded shadow">
                <VaccineCardComponent
                    :status="vaccinationStatus"
                    :error="bannerError"
                    :show-generic-save-instructions="!downloadButtonShown"
                />
                <div
                    v-if="downloadButtonShown"
                    class="actions pa-4 d-flex d-print-none justify-center"
                >
                    <HgButtonComponent
                        v-if="!saveExportPdfShown"
                        data-testid="ave-a-copy-btn"
                        variant="primary"
                        class="ml-4"
                        text="Save a Copy"
                        @click="showDownloadImageModal()"
                    />
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
                </div>
                <div
                    v-if="
                        vccinationStatusPublicStore.isFullyVaccinated &&
                        saveExportPdfShown
                    "
                    class="d-print-none px-4 pb-4"
                    :class="{ 'pt-4': !downloadButtonShown }"
                >
                    <div class="callout">
                        <p class="ma-0">
                            Note: Your
                            <strong>federal proof of vaccination</strong> can be
                            downloaded using the
                            <strong>"Save as"</strong> button.
                        </p>
                    </div>
                </div>
                <div
                    v-if="
                        vccinationStatusPublicStore.isPartiallyVaccinated ||
                        vccinationStatusPublicStore.isVaccinationNotFound
                    "
                    class="d-print-none px-4 pb-4"
                    :class="{ 'pt-4': !downloadButtonShown }"
                >
                    <div class="callout">
                        <p
                            v-if="
                                vccinationStatusPublicStore.isPartiallyVaccinated &&
                                saveExportPdfShown
                            "
                        >
                            Note: Your
                            <strong>federal proof of vaccination</strong> can be
                            downloaded using the
                            <strong>"Save as"</strong> button.
                        </p>
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
                ref="downloadImageModal"
                title="Vaccine Card Download"
                message="Next, you'll see an image of your card.
                                Depending on your browser, you may need to
                                manually save the image to your files or photos.
                                If you want to print, we recommend you use the print function in
                                your browser."
                @submit="downloadImage"
            />
            <MessageModalComponent
                ref="downloadPdfModal"
                title="Sensitive Document Download"
                message="The file that you are downloading contains personal information. If you are on a public computer, please ensure that the file is deleted before you log off."
                @submit="downloadPdf"
            />
        </div>
        <div
            v-else
            class="flex-grow-1 d-flex flex-column justify-content-between"
        >
            <v-form
                class="vaccine-card-form bg-white rounded shadow ma-2 ma-sm-4 py-4 px-4 px-sm-12 align-self-center"
                @submit.prevent="handleSubmit"
            >
                <div class="my-2 my-sm-12 px-0 px-sm-12">
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
                    <h2
                        data-testid="vaccineCardFormTitle"
                        class="vaccine-card-form-title text-center pb-4 mb-6"
                    >
                        Get your proof of vaccination
                    </h2>
                    <p class="mb-6">
                        To get your BC Vaccine Card and Federal Proof of
                        Vaccination, please provide:
                    </p>
                    <div>
                        <v-row>
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
                                        :error="ValidationUtil.isValid(v$.phn)"
                                        :error-messages="
                                            ValidationUtil.getErrorMessages(
                                                v$.phn
                                            )
                                        "
                                        :blur="v$.phn.$touch()"
                                    ></v-text-field>
                                </div>
                            </v-col>
                        </v-row>
                    </div>
                    <div>
                        <v-row>
                            <v-col>
                                <label for="dateOfBirth">Date of Birth</label>
                                <div>
                                    <HgDatePickerComponent
                                        id="dateOfBirth"
                                        v-model="dateOfBirth"
                                        label="Date of Birth"
                                        data-testid="dateOfBirthInput"
                                        :state="
                                            ValidationUtil.isValid(
                                                v$.dateOfBirth
                                            )
                                        "
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
                    </div>
                    <div>
                        <v-row>
                            <v-col>
                                <label for="dateOfVaccine"
                                    >Date of Vaccine (Any Dose)</label
                                >
                                <div>
                                    <HgDatePickerComponent
                                        id="dateOfVaccine"
                                        v-model="dateOfVaccine"
                                        label="Date of Vaccine (Any Dose)"
                                        data-testid="dateOfBirthInput"
                                        :state="
                                            ValidationUtil.isValid(
                                                v$.dateOfVaccine
                                            )
                                        "
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
                    </div>
                    <v-row class="pt-4 justify-content-between">
                        <v-col cols="6">
                            <HgButtonComponent
                                variant="secondary"
                                aria-label="Cancel"
                                data-testid="btnCancel"
                                block
                                to="/"
                            >
                                Cancel
                            </HgButtonComponent>
                        </v-col>
                        <v-col cols="6">
                            <HgButtonComponent
                                variant="primary"
                                aria-label="Enter"
                                :disabled="
                                    vccinationStatusPublicStore.vaccinationStatusIsLoading
                                "
                                data-testid="btnEnter"
                                block
                                type="submit"
                            >
                                Enter
                            </HgButtonComponent>
                        </v-col>
                    </v-row>
                    <HgButtonComponent
                        id="privacy-statement"
                        aria-label="Privacy Statement"
                        variant="link"
                        data-testid="btnPrivacyStatement"
                        class="shadow-none pa-0 mt-4"
                        prepend-icon="info-circle"
                    >
                        Privacy Statement
                        <v-overlay
                            activator="parent"
                            location-strategy="connected"
                            scroll-strategy="block"
                        >
                            <v-card class="pa-2" width="250">
                                <span class="text-body-2">
                                    Your information is being collected to
                                    provide you with your COVID‑19 vaccination
                                    status under s. 26(c) of the
                                    <em
                                        >Freedom of Information and Protection
                                        of Privacy Act</em
                                    >. Contact the Ministry Privacy Officer at
                                    <a
                                        href="mailto:MOH.Privacy.Officer@gov.bc.ca"
                                        >MOH.Privacy.Officer@gov.bc.ca</a
                                    >
                                    if you have any questions about this
                                    collection.
                                </span></v-card
                            >
                        </v-overlay>
                    </HgButtonComponent>
                    <div class="text-center">
                        <v-row class="my-4 no-gutters d-flex align-center">
                            <v-col>
                                <hr />
                            </v-col>
                            <v-col cols="auto">
                                <h3 class="h5 ma-0 px-4 text-muted">OR</h3>
                            </v-col>
                            <v-col>
                                <hr />
                            </v-col>
                        </v-row>
                        <p>Already a Health Gateway user?</p>
                        <router-link to="/login">
                            <HgButtonComponent
                                aria-label="BC Services Card Login"
                                data-testid="btnLogin"
                                variant="primary"
                                class="login-button"
                                text="Log in with BC Services Card"
                            >
                                <span>Log in with BC Services Card</span>
                            </HgButtonComponent>
                        </router-link>
                    </div>
                </div>
            </v-form>
            <div class="mt-6 px-4 px-sm-12 py-6 bg-white">
                <h3 class="mb-4">Help in other languages</h3>
                <p>
                    Talk to someone on the phone. Get support in 140+ languages,
                    including:
                </p>
                <p>
                    <span lang="zh">國粵語</span> |
                    <span lang="pa">ਅਨੁਵਾਦ ਸਰਵਿਸਿਜ਼</span> |
                    <span lang="ar">خدمات-ت-رج-م-ه؟</span> |
                    <span lang="fr">Français</span> |
                    <span lang="es">Español</span>
                </p>
                <p>
                    <strong>
                        Service is available every day: 7 am to 7 pm or 9 am to
                        5 pm on holidays.
                    </strong>
                </p>
                <div class="my-4">
                    <HgButtonComponent
                        variant="secondary"
                        text="Call: 1‑833‑838‑2323 (Toll‑Free)"
                        href="tel:711"
                    />
                </div>
                <div class="my-4">
                    <HgButtonComponent
                        variant="secondary"
                        text="Telephone for the Deaf: Dial 711"
                        href="tel:+18338382323"
                    />
                </div>
                <div class="text-muted">
                    Standard message and data rates may apply.
                </div>
            </div>
        </div>
    </div>
</template>

<style lang="scss" scoped>
.vaccine-card-form {
    print-color-adjust: exact;
    max-width: 600px;
}

.vaccine-card-form-title {
    border-bottom: 3px solid #fcba19;
    font-size: 1.25rem;

    @media (min-width: 576px) {
        font-size: 1.5rem;
    }
}

.vaccine-card {
    max-width: 438px;
    print-color-adjust: exact;

    .actions {
        border-bottom-left-radius: 0.25rem;
        border-bottom-right-radius: 0.25rem;
    }
}
</style>
