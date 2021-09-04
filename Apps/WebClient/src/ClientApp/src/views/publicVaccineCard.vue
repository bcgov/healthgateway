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
import VaccineCardComponent from "@/components/vaccineCard.vue";
import { VaccinationState } from "@/constants/vaccinationState";
import BannerError from "@/models/bannerError";
import { DateWrapper, StringISODate } from "@/models/dateWrapper";
import Report from "@/models/report";
import VaccinationStatus from "@/models/vaccinationStatus";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import { ILogger } from "@/services/interfaces";
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

    private get downloadButtonEnabled(): boolean {
        return (
            this.status?.state === VaccinationState.PartiallyVaccinated ||
            this.status?.state === VaccinationState.FullyVaccinated
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
                phn: this.phn,
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
}
</script>

<template>
    <div class="background flex-grow-1 d-flex flex-column">
        <loading
            :is-loading="isLoading || isDownloading"
            :text="loadingStatusMessage"
        />
        <div class="header">
            <img
                class="img-fluid m-3"
                src="@/assets/images/gov/bcid-logo-rev-en.svg"
                width="181"
                alt="BC Mark"
            />
        </div>
        <div
            v-if="displayResult"
            class="vaccine-card align-self-center w-100 m-3 bg-white rounded"
        >
            <vaccine-card :status="status" :error="error" />
            <div class="actions m-3 d-flex justify-content-between">
                <hg-button variant="secondary" to="/">Done</hg-button>
                <hg-button
                    v-if="downloadButtonEnabled"
                    variant="primary"
                    class="ml-3"
                    @click="showSensitiveDocumentDownloadModal()"
                >
                    Save a Copy
                </hg-button>
            </div>
            <MessageModalComponent
                ref="sensitivedocumentDownloadModal"
                title="Sensitive Document Download"
                message="The file that you are downloading contains personal information. If you are on a public computer, please ensure that the file is deleted before you log off."
                @submit="download"
            />
        </div>
        <div v-else class="flex-grow-1 d-flex flex-column">
            <div class="vaccine-card-banner p-3">
                <div class="container d-flex align-items-center">
                    <img
                        src="@/assets/images/vaccine-card/vaccine-card-banner-image.svg"
                        alt="Vaccine Card Logo"
                        class="mr-2"
                    />
                    <h3 class="m-0">BC Vaccine Card</h3>
                </div>
            </div>
            <form class="bg-white flex-grow-1" @submit.prevent="handleSubmit">
                <div class="container py-3">
                    <div v-if="error !== undefined">
                        <b-alert
                            variant="danger"
                            class="no-print mb-3 p-3"
                            :show="error !== undefined"
                            dismissible
                        >
                            <h4>Our Apologies</h4>
                            <div
                                data-testid="errorTextDescription"
                                class="pl-4"
                            >
                                We've found an issue and the Health Gateway team
                                is working hard to fix it.
                            </div>
                        </b-alert>
                    </div>
                    <h4 class="mb-3">You must provide:</h4>
                    <b-row no-gutters>
                        <b-col cols="auto">
                            <b-form-group
                                label="Personal Health Number"
                                label-for="phn"
                            >
                                <b-form-input
                                    id="phn"
                                    v-model="phn"
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
                    <b-row no-gutters>
                        <b-col cols="auto">
                            <b-form-group
                                label="Date of Birth"
                                label-for="dateOfBirth"
                                :state="isValid($v.dateOfBirth)"
                            >
                                <date-picker
                                    id="dateOfBirth"
                                    v-model="dateOfBirth"
                                    data-testid="dateOfBirthInput"
                                    aria-label="Date of Birth"
                                    :state="isValid($v.dateOfBirth)"
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
                    <b-row no-gutters>
                        <b-col cols="auto">
                            <b-form-group
                                label="Date of Vaccine (Dose 1 or Dose 2)"
                                label-for="dateOfVaccine"
                                :state="isValid($v.dateOfVaccine)"
                            >
                                <date-picker
                                    id="dateOfVaccine"
                                    v-model="dateOfVaccine"
                                    data-testid="dateOfVaccineInput"
                                    aria-label="Date of Vaccine (Dose 1 or Dose 2)"
                                    :state="isValid($v.dateOfVaccine)"
                                    @blur="$v.dateOfVaccine.$touch()"
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
                    <div>
                        <hg-button
                            variant="secondary"
                            aria-label="Cancel"
                            class="mt-3 mr-2"
                            to="/"
                        >
                            Cancel
                        </hg-button>
                        <hg-button
                            variant="primary"
                            aria-label="Enter"
                            type="submit"
                            :disabled="isLoading"
                            class="mt-3"
                        >
                            Enter
                        </hg-button>
                    </div>
                    <div class="my-5">
                        <h3 class="my-5">OR</h3>
                        <h4 class="my-3">Already a Health Gateway user?</h4>
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
                    <hr />
                    <div class="mt-4">
                        <h3 class="mb-3">Help in other languages</h3>
                        <p>
                            <span>
                                Talk to someone on the phone. Get support in
                                140+ languages, including:
                            </span>
                            <br aria-hidden="true" />
                            <br aria-hidden="true" />
                            <span lang="zh">國粵語</span> |
                            <span lang="pa">ਅਨੁਵਾਦ ਸਰਵਿਸਿਜ਼</span> |
                            <span lang="ar">خدمات-ت-رج-م-ه؟</span> |
                            <span lang="fr">Français</span> |
                            <span lang="es">Español</span>
                        </p>
                        <p>
                            <strong>
                                Service is available every day: 7 am to 7 pm or
                                9 am to 5 pm on holidays.
                            </strong>
                        </p>
                        <div class="my-3">
                            <hg-button
                                variant="secondary"
                                href="tel:+18338382323"
                            >
                                Call: 1-833-838-2323 (toll free)
                            </hg-button>
                        </div>
                        <div class="my-3">
                            <hg-button variant="secondary" href="tel:711">
                                Telephone for the deaf: Dial 711
                            </hg-button>
                        </div>
                        <p class="text-muted">
                            Standard message and data rates may apply.
                        </p>
                    </div>
                </div>
            </form>
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

.trace-id {
    overflow-wrap: anywhere;
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
