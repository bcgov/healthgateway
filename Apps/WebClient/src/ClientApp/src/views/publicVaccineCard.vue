<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faInfoCircle } from "@fortawesome/free-solid-svg-icons";
import { saveAs } from "file-saver";
import Vue from "vue";
import { Component, Ref, Watch } from "vue-property-decorator";
import { required } from "vuelidate/lib/validators";
import { Validation } from "vuelidate/vuelidate";
import { Action, Getter } from "vuex-class";

import Image06 from "@/assets/images/landing/006-BCServicesCardLogo.png";
import DatePickerComponent from "@/components/datePicker.vue";
import ErrorCardComponent from "@/components/errorCard.vue";
import LoadingComponent from "@/components/loading.vue";
import MessageModalComponent from "@/components/modal/genericMessage.vue";
import HgDateDropdownComponent from "@/components/shared/hgDateDropdown.vue";
import VaccineCardComponent from "@/components/vaccineCard.vue";
import { VaccinationState } from "@/constants/vaccinationState";
import BannerError from "@/models/bannerError";
import type { WebClientConfiguration } from "@/models/configData";
import { DateWrapper, StringISODate } from "@/models/dateWrapper";
import Report from "@/models/report";
import VaccinationStatus from "@/models/vaccinationStatus";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import { ILogger } from "@/services/interfaces";
import { Mask, phnMask } from "@/utility/masks";
import PHNValidator from "@/utility/phnValidator";
import SnowPlow from "@/utility/snowPlow";

library.add(faInfoCircle);

const validPersonalHealthNumber = (value: string): boolean => {
    var phn = value.replace(/\D/g, "");
    return PHNValidator.IsValid(phn);
};

@Component({
    components: {
        "vaccine-card": VaccineCardComponent,
        "date-picker": DatePickerComponent,
        "error-card": ErrorCardComponent,
        loading: LoadingComponent,
        MessageModalComponent,
        "hg-date-dropdown": HgDateDropdownComponent,
    },
})
export default class PublicVaccineCardView extends Vue {
    @Action("retrieveVaccineStatus", { namespace: "vaccinationStatus" })
    retrieveVaccineStatus!: (params: {
        phn: string;
        dateOfBirth: StringISODate;
        dateOfVaccine: StringISODate;
    }) => Promise<void>;

    @Action("retrieveVaccineStatusPdf", { namespace: "vaccinationStatus" })
    retrieveVaccineStatusPdf!: (params: {
        phn: string;
        dateOfBirth: StringISODate;
        dateOfVaccine: StringISODate;
    }) => Promise<Report>;

    @Getter("webClient", { namespace: "config" })
    config!: WebClientConfiguration;

    @Getter("vaccinationStatus", { namespace: "vaccinationStatus" })
    status!: VaccinationStatus | undefined;

    @Getter("isLoading", { namespace: "vaccinationStatus" })
    isLoading!: boolean;

    @Getter("error", { namespace: "vaccinationStatus" })
    error!: BannerError | undefined;

    @Getter("statusMessage", { namespace: "vaccinationStatus" })
    statusMessage!: string;

    @Ref("sensitivedocumentDownloadModal")
    readonly sensitivedocumentDownloadModal!: MessageModalComponent;

    private bcsclogo: string = Image06;

    private logger!: ILogger;
    private displayResult = false;
    private isDownloading = false;

    private phn = "";
    private dateOfBirth = "";
    private dateOfVaccine = "";

    private get loadingStatusMessage(): string {
        return this.isDownloading ? "Downloading PDF..." : this.statusMessage;
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
                phn: this.phn.replace(/\D/g, ""),
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

    private download() {
        this.isDownloading = true;
        SnowPlow.trackEvent({
            action: "save_qr",
            text: "vaxcard",
        });
        this.retrieveVaccineStatusPdf({
            phn: this.status?.personalhealthnumber || "",
            dateOfBirth: new DateWrapper(this.status?.birthdate || "").format(
                "yyyy-MM-dd"
            ),
            dateOfVaccine: new DateWrapper(
                this.status?.vaccinedate || ""
            ).format("yyyy-MM-dd"),
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
                this.logger.error(`Error retrieving vaccine card PDF: ${err}`);
            })
            .finally(() => {
                this.isDownloading = false;
            });
    }

    private created() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
    }

    private get phnMask(): Mask {
        return phnMask;
    }
}
</script>

<template>
    <div class="background flex-grow-1 d-flex flex-column">
        <loading
            :is-loading="isLoading || isDownloading"
            :text="loadingStatusMessage"
        />
        <div class="header">
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
                <vaccine-card :status="status" :error="error" />
                <div
                    v-if="downloadButtonShown"
                    class="actions p-3 d-flex justify-content-between"
                >
                    <hg-button
                        variant="primary"
                        class="ml-3"
                        @click="showSensitiveDocumentDownloadModal()"
                    >
                        Save a Copy
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
        <div v-else class="flex-grow-1 d-flex flex-column">
            <form
                class="
                    vaccine-card-form
                    flex-grow-1
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
                <div class="container my-2 my-sm-5 px-0 px-sm-5">
                    <div v-if="error !== undefined">
                        <b-alert
                            variant="danger"
                            class="no-print mb-3 p-3"
                            :show="error !== undefined"
                            dismissible
                        >
                            <h2 class="h4">Our Apologies</h2>
                            <div
                                data-testid="errorTextDescription"
                                class="pl-4"
                            >
                                We've found an issue and the Health Gateway team
                                is working hard to fix it.
                            </div>
                        </b-alert>
                    </div>
                    <h2 class="vaccine-card-form-title text-center pb-3 mb-4">
                        Access Your BC Vaccine Card
                    </h2>
                    <p class="mb-4">
                        To access your BC Vaccine Card, please provide:
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
                                >
                                    Personal Health Number is required.
                                </b-form-invalid-feedback>
                                <b-form-invalid-feedback
                                    v-else-if="!$v.phn.formatted"
                                    aria-label="Invalid Personal Health Number"
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
                    <hg-button
                        id="privacy-statement"
                        aria-label="Privacy Statement"
                        href="#"
                        tabindex="0"
                        variant="link"
                        class="shadow-none p-0"
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
                        or 778-698-5849 if you have any questions about this
                        collection.
                    </b-popover>
                    <b-row class="mt-3 justify-content-between">
                        <b-col cols="5">
                            <hg-button
                                variant="secondary"
                                aria-label="Cancel"
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
                                :disabled="isLoading"
                                class="w-100"
                            >
                                Enter
                            </hg-button>
                        </b-col>
                    </b-row>
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
                        Call: 1-833-838-2323 (toll free)
                    </hg-button>
                </div>
                <div class="my-3">
                    <hg-button variant="secondary" href="tel:711">
                        Telephone for the deaf: Dial 711
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

    .actions {
        border-bottom-left-radius: 0.25rem;
        border-bottom-right-radius: 0.25rem;
    }
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
