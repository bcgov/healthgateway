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
import { CustomBannerError } from "@/models/bannerError";
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
import SnowPlow from "@/utility/snowPlow";

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
    publicCovidTestResponseResultError!: CustomBannerError | undefined;

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
    private dateOfCollection = "";

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

    private get haveErrorDetails(): boolean {
        const detail = this.publicCovidTestResponseResultError?.detail;
        return detail ? true : false;
    }

    private cancel() {
        // Reset store module in case there are errors
        this.resetPublicCovidTestResponseResult();
        router.push("/");
    }

    private checkAnotherTest() {
        this.displayResult = false;
        this.phn = "";
        this.dateOfBirth = "";
        this.dateOfCollection = "";

        // Reset input components when changing between div tags
        this.$nextTick(() => {
            this.$v.$reset();
        });
        // Depending on where button is clicked on page, we need to ensure that top of page is displayed on the changed DIV
        window.scrollTo(0, 0);
    }

    private created() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
    }

    private formatDate(date: StringISODateTime): string {
        if (date) {
            const dateWrapper = new DateWrapper(date, { hasTime: true });
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
            SnowPlow.trackEvent({
                action: "view",
                text: "public_covid_test",
            });
            this.retrievePublicCovidTests({
                phn: this.phn.replace(/ /g, ""),
                dateOfBirth: this.dateOfBirth,
                collectionDate: this.dateOfCollection,
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

    private getPeriod(link: string, length: number, index: number): string {
        if (link) {
            if (this.isLastRow(length, index)) {
                return ".";
            }
            return "";
        }
        return "";
    }

    private isLastRow(length: number, index: number): boolean {
        const indexSize = length - 1;
        return indexSize == index;
    }

    private showLink(link: string, length: number, index: number): boolean {
        if (link) {
            return this.isLastRow(length, index);
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
            dateOfCollection: {
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
            <img
                class="img-fluid m-3"
                src="@/assets/images/gov/bcid-logo-rev-en.svg"
                width="152"
                alt="BC Mark"
                @mousedown="cancel"
            />
        </div>
        <div
            v-if="displayResult"
            class="flex-grow-1 d-flex flex-column justify-content-between"
        >
            <form
                class="vaccine-card-form bg-white rounded shadow m-2 m-sm-3 py-3 px-3 px-sm-5 align-self-center"
                @submit.prevent="handleSubmit"
            >
                <div class="my-2 my-sm-5 px-0 px-sm-5">
                    <h2
                        data-testid="public-covid-test-result-form-title"
                        class="vaccine-card-form-title text-center pb-3 mb-4"
                    >
                        Your COVID-19 Test Result
                    </h2>
                    <div
                        v-if="publicCovidTests.length"
                        data-testid="public-display-name"
                    >
                        <b-row>
                            <b-col>
                                <strong>Name: </strong> {{ patientDisplayName }}
                            </b-col>
                        </b-row>
                    </div>
                    <div
                        v-for="(publicCovidTest, index) in publicCovidTests"
                        :key="index"
                        class="covid-test-result mt-2 px-2 py-2"
                    >
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
                            <b-col :data-testid="'test-type-' + (index + 1)">
                                <strong>Test Type: </strong>
                                {{ publicCovidTest.testType }}
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
                            <b-col data-testid="test-status">
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
                            <b-col data-testid="reporting-lab"
                                ><strong>Reporting Lab: </strong>
                                {{ publicCovidTest.lab }}
                            </b-col>
                        </b-row>
                        <b-row class="px-2 pt-1 pb-2">
                            <b-col data-testid="result-description">
                                <strong>Result Description:</strong>
                            </b-col>
                        </b-row>
                        <div
                            v-for="(
                                resultDescription, resultDescriptionIndex
                            ) in publicCovidTest.resultDescription"
                            :key="resultDescriptionIndex"
                        >
                            <b-row class="px-2 pb-2">
                                <b-col>
                                    {{ resultDescription }}
                                    <a
                                        v-if="
                                            showLink(
                                                publicCovidTest.resultLink,
                                                publicCovidTest
                                                    .resultDescription.length,
                                                resultDescriptionIndex
                                            )
                                        "
                                        :href="publicCovidTest.resultLink"
                                        :data-testid="
                                            'result-link-' +
                                            (resultDescriptionIndex + 1)
                                        "
                                        target="blank_"
                                        >this page</a
                                    >
                                    <span>{{
                                        getPeriod(
                                            publicCovidTest.resultLink,
                                            publicCovidTest.resultDescription
                                                .length,
                                            resultDescriptionIndex
                                        )
                                    }}</span>
                                </b-col>
                            </b-row>
                        </div>
                    </div>
                </div>
                <div class="my-2 my-sm-5 px-0 px-sm-5">
                    <div class="text-center">
                        <b-row class="mt-3 justify-content-center">
                            <b-col cols="8">
                                <hg-button
                                    variant="secondary"
                                    aria-label="Check Another Test"
                                    data-testid="btnCheckAnotherTest"
                                    class="w-100"
                                    @click="checkAnotherTest()"
                                >
                                    Check Another Test
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
                class="vaccine-card-form bg-white rounded shadow m-2 m-sm-3 py-3 px-3 px-sm-5 align-self-center"
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
                                <div :class="{ 'mb-3': haveErrorDetails }">
                                    {{
                                        publicCovidTestResponseResultError.description
                                    }}
                                </div>
                                <div>
                                    {{
                                        publicCovidTestResponseResultError.detail
                                    }}
                                </div>
                            </div>
                        </b-alert>
                    </div>
                    <h2
                        data-testid="public-covid-test-form-title"
                        class="vaccine-card-form-title text-center pb-3 mb-4"
                    >
                        Get Your COVID-19 Test Result
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
                                label="Date of COVID-19 Test"
                                label-for="dateOfCollection"
                                :state="isValid($v.dateOfCollection)"
                            >
                                <hg-date-dropdown
                                    id="dateOfCollection"
                                    v-model="dateOfCollection"
                                    :state="isValid($v.dateOfCollection)"
                                    :allow-future="false"
                                    :min-year="2020"
                                    data-testid="dateOfCollectionInput"
                                    aria-label="Date of COVID-19 Test"
                                    @blur="$v.dateOfBirth.$touch()"
                                />
                                <b-form-invalid-feedback
                                    v-if="
                                        $v.dateOfCollection.$dirty &&
                                        !$v.dateOfCollection.required
                                    "
                                    aria-label="Invalid Collection Date"
                                    data-testid="feedbackCollectionDateIsRequired"
                                    force-show
                                >
                                    A valid collection date is required.
                                </b-form-invalid-feedback>
                                <b-form-invalid-feedback
                                    v-else-if="
                                        $v.dateOfCollection.$dirty &&
                                        !$v.dateOfCollection.maxValue
                                    "
                                    aria-label="Invalid Collection Date"
                                    force-show
                                >
                                    Collection Date must be in the past.
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
                                @mousedown="cancel"
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
                        your COVID-19 test result under s. 26(c) of the
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
