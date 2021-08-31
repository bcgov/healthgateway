<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faInfoCircle } from "@fortawesome/free-solid-svg-icons";
import Vue from "vue";
import { Component, Watch } from "vue-property-decorator";
import { required } from "vuelidate/lib/validators";
import { Validation } from "vuelidate/vuelidate";
import { Action, Getter } from "vuex-class";

import Image06 from "@/assets/images/landing/006-BCServicesCardLogo.png";
import DatePickerComponent from "@/components/datePicker.vue";
import ErrorCardComponent from "@/components/errorCard.vue";
import LoadingComponent from "@/components/loading.vue";
import VaccinationStatusResultComponent from "@/components/vaccinationStatusResult.vue";
import BannerError from "@/models/bannerError";
import { DateWrapper, StringISODate } from "@/models/dateWrapper";
import VaccinationStatus from "@/models/vaccinationStatus";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import { ILogger } from "@/services/interfaces";
import PHNValidator from "@/utility/phnValidator";

library.add(faInfoCircle);

const validPersonalHealthNumber = (value: string): boolean => {
    var phn = value.replace(/\D/g, "");
    return PHNValidator.IsValid(phn);
};

@Component({
    components: {
        "vaccination-status-result": VaccinationStatusResultComponent,
        "date-picker": DatePickerComponent,
        "error-card": ErrorCardComponent,
        LoadingComponent,
    },
})
export default class VaccinationStatusView extends Vue {
    @Action("retrieve", { namespace: "vaccinationStatus" })
    retrieveVaccinationStatus!: (params: {
        phn: string;
        dateOfBirth: StringISODate;
    }) => Promise<void>;

    @Getter("vaccinationStatus", { namespace: "vaccinationStatus" })
    status!: VaccinationStatus | undefined;

    @Getter("isLoading", { namespace: "vaccinationStatus" })
    isLoading!: boolean;

    @Getter("error", { namespace: "vaccinationStatus" })
    error!: BannerError | undefined;

    @Getter("statusMessage", { namespace: "vaccinationStatus" })
    statusMessage!: string;

    private bcsclogo: string = Image06;

    private logger!: ILogger;
    private displayResult = false;

    private phn = "";
    private dateOfBirth = "";

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
            this.retrieveVaccinationStatus({
                phn: this.phn,
                dateOfBirth: this.dateOfBirth,
            })
                .then(() => {
                    this.logger.debug("Vaccination status retrieved");
                })
                .catch((err) => {
                    this.logger.error(
                        `Error retrieving vaccination status: ${err}`
                    );
                });
        }
    }

    private created() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
    }
}
</script>

<template>
    <div class="background flex-grow-1 d-flex flex-column">
        <LoadingComponent :is-loading="isLoading" :text="statusMessage" />
        <div class="header">
            <img
                class="img-fluid m-3"
                src="@/assets/images/gov/bcid-logo-rev-en.svg"
                width="181"
                alt="BC Mark"
            />
        </div>
        <vaccination-status-result v-if="displayResult" />
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
                            class="no-print mb-3"
                            :show="error !== undefined"
                            dismissible
                        >
                            <h4>{{ error.title }}</h4>
                            <h6>{{ error.errorCode }}</h6>
                            <div class="pl-4">
                                <p data-testid="errorTextDescription">
                                    {{ error.description }}
                                </p>
                                <p data-testid="errorTextDetails">
                                    {{ error.detail }}
                                </p>
                                <p
                                    v-if="error.traceId"
                                    data-testid="errorSupportDetails"
                                >
                                    If this issue persists, contact
                                    HealthGateway@gov.bc.ca and provide
                                    <span class="trace-id">{{
                                        error.traceId
                                    }}</span>
                                </p>
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
                                    :state="isValid($v.phn)"
                                    @blur="$v.phn.$touch()"
                                />
                                <b-form-invalid-feedback
                                    v-if="!$v.phn.required"
                                >
                                    Personal Health Number is required.
                                </b-form-invalid-feedback>
                                <b-form-invalid-feedback
                                    v-else-if="!$v.phn.formatted"
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
                                    :state="isValid($v.dateOfBirth)"
                                    @blur="$v.dateOfBirth.$touch()"
                                />
                                <b-form-invalid-feedback
                                    v-if="
                                        $v.dateOfBirth.$dirty &&
                                        !$v.dateOfBirth.required
                                    "
                                    force-show
                                >
                                    A valid date of birth is required.
                                </b-form-invalid-feedback>
                                <b-form-invalid-feedback
                                    v-else-if="
                                        $v.dateOfBirth.$dirty &&
                                        !$v.dateOfBirth.maxValue
                                    "
                                    force-show
                                >
                                    Date of birth must be before today.
                                </b-form-invalid-feedback>
                            </b-form-group>
                        </b-col>
                    </b-row>
                    <hg-button
                        id="privacy-statement"
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
                        <hg-button variant="secondary" class="mt-3 mr-2" to="/">
                            Cancel
                        </hg-button>
                        <hg-button
                            variant="primary"
                            type="submit"
                            :disabled="isLoading"
                            class="mt-3"
                        >
                            Enter
                        </hg-button>
                    </div>
                    <div>
                        <h3 class="my-5">OR</h3>
                        <h4 class="my-3">Already a Health Gateway user?</h4>
                        <router-link to="/login">
                            <hg-button
                                id="btnLogin"
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
