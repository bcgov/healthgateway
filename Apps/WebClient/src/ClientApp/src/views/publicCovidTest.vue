<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faInfoCircle } from "@fortawesome/free-solid-svg-icons";
import Vue from "vue";
import { Component, Watch } from "vue-property-decorator";
import { required } from "vuelidate/lib/validators";
import { Validation } from "vuelidate/vuelidate";
import { Action, Getter } from "vuex-class";

import Image06 from "@/assets/images/landing/006-BCServicesCardLogo.png";
import ErrorCardComponent from "@/components/errorCard.vue";
import LoadingComponent from "@/components/loading.vue";
import HgDateDropdownComponent from "@/components/shared/hgDateDropdown.vue";
import BannerError from "@/models/bannerError";
import type { WebClientConfiguration } from "@/models/configData";
import {
    DateWrapper,
    StringISODate,
    StringISODateTime,
} from "@/models/dateWrapper";
import {
    PublicCovidTestRecord,
    PublicCovidTestResponseResult,
} from "@/models/laboratory";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import router from "@/router";
import { ILogger } from "@/services/interfaces";
import { Mask, phnMask } from "@/utility/masks";
import PHNValidator from "@/utility/phnValidator";

library.add(faInfoCircle);

const validPersonalHealthNumber = (value: string): boolean => {
    var phn = value.replace(/ /g, "");
    return PHNValidator.IsValid(phn);
};

@Component({
    components: {
        "error-card": ErrorCardComponent,
        loading: LoadingComponent,
        "hg-date-dropdown": HgDateDropdownComponent,
    },
})
export default class PublicCovidTestView extends Vue {
    @Action("retrievePublicCovidTests", { namespace: "laboratory" })
    retrievePublicCovidTests!: (params: {
        phn: string;
        dateOfBirth: StringISODate;
        collectionDate: StringISODate;
    }) => Promise<void>;

    @Action("resetPublicCovidTestResponseResult", { namespace: "laboratory" })
    resetPublicCovidTestResponseResult!: () => Promise<void>;

    @Getter("webClient", { namespace: "config" })
    config!: WebClientConfiguration;

    @Getter("publicCovidTestResponseResult", { namespace: "laboratory" })
    publicCovidTestResponseResult!: PublicCovidTestResponseResult | undefined;

    @Getter("isPublicCovidTestResponseResultLoading", {
        namespace: "laboratory",
    })
    isPublicCovidTestResponseResultLoading!: boolean;

    @Getter("publicCovidTestResponseResultError", { namespace: "laboratory" })
    publicCovidTestResponseResultError!: BannerError | undefined;

    @Getter("publicCovidTestResponseResultStatusMessage", {
        namespace: "laboratory",
    })
    publicCovidTestResponseResultStatusMessage!: string;

    private bcsclogo: string = Image06;

    private logger!: ILogger;
    private displayResult = false;
    private isLoadingCovidTests = false;

    private phn = "";
    private dateOfBirth = "";
    private dateOfVaccine = "";

    private get publicCovidTests(): PublicCovidTestRecord[] | undefined {
        return this.publicCovidTestResponseResult?.records;
    }

    public get isLoading(): boolean {
        return this.isPublicCovidTestResponseResultLoading;
    }

    private get loadingStatusMessage(): string {
        if (this.isLoadingCovidTests) {
            return "Gathering your covid tests....";
        }
        if (this.isPublicCovidTestResponseResultLoading) {
            return this.publicCovidTestResponseResultStatusMessage;
        } else {
            return "";
        }
    }

    private get phnMask(): Mask {
        return phnMask;
    }

    private get patientDisplayName(): string | undefined | null {
        return this.publicCovidTestResponseResult?.records[0]
            ?.patientDisplayName;
    }

    private cancel() {
        // Reset store module in case there are validation errors
        this.resetPublicCovidTestResponseResult();
        router.push("/");
    }

    private checkAnotherTest() {
        this.displayResult = false;
        this.phn = "";
        this.dateOfBirth = "";
        this.dateOfVaccine = "";

        // Reset input components when changing between div tags
        this.$nextTick(() => {
            this.$v.$reset();
        });
        window.scrollTo(0, 0);
    }

    private created() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
    }

    private formatDate(date: StringISODateTime): string {
        if (date) {
            const dateWrapper = new DateWrapper(date);
            const dateString = dateWrapper.format("yyyy-MMM-dd");
            const timeString = dateWrapper.format("t").replace(" ", "\u00A0");
            return `${dateString}, ${timeString}`;
        }
        return "";
    }

    private handleSubmit() {
        this.$v.$touch();
        if (!this.$v.$invalid) {
            this.isLoadingCovidTests = true;
            this.retrievePublicCovidTests({
                phn: this.phn.replace(/ /g, ""),
                dateOfBirth: this.dateOfBirth,
                collectionDate: this.dateOfVaccine,
            })
                .then(() => {
                    this.logger.debug(
                        "Public Covid Tests retrieved from store"
                    );
                })
                .catch((err) => {
                    this.logger.error(
                        `Error retrieving Public Covid Tests from store: ${err}`
                    );
                })
                .finally(() => {
                    this.isLoadingCovidTests = false;
                });
        }
    }

    private getClass(outcome: string): string[] {
        switch (outcome?.toUpperCase()) {
            case "NEGATIVE":
                return ["text-success"];
            case "POSITIVE":
                return ["text-danger"];
            default:
                return [];
        }
    }

    private isStringEmpty(param: string): boolean {
        if (param) {
            return true;
        }
        return false;
    }

    private isValid(param: Validation): boolean | undefined {
        return param.$dirty ? !param.$invalid : undefined;
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

    @Watch("publicCovidTestResponseResult")
    private onPublicCovidTestResponseResultChange() {
        if (this.publicCovidTestResponseResult?.loaded) {
            this.displayResult = true;
        }
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
        <div
            v-if="displayResult"
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
                    <h2
                        data-testid="public-covid-test-result-form-title"
                        class="vaccine-card-form-title text-center pb-3 mb-4"
                    >
                        Your COVID-19 test result
                    </h2>
                    <b-row>
                        <b-col>
                            <strong>Name: </strong> {{ patientDisplayName }}
                        </b-col>
                    </b-row>
                    <div
                        v-for="(publicCovidTest, index) in publicCovidTests"
                        :key="index"
                        class="covid-test-result mt-2 px-2 py-2"
                    >
                        <b-row class="px-2 pt-1">
                            <b-col :data-testid="'test-type-' + (index + 1)">
                                <strong>Test Type: </strong>
                                {{ publicCovidTest.testType }}
                            </b-col>
                        </b-row>
                        <b-row class="px-2 pt-1">
                            <b-col>
                                <strong>Result: </strong>
                                <span
                                    :data-testid="
                                        'test-outcome-span-' + (index + 1)
                                    "
                                    v-bind="$attrs"
                                    :class="
                                        getClass(publicCovidTest.testOutcome)
                                    "
                                >
                                    <strong
                                        :data-testid="
                                            'test-outcome-' + (index + 1)
                                        "
                                        >{{
                                            publicCovidTest.testOutcome
                                        }}</strong
                                    ></span
                                >
                            </b-col>
                        </b-row>
                        <b-row class="px-2 pt-1">
                            <b-col
                                :data-testid="'collection-date-' + (index + 1)"
                            >
                                <strong>Collection Date: </strong>
                                {{
                                    formatDate(
                                        publicCovidTest.collectionDateTime
                                    )
                                }}
                            </b-col>
                        </b-row>
                        <b-row class="px-2 pt-1">
                            <b-col>
                                <strong>Test Status: </strong>
                                {{ publicCovidTest.testStatus }}
                            </b-col>
                        </b-row>
                        <b-row class="px-2 pt-1">
                            <b-col :data-testid="'result-date-' + (index + 1)">
                                <strong>Result Date: </strong>
                                {{ formatDate(publicCovidTest.resultDateTime) }}
                            </b-col>
                        </b-row>
                        <b-row class="px-2 pt-1">
                            <b-col
                                ><strong>Reporting Lab: </strong>
                                {{ publicCovidTest.lab }}
                            </b-col>
                        </b-row>
                        <b-row class="px-2 pt-1">
                            <b-col>
                                <strong>Result Description:</strong>
                            </b-col>
                        </b-row>
                        <b-row class="px-2 pb-1">
                            <b-col>
                                {{ publicCovidTest.resultDescription }}
                            </b-col>
                        </b-row>
                        <b-row
                            v-if="isStringEmpty(publicCovidTest.resultLink)"
                            class="px-2 pb-1"
                        >
                            <b-col>
                                <a
                                    :href="publicCovidTest.resultLink"
                                    :data-testid="'result-link-' + (index + 1)"
                                    target="blank_"
                                    >More Information</a
                                >
                            </b-col>
                        </b-row>
                    </div>
                </div>
                <div class="my-2 my-sm-5 px-0 px-sm-5">
                    <div class="text-center">
                        <b-row class="mt-3 justify-content-center">
                            <b-col cols="8">
                                <hg-button
                                    variant="secondary"
                                    aria-label="Check another test"
                                    data-testid="btnCheckAnotherTest"
                                    class="w-100"
                                    @click="checkAnotherTest()"
                                >
                                    Check another test
                                </hg-button>
                            </b-col>
                        </b-row>
                    </div>
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
        </div>
        <div
            v-else
            class="flex-grow-1 d-flex flex-column justify-content-between"
        >
            <form
                id="publicCovidTestForm"
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
                    <div
                        v-if="publicCovidTestResponseResultError !== undefined"
                    >
                        <b-alert
                            variant="danger"
                            class="mb-3 p-3"
                            show
                            dismissible
                        >
                            <h2 class="h4">
                                {{ publicCovidTestResponseResultError.title }}
                            </h2>
                            <div
                                data-testid="error-text-description"
                                class="pl-4"
                            >
                                {{
                                    publicCovidTestResponseResultError.description
                                }}
                            </div>
                        </b-alert>
                    </div>
                    <h2
                        data-testid="public-covid-test-form-title"
                        class="vaccine-card-form-title text-center pb-3 mb-4"
                    >
                        Get your COVID-19 test result
                    </h2>
                    <p class="mb-4">
                        To get your COVID-19 test result, please provide:
                    </p>
                    <b-row>
                        <b-col>
                            <b-form-group
                                label="Personal Health Number"
                                label-for="phn"
                            >
                                <b-form-input
                                    id="phn"
                                    ref="phnInput"
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
                                label="Date of Vaccine (Any Dose)"
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
                                    aria-label="Date of Vaccine (Any Dose)"
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
                                @click.prevent="cancel"
                            >
                                Cancel
                            </hg-button>
                        </b-col>
                        <b-col cols="5">
                            <hg-button
                                variant="primary"
                                aria-label="Enter"
                                type="submit"
                                :disabled="
                                    isPublicCovidTestResponseResultLoading
                                "
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

.covid-test-result {
    background-color: $soft_background;
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
