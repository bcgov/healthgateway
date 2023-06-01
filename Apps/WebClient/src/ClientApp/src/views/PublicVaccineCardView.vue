<script setup lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faInfoCircle } from "@fortawesome/free-solid-svg-icons";
import { BaseValidation, useVuelidate } from "@vuelidate/core";
import { required } from "@vuelidate/validators";
import { saveAs } from "file-saver";
import html2canvas from "html2canvas";
import { computed, ref, watch } from "vue";
import { useStore } from "vue-composition-wrapper";

import LoadingComponent from "@/components/LoadingComponent.vue";
import MessageModalComponent from "@/components/modal/MessageModalComponent.vue";
import HgDateDropdownComponent from "@/components/shared/HgDateDropdownComponent.vue";
import TooManyRequestsComponent from "@/components/TooManyRequestsComponent.vue";
import VaccineCardComponent from "@/components/VaccineCardComponent.vue";
import { VaccinationState } from "@/constants/vaccinationState";
import type { WebClientConfiguration } from "@/models/configData";
import CovidVaccineRecord from "@/models/covidVaccineRecord";
import { DateWrapper, StringISODate } from "@/models/dateWrapper";
import { CustomBannerError } from "@/models/errors";
import VaccinationStatus from "@/models/vaccinationStatus";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger } from "@/services/interfaces";
import { phnMask } from "@/utility/masks";
import PHNValidator from "@/utility/phnValidator";
import SnowPlow from "@/utility/snowPlow";

library.add(faInfoCircle);

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const store = useStore();

const config = computed<WebClientConfiguration>(
    () => store.getters["config/webClient"]
);
const status = computed<VaccinationStatus | undefined>(
    () => store.getters["vaccinationStatus/publicVaccinationStatus"]
);
const isVaccinationStatusLoading = computed<boolean>(
    () => store.getters["vaccinationStatus/publicIsLoading"]
);
const vaccinationStatusError = computed<CustomBannerError | undefined>(
    () => store.getters["vaccinationStatus/publicError"]
);
const vaccineRecordError = computed<CustomBannerError | undefined>(
    () => store.getters["vaccinationStatus/publicVaccineRecordError"]
);
const statusMessage = computed<string>(
    () => store.getters["vaccinationStatus/publicStatusMessage"]
);
const vaccineRecord = computed<CovidVaccineRecord | undefined>(
    () => store.getters["vaccinationStatus/publicVaccineRecord"]
);
const vaccineRecordStatusMessage = computed<string>(
    () => store.getters["vaccinationStatus/publicVaccineRecordStatusMessage"]
);
const vaccineRecordIsLoading = computed<boolean>(
    () => store.getters["vaccinationStatus/publicVaccineRecordIsLoading"]
);

const displayResult = ref(false);
const isDownloading = ref(false);
const phn = ref("");
const dateOfBirth = ref("");
const dateOfVaccine = ref("");

const downloadImageModal = ref<MessageModalComponent>();
const downloadPdfModal = ref<MessageModalComponent>();

const vaccinationState = computed(() => status.value?.state);
const isPartiallyVaccinated = computed(
    () => vaccinationState.value === VaccinationState.PartiallyVaccinated
);
const isVaccinationNotFound = computed(
    () => vaccinationState.value === VaccinationState.NotFound
);
const isFullyVaccinated = computed(
    () => vaccinationState.value === VaccinationState.FullyVaccinated
);
const bannerError = computed(
    () => vaccinationStatusError.value ?? vaccineRecordError.value
);
const loadingStatusMessage = computed(() =>
    isDownloading.value
        ? "Downloading..."
        : vaccineRecordIsLoading.value
        ? vaccineRecordStatusMessage.value
        : statusMessage.value
);
const downloadButtonShown = computed(
    () => isPartiallyVaccinated.value || isFullyVaccinated.value
);
const saveExportPdfShown = computed(
    () =>
        config.value.featureToggleConfiguration.covid19.publicCovid19
            .showFederalProofOfVaccination
);
const isLoading = computed(
    () =>
        isVaccinationStatusLoading.value ||
        vaccineRecordIsLoading.value ||
        isDownloading.value
);
const validations = computed(() => ({
    phn: {
        required,
        formatted: (value: string) => PHNValidator.IsValid(value),
    },
    dateOfBirth: {
        required,
        maxValue: (value: string) =>
            new DateWrapper(value).isBefore(new DateWrapper()),
    },
    dateOfVaccine: {
        required,
        maxValue: (value: string) =>
            new DateWrapper(value).isBefore(new DateWrapper()),
    },
}));

const v$ = useVuelidate(validations, { phn, dateOfBirth, dateOfVaccine });

function retrieveVaccineStatus(
    phn: string,
    dateOfBirth: StringISODate,
    dateOfVaccine: StringISODate
): Promise<void> {
    return store.dispatch("vaccinationStatus/retrievePublicVaccineStatus", {
        phn,
        dateOfBirth,
        dateOfVaccine,
    });
}

function retrievePublicVaccineRecord(
    phn: string,
    dateOfBirth: StringISODate,
    dateOfVaccine: StringISODate
): Promise<void> {
    return store.dispatch("vaccinationStatus/retrievePublicVaccineRecord", {
        phn,
        dateOfBirth,
        dateOfVaccine,
    });
}

function isValid(param: BaseValidation): boolean | undefined {
    return param.$dirty ? !param.$invalid : undefined;
}

function handleSubmit(): void {
    v$.value.$touch();
    if (!v$.value.$invalid) {
        SnowPlow.trackEvent({
            action: "view_qr",
            text: "vaxcard",
        });
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

watch(status, (value) => {
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
    <div class="background flex-grow-1 d-flex flex-column">
        <LoadingComponent
            :is-loading="isLoading"
            :text="loadingStatusMessage"
        />
        <div class="header d-print-none">
            <router-link id="homeLink" to="/" aria-label="Return to home page">
                <img
                    class="img-fluid m-3"
                    src="@/assets/images/gov/bcid-logo-rev-en.svg"
                    width="152"
                    alt="BC Mark"
                />
            </router-link>
        </div>
        <div
            v-if="displayResult"
            class="vaccine-card align-self-center w-100 p-3"
        >
            <div class="bg-white rounded shadow">
                <VaccineCardComponent
                    :status="status"
                    :error="bannerError"
                    :show-generic-save-instructions="!downloadButtonShown"
                />
                <div
                    v-if="downloadButtonShown"
                    class="actions p-3 d-flex d-print-none justify-content-center"
                >
                    <hg-button
                        v-if="!saveExportPdfShown"
                        data-testid="save-a-copy-btn"
                        variant="primary"
                        class="ml-3"
                        @click="showDownloadImageModal()"
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
                            data-testid="save-as-image-dropdown-item"
                            @click="showDownloadImageModal()"
                            >Image (BC proof)</b-dropdown-item
                        >
                        <b-dropdown-item
                            v-if="saveExportPdfShown"
                            data-testid="save-as-pdf-dropdown-item"
                            @click="showDownloadPdfModal()"
                            >PDF (Federal proof)</b-dropdown-item
                        >
                    </hg-dropdown>
                </div>
                <div
                    v-if="isFullyVaccinated && saveExportPdfShown"
                    class="d-print-none px-3 pb-3"
                    :class="{ 'pt-3': !downloadButtonShown }"
                >
                    <div class="callout">
                        <p class="m-0">
                            Note: Your
                            <strong>federal proof of vaccination</strong> can be
                            downloaded using the
                            <strong>"Save as"</strong> button.
                        </p>
                    </div>
                </div>
                <div
                    v-if="isPartiallyVaccinated || isVaccinationNotFound"
                    class="d-print-none px-3 pb-3"
                    :class="{ 'pt-3': !downloadButtonShown }"
                >
                    <div class="callout">
                        <p v-if="isPartiallyVaccinated && saveExportPdfShown">
                            Note: Your
                            <strong>federal proof of vaccination</strong> can be
                            downloaded using the
                            <strong>"Save as"</strong> button.
                        </p>
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
            <form
                class="vaccine-card-form bg-white rounded shadow m-2 m-sm-3 py-3 px-3 px-sm-5 align-self-center"
                @submit.prevent="handleSubmit"
            >
                <div class="my-2 my-sm-5 px-0 px-sm-5">
                    <TooManyRequestsComponent location="publicVaccineCard" />
                    <div v-if="bannerError !== undefined">
                        <b-alert
                            variant="danger"
                            class="mb-3 p-3"
                            show
                            dismissible
                        >
                            <h2 class="h4">
                                {{ bannerError.title }}
                            </h2>
                            <div
                                data-testid="errorTextDescription"
                                class="pl-4"
                            >
                                {{ bannerError.description }}
                            </div>
                        </b-alert>
                    </div>
                    <h2
                        data-testid="vaccineCardFormTitle"
                        class="vaccine-card-form-title text-center pb-3 mb-4"
                    >
                        Get your proof of vaccination
                    </h2>
                    <p class="mb-4">
                        To get your BC Vaccine Card and Federal Proof of
                        Vaccination, please provide:
                    </p>
                    <b-form-group
                        label="Personal Health Number"
                        label-for="phn"
                    >
                        <b-form-input
                            id="phn"
                            v-model="phn"
                            v-mask="phnMask"
                            data-testid="phnInput"
                            autofocus
                            aria-label="Personal Health Number"
                            :state="isValid(v$.phn)"
                            @blur="v$.phn.$touch()"
                        />
                        <b-form-invalid-feedback
                            v-if="!v$.phn.required"
                            aria-label="Invalid Personal Health Number"
                            data-testid="feedbackPhnIsRequired"
                        >
                            Personal Health Number is required.
                        </b-form-invalid-feedback>
                        <b-form-invalid-feedback
                            v-else-if="!v$.phn.formatted"
                            aria-label="Invalid Personal Health Number"
                            data-testid="feedbackPhnMustBeValid"
                        >
                            Personal Health Number must be valid.
                        </b-form-invalid-feedback>
                    </b-form-group>
                    <b-form-group
                        label="Date of Birth"
                        label-for="dateOfBirth"
                        :state="isValid(v$.dateOfBirth)"
                    >
                        <HgDateDropdownComponent
                            id="dateOfBirth"
                            v-model="dateOfBirth"
                            :state="isValid(v$.dateOfBirth)"
                            :allow-future="false"
                            data-testid="dateOfBirthInput"
                            aria-label="Date of Birth"
                            @blur="v$.dateOfBirth.$touch()"
                        />
                        <b-form-invalid-feedback
                            v-if="
                                v$.dateOfBirth.$dirty &&
                                !v$.dateOfBirth.required
                            "
                            aria-label="Invalid Date of Birth"
                            data-testid="feedbackDobIsRequired"
                            force-show
                        >
                            A valid date of birth is required.
                        </b-form-invalid-feedback>
                        <b-form-invalid-feedback
                            v-else-if="
                                v$.dateOfBirth.$dirty &&
                                !v$.dateOfBirth.maxValue
                            "
                            aria-label="Invalid Date of Birth"
                            force-show
                        >
                            Date of birth must be in the past.
                        </b-form-invalid-feedback>
                    </b-form-group>
                    <b-form-group
                        label="Date of Vaccine (Any Dose)"
                        label-for="dateOfVaccine"
                        :state="isValid(v$.dateOfVaccine)"
                    >
                        <HgDateDropdownComponent
                            id="dateOfVaccine"
                            v-model="dateOfVaccine"
                            :state="isValid(v$.dateOfVaccine)"
                            :allow-future="false"
                            :min-year="2020"
                            data-testid="dateOfVaccineInput"
                            aria-label="Date of Vaccine (Any Dose)"
                            @blur="v$.dateOfBirth.$touch()"
                        />
                        <b-form-invalid-feedback
                            v-if="
                                v$.dateOfVaccine.$dirty &&
                                !v$.dateOfVaccine.required
                            "
                            aria-label="Invalid Date of Vaccine"
                            data-testid="feedbackDovIsRequired"
                            force-show
                        >
                            A valid date of vaccine is required.
                        </b-form-invalid-feedback>
                        <b-form-invalid-feedback
                            v-else-if="
                                v$.dateOfVaccine.$dirty &&
                                !v$.dateOfVaccine.maxValue
                            "
                            aria-label="Invalid Date of Vaccine"
                            force-show
                        >
                            Date of vaccine must be in the past.
                        </b-form-invalid-feedback>
                    </b-form-group>
                    <b-row class="pt-3 justify-content-between">
                        <b-col cols="5">
                            <hg-button
                                variant="secondary"
                                aria-label="Cancel"
                                data-testid="btnCancel"
                                class="w-100"
                                to="/"
                            >
                                Cancel
                            </hg-button>
                        </b-col>
                        <b-col cols="5">
                            <hg-button
                                variant="primary"
                                aria-label="Enter"
                                type="submit"
                                :disabled="isVaccinationStatusLoading"
                                data-testid="btnEnter"
                                class="w-100"
                            >
                                Enter
                            </hg-button>
                        </b-col>
                    </b-row>
                    <hg-button
                        id="privacy-statement"
                        aria-label="Privacy Statement"
                        href="#"
                        tabindex="0"
                        variant="link"
                        data-testid="btnPrivacyStatement"
                        class="shadow-none p-0 mt-3"
                    >
                        <hg-icon icon="info-circle" size="small" class="mr-1" />
                        <small>Privacy Statement</small>
                    </hg-button>
                    <b-popover
                        target="privacy-statement"
                        triggers="hover focus"
                        placement="topright"
                        boundary="viewport"
                    >
                        Your information is being collected to provide you with
                        your COVID‑19 vaccination status under s. 26(c) of the
                        <em
                            >Freedom of Information and Protection of Privacy
                            Act</em
                        >. Contact the Ministry Privacy Officer at
                        <a href="mailto:MOH.Privacy.Officer@gov.bc.ca"
                            >MOH.Privacy.Officer@gov.bc.ca</a
                        >
                        if you have any questions about this collection.
                    </b-popover>
                    <div class="text-center">
                        <b-row class="my-3 no-gutters align-items-center">
                            <b-col>
                                <hr />
                            </b-col>
                            <b-col cols="auto">
                                <h3 class="h5 m-0 px-3 text-muted">OR</h3>
                            </b-col>
                            <b-col>
                                <hr />
                            </b-col>
                        </b-row>
                        <p>Already a Health Gateway user?</p>
                        <router-link to="/login">
                            <hg-button
                                id="btnLogin"
                                aria-label="BC Services Card Login"
                                data-testid="btnLogin"
                                variant="primary"
                                class="login-button"
                            >
                                <span>Log in with BC Services Card</span>
                            </hg-button>
                        </router-link>
                    </div>
                </div>
            </form>
            <div class="mt-4 px-3 px-sm-5 py-4 bg-white">
                <h3 class="mb-3">Help in other languages</h3>
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
                <div class="my-3">
                    <hg-button variant="secondary" href="tel:+18338382323">
                        Call: 1‑833‑838‑2323 (Toll‑Free)
                    </hg-button>
                </div>
                <div class="my-3">
                    <hg-button variant="secondary" href="tel:711">
                        Telephone for the Deaf: Dial 711
                    </hg-button>
                </div>
                <div class="text-muted">
                    Standard message and data rates may apply.
                </div>
            </div>
        </div>
    </div>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

.background {
    background-color: $hg-background;
}

.header {
    background-color: $hg-brand-primary;
}

.vaccine-card-banner {
    background: $hg-vaccine-card-header;
    color: #212529;

    img {
        width: 2.5rem;
        height: 2.5rem;
    }
}

.vaccine-card-form {
    color: $hg-text-primary;
    print-color-adjust: exact;
    max-width: 600px;
}

.vaccine-card-form-title {
    border-bottom: 3px solid $hg-brand-accent;
    font-size: 1.25rem;

    @media (min-width: 576px) {
        font-size: 1.5rem;
    }
}

.login-button {
    background-color: #1a5a95 !important;
    border-color: #1a5a95 !important;
}

.vaccine-card {
    max-width: 438px;
    print-color-adjust: exact;

    .actions {
        border-bottom-left-radius: 0.25rem;
        border-bottom-right-radius: 0.25rem;
    }
}

.callout {
    padding: 1rem;
    border-left: 0.25rem solid $hg-brand-secondary;
    border-radius: 0.25rem;
    background-color: $hg-background;
    color: $hg-text-primary;
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
