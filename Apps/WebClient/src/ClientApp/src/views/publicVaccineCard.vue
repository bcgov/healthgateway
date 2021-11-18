<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faInfoCircle } from "@fortawesome/free-solid-svg-icons";
import { saveAs } from "file-saver";
import html2canvas from "html2canvas";
import Vue from "vue";
import { Component, Ref, Watch } from "vue-property-decorator";
import { required } from "vuelidate/lib/validators";
import { Validation } from "vuelidate/vuelidate";
import { Action, Getter } from "vuex-class";

import Image06 from "@/assets/images/landing/006-BCServicesCardLogo.png";
import ErrorCardComponent from "@/components/errorCard.vue";
import LoadingComponent from "@/components/loading.vue";
import MessageModalComponent from "@/components/modal/genericMessage.vue";
import HgDateDropdownComponent from "@/components/shared/hgDateDropdown.vue";
import VaccineCardComponent from "@/components/vaccineCard.vue";
import { VaccinationState } from "@/constants/vaccinationState";
import BannerError from "@/models/bannerError";
import type { WebClientConfiguration } from "@/models/configData";
import CovidVaccineRecord from "@/models/covidVaccineRecord";
import { DateWrapper, StringISODate } from "@/models/dateWrapper";
import VaccinationStatus from "@/models/vaccinationStatus";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import { ILogger, IVaccinationStatusService } from "@/services/interfaces";
import { Mask, phnMask } from "@/utility/masks";
import PHNValidator from "@/utility/phnValidator";
import SnowPlow from "@/utility/snowPlow";

library.add(faInfoCircle);

const validPersonalHealthNumber = (value: string): boolean => {
    var phn = value.replace(/ /g, "");
    return PHNValidator.IsValid(phn);
};

@Component({
    components: {
        "vaccine-card": VaccineCardComponent,
        "error-card": ErrorCardComponent,
        loading: LoadingComponent,
        "message-modal": MessageModalComponent,
        "hg-date-dropdown": HgDateDropdownComponent,
    },
})
export default class PublicVaccineCardView extends Vue {
    private vaccinationStatusService!: IVaccinationStatusService;

    @Action("retrieveVaccineStatus", { namespace: "vaccinationStatus" })
    retrieveVaccineStatus!: (params: {
        phn: string;
        dateOfBirth: StringISODate;
        dateOfVaccine: StringISODate;
    }) => Promise<void>;

    @Action("retrievePublicVaccineRecord", { namespace: "vaccinationStatus" })
    retrievePublicVaccineRecord!: (params: {
        phn: string;
        dateOfBirth: StringISODate;
        dateOfVaccine: StringISODate;
    }) => Promise<void>;

    @Getter("webClient", { namespace: "config" })
    config!: WebClientConfiguration;

    @Getter("vaccinationStatus", { namespace: "vaccinationStatus" })
    status!: VaccinationStatus | undefined;

    @Getter("isLoading", { namespace: "vaccinationStatus" })
    isVaccinationStatusLoading!: boolean;

    @Getter("error", { namespace: "vaccinationStatus" })
    vaccinationStatusError!: BannerError | undefined;

    @Getter("publicVaccineRecordError", { namespace: "vaccinationStatus" })
    publicVaccineRecordError!: BannerError | undefined;

    @Getter("statusMessage", { namespace: "vaccinationStatus" })
    statusMessage!: string;

    @Getter("publicVaccineRecord", { namespace: "vaccinationStatus" })
    vaccineRecord!: CovidVaccineRecord | undefined;

    @Getter("publicVaccineRecordStatusMessage", {
        namespace: "vaccinationStatus",
    })
    vaccineRecordStatusMessage!: string;

    @Getter("publicVaccineRecordIsLoading", {
        namespace: "vaccinationStatus",
    })
    vaccineRecordIsLoading!: boolean;

    @Ref("messageModal")
    readonly messageModal!: MessageModalComponent;

    @Ref("sensitivedocumentDownloadModal")
    readonly sensitivedocumentDownloadModal!: MessageModalComponent;

    private get vaccinationState(): VaccinationState | undefined {
        return this.status?.state;
    }

    private get isPartiallyVaccinated(): boolean {
        return this.vaccinationState === VaccinationState.PartiallyVaccinated;
    }

    private get isVaccinationNotFound(): boolean {
        return this.vaccinationState === VaccinationState.NotFound;
    }

    private get isFullyVaccinated(): boolean {
        return this.vaccinationState === VaccinationState.FullyVaccinated;
    }

    private get bannerError(): BannerError | undefined {
        if (
            this.vaccinationStatusError !== undefined &&
            this.publicVaccineRecordError === undefined
        ) {
            return this.vaccinationStatusError;
        }
        if (
            this.vaccinationStatusError === undefined &&
            this.publicVaccineRecordError !== undefined
        ) {
            return this.publicVaccineRecordError;
        }
    }
    private bcsclogo: string = Image06;

    private logger!: ILogger;
    private displayResult = false;
    private isDownloading = false;

    private phn = "";
    private dateOfBirth = "";
    private dateOfVaccine = "";

    private downloadError: BannerError | null = null;

    private get loadingStatusMessage(): string {
        return this.isDownloading
            ? "Downloading...."
            : this.vaccineRecordIsLoading
            ? this.vaccineRecordStatusMessage
            : this.statusMessage;
    }

    private get downloadButtonShown(): boolean {
        return (
            this.config.modules["VaccinationStatusPdf"] &&
            (this.status?.state === VaccinationState.PartiallyVaccinated ||
                this.status?.state === VaccinationState.FullyVaccinated)
        );
    }

    private validations() {
        return {
            phn: {
                required: required,
                formatted: validPersonalHealthNumber,
            },
            dateOfBirth: {
                required: required,
                maxValue: (value: string) =>
                    new DateWrapper(value).isBefore(new DateWrapper()),
            },
            dateOfVaccine: {
                required: required,
                maxValue: (value: string) =>
                    new DateWrapper(value).isBefore(new DateWrapper()),
            },
        };
    }

    @Watch("status")
    private onStatusChange() {
        if (this.status?.loaded) {
            this.displayResult = true;
        }
    }

    private isValid(param: Validation): boolean | undefined {
        return param.$dirty ? !param.$invalid : undefined;
    }

    private handleSubmit() {
        this.$v.$touch();
        if (!this.$v.$invalid) {
            SnowPlow.trackEvent({
                action: "view_qr",
                text: "vaxcard",
            });
            this.retrieveVaccineStatus({
                phn: this.phn.replace(/ /g, ""),
                dateOfBirth: this.dateOfBirth,
                dateOfVaccine: this.dateOfVaccine,
            })
                .then(() => {
                    this.logger.debug("Vaccine card retrieved");
                })
                .catch((err) => {
                    this.logger.error(`Error retrieving vaccine card: ${err}`);
                });
        }
    }

    private showSensitiveDocumentDownloadModal() {
        this.sensitivedocumentDownloadModal.showModal();
    }

    private get saveExportPdfShown(): boolean {
        return this.config.modules["PublicVaccineDownloadPdf"];
    }

    private download() {
        const printingArea: HTMLElement | null =
            document.querySelector(".vaccine-card");

        if (printingArea !== null) {
            this.downloadError = null;
            this.isDownloading = true;

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
                    fetch(dataUrl).then((res) => {
                        res.blob().then((blob) => {
                            saveAs(blob, "ProvincialVaccineProof.png");
                        });
                    });
                })
                .finally(() => {
                    this.isDownloading = false;
                });
        }
    }

    private created() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        this.vaccinationStatusService =
            container.get<IVaccinationStatusService>(
                SERVICE_IDENTIFIER.VaccinationStatusService
            );
    }

    private get phnMask(): Mask {
        return phnMask;
    }

    private showConfirmationModal() {
        this.messageModal.showModal();
    }

    private showVaccineCardMessageModal() {
        this.sensitivedocumentDownloadModal.showModal();
    }

    @Watch("vaccineRecord")
    private saveVaccinePdf() {
        if (this.vaccineRecord !== undefined) {
            const mimeType = this.vaccineRecord.document.mediaType;
            const downloadLink = `data:${mimeType};base64,${this.vaccineRecord.document.data}`;
            fetch(downloadLink).then((res) => {
                SnowPlow.trackEvent({
                    action: "download_card",
                    text: "Public COVID Card PDF",
                });
                res.blob().then((blob) => {
                    saveAs(blob, "VaccineProof.pdf");
                });
            });
        }
    }

    private downloadVaccinePdf() {
        this.retrievePublicVaccineRecord({
            phn: this.phn.replace(/ /g, ""),
            dateOfBirth: this.dateOfBirth,
            dateOfVaccine: this.dateOfVaccine,
        }).catch((err) => {
            this.logger.error(`Error loading public record data: ${err}`);
        });
    }

    public get isLoading(): boolean {
        return (
            this.isVaccinationStatusLoading ||
            this.vaccineRecordIsLoading ||
            this.isDownloading
        );
    }
}
</script>

<template>
    <div class="background flex-grow-1 d-flex flex-column">
        <loading :is-loading="isLoading" :text="loadingStatusMessage" />
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
        <div v-if="downloadError !== null" class="container d-print-none">
            <b-alert
                variant="danger"
                class="no-print my-3 p-3"
                :show="downloadError !== null"
                dismissible
            >
                <h4>Our Apologies</h4>
                <div data-testid="errorTextDescription" class="pl-4">
                    We've found an issue and the Health Gateway team is working
                    hard to fix it.
                </div>
            </b-alert>
        </div>
        <div
            v-if="displayResult"
            class="vaccine-card align-self-center w-100 p-3"
        >
            <div class="bg-white rounded shadow">
                <vaccine-card
                    :status="status"
                    :error="bannerError"
                    :show-generic-save-instructions="!downloadButtonShown"
                />
                <div
                    v-if="downloadButtonShown"
                    class="
                        actions
                        p-3
                        d-flex d-print-none
                        justify-content-center
                    "
                >
                    <hg-button
                        v-if="!saveExportPdfShown"
                        data-testid="save-a-copy-btn"
                        variant="primary"
                        class="ml-3"
                        @click="showSensitiveDocumentDownloadModal()"
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
                            @click="showVaccineCardMessageModal()"
                            >Image (BC proof)</b-dropdown-item
                        >
                        <b-dropdown-item
                            v-if="saveExportPdfShown"
                            data-testid="save-as-pdf-dropdown-item"
                            @click="showConfirmationModal()"
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
            <message-modal
                ref="sensitivedocumentDownloadModal"
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
                @submit="downloadVaccinePdf"
            />
        </div>
        <div
            v-else
            class="flex-grow-1 d-flex flex-column justify-content-between"
        >
            <form
                class="
                    vaccine-card-form
                    bg-white
                    rounded
                    shadow
                    m-2 m-sm-3
                    py-3
                    px-3 px-sm-5
                    align-self-center
                "
                @submit.prevent="handleSubmit"
            >
                <div class="my-2 my-sm-5 px-0 px-sm-5">
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
                    <b-row>
                        <b-col>
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
                                    :state="isValid($v.phn)"
                                    @blur="$v.phn.$touch()"
                                />
                                <b-form-invalid-feedback
                                    v-if="!$v.phn.required"
                                    aria-label="Invalid Personal Health Number"
                                    data-testid="feedbackPhnIsRequired"
                                >
                                    Personal Health Number is required.
                                </b-form-invalid-feedback>
                                <b-form-invalid-feedback
                                    v-else-if="!$v.phn.formatted"
                                    aria-label="Invalid Personal Health Number"
                                    data-testid="feedbackPhnMustBeValid"
                                >
                                    Personal Health Number must be valid.
                                </b-form-invalid-feedback>
                            </b-form-group>
                        </b-col>
                    </b-row>
                    <b-row>
                        <b-col>
                            <b-form-group
                                label="Date of Birth"
                                label-for="dateOfBirth"
                                :state="isValid($v.dateOfBirth)"
                            >
                                <hg-date-dropdown
                                    id="dateOfBirth"
                                    v-model="dateOfBirth"
                                    :state="isValid($v.dateOfBirth)"
                                    :allow-future="false"
                                    data-testid="dateOfBirthInput"
                                    aria-label="Date of Birth"
                                    @blur="$v.dateOfBirth.$touch()"
                                />
                                <b-form-invalid-feedback
                                    v-if="
                                        $v.dateOfBirth.$dirty &&
                                        !$v.dateOfBirth.required
                                    "
                                    aria-label="Invalid Date of Birth"
                                    data-testid="feedbackDobIsRequired"
                                    force-show
                                >
                                    A valid date of birth is required.
                                </b-form-invalid-feedback>
                                <b-form-invalid-feedback
                                    v-else-if="
                                        $v.dateOfBirth.$dirty &&
                                        !$v.dateOfBirth.maxValue
                                    "
                                    aria-label="Invalid Date of Birth"
                                    force-show
                                >
                                    Date of birth must be in the past.
                                </b-form-invalid-feedback>
                            </b-form-group>
                        </b-col>
                    </b-row>
                    <b-row>
                        <b-col>
                            <b-form-group
                                label="Date of Vaccine (Dose 1 or Dose 2)"
                                label-for="dateOfVaccine"
                                :state="isValid($v.dateOfVaccine)"
                            >
                                <hg-date-dropdown
                                    id="dateOfVaccine"
                                    v-model="dateOfVaccine"
                                    :state="isValid($v.dateOfVaccine)"
                                    :allow-future="false"
                                    :min-year="2020"
                                    data-testid="dateOfVaccineInput"
                                    aria-label="Date of Vaccine (Dose 1 or Dose 2)"
                                    @blur="$v.dateOfBirth.$touch()"
                                />
                                <b-form-invalid-feedback
                                    v-if="
                                        $v.dateOfVaccine.$dirty &&
                                        !$v.dateOfVaccine.required
                                    "
                                    aria-label="Invalid Date of Vaccine"
                                    data-testid="feedbackDovIsRequired"
                                    force-show
                                >
                                    A valid date of vaccine is required.
                                </b-form-invalid-feedback>
                                <b-form-invalid-feedback
                                    v-else-if="
                                        $v.dateOfVaccine.$dirty &&
                                        !$v.dateOfVaccine.maxValue
                                    "
                                    aria-label="Invalid Date of Vaccine"
                                    force-show
                                >
                                    Date of vaccine must be in the past.
                                </b-form-invalid-feedback>
                            </b-form-group>
                        </b-col>
                    </b-row>
                    <b-row class="mt-3 justify-content-between">
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
                    >
                        Your information is being collected to provide you with
                        your COVID-19 vaccination status under s. 26(c) of the
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
                            <b-col><hr /></b-col>
                            <b-col cols="auto">
                                <h3 class="h5 m-0 px-3 text-muted">OR</h3>
                            </b-col>
                            <b-col><hr /></b-col>
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
                                <img
                                    class="mr-2 mb-1"
                                    :src="bcsclogo"
                                    height="16"
                                    alt="BC Services Card App Icon"
                                />
                                <span>Log In with BC Services Card App</span>
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
                        Call: 1-833-838-2323 (Toll-Free)
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
    color-adjust: exact;
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
    color-adjust: exact;

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
