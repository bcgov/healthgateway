<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faInfoCircle } from "@fortawesome/free-solid-svg-icons";
import Vue from "vue";
import { Component } from "vue-property-decorator";
import { required } from "vuelidate/lib/validators";
import { Validation } from "vuelidate/vuelidate";
import { Action, Getter } from "vuex-class";

import LoadingComponent from "@/components/loading.vue";
import HgDateDropdownComponent from "@/components/shared/hgDateDropdown.vue";
import { ResultType } from "@/constants/resulttype";
import { DateWrapper } from "@/models/dateWrapper";
import { AuthenticatedRapidTestRequest } from "@/models/laboratory";
import PatientData from "@/models/patientData";
import { ResultError } from "@/models/requestResult";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import { ILaboratoryService, ILogger } from "@/services/interfaces";
import SnowPlow from "@/utility/snowPlow";

library.add(faInfoCircle);

@Component({
    components: {
        LoadingComponent,
        "hg-date-dropdown": HgDateDropdownComponent,
    },
})
export default class AuthenticatedRapidTestComponent extends Vue {
    @Action("getPatientData", { namespace: "user" })
    getPatientData!: () => Promise<void>;

    @Getter("patientData", { namespace: "user" })
    patientData!: PatientData;

    private isLoading = false;
    private isVisible = false;
    private isSuccess = false;
    private errorMessage = "";

    private logger!: ILogger;
    private laboratoryService!: ILaboratoryService;

    private rapidTest: AuthenticatedRapidTestRequest = {
        labSerialNumber: "",
        dateTestTaken: "",
    };

    private resultOptions = [
        { value: true, text: "Positive" },
        { value: false, text: "Negative" },
    ];

    private created() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        this.laboratoryService = container.get<ILaboratoryService>(
            SERVICE_IDENTIFIER.LaboratoryService
        );
        this.isLoading = true;
        this.getPatientData()
            .catch((err) => {
                this.isLoading = false;
                this.hideModal();
                this.logger.error(`Error loading patient data: ${err}`);
            })
            .finally(() => {
                this.isLoading = false;
            });
    }

    private formatDate(date: string | undefined): string {
        return date === undefined
            ? ""
            : DateWrapper.format(date, "yyyy-MMM-dd");
    }

    private formatPatientData(data: string | undefined): string {
        return data === undefined ? "" : data;
    }

    private clear() {
        this.rapidTest = {
            labSerialNumber: "",
            dateTestTaken: "",
        };
        this.isVisible = true;
        this.isSuccess = false;
    }

    public showModal(): void {
        // Reset input components when changing between div tags
        this.resetInput();
        this.clear();
        this.errorMessage = "";
    }

    public hideModal(): void {
        this.$v.$reset();
        this.isVisible = false;
    }

    private resetInput(): void {
        this.$nextTick(() => {
            this.$v.$reset();
        });
    }

    private isValid(param: Validation): boolean | undefined {
        return param.$dirty ? !param.$invalid : undefined;
    }

    private validations() {
        return {
            rapidTest: {
                labSerialNumber: {
                    required: required,
                },
                dateTestTaken: {
                    required: required,
                    maxValue: (value: string) =>
                        new DateWrapper(value).isBefore(new DateWrapper()),
                },
                positive: { required: required },
            },
        };
    }
    private handleSubmit(bvModalEvt: Event) {
        // Prevent modal from closing
        bvModalEvt.preventDefault();
        this.$v.$touch();
        if (!this.$v.$invalid) {
            this.$v.$reset();
            this.submitRapidTest();
        }
    }

    private submitRapidTest() {
        this.isLoading = true;
        SnowPlow.trackEvent({
            action: "submit",
            text: "Autheticate_Rapid_Test",
        });
        this.rapidTest.phn = this.patientData.personalhealthnumber;
        this.laboratoryService
            .postAuthenticatedRapidTest(this.patientData.hdid, this.rapidTest)
            .then((result) => {
                if (result.resultStatus === ResultType.Success) {
                    this.isSuccess = true;
                    this.errorMessage = "";
                    this.logger.debug(`Rapid Test was submitted." ${result} `);
                } else {
                    this.isSuccess = false;
                    this.errorMessage = `${result.resultError?.resultMessage}`;
                    this.logger.debug(
                        `Rapid Test was not submitted." ${result.resultError?.resultMessage} `
                    );
                }
            })
            .catch((err: ResultError) => {
                this.isSuccess = false;
                this.errorMessage = "Errors submitting rapid test";
                this.logger.error(`Errors submitting rapid test : ${err}`);
            })
            .finally(() => {
                this.isLoading = false;
            });
    }

    private get isDisabled(): boolean {
        return this.isSuccess;
    }

    private get showErrorBanner(): boolean {
        return this.errorMessage !== undefined && this.errorMessage.length > 0;
    }

    private get fullName(): string {
        return (
            this.formatPatientData(this.patientData?.firstname) +
            " " +
            this.formatPatientData(this.patientData?.lastname)
        );
    }

    private get birthDate(): string {
        return this.formatDate(this.patientData?.birthdate ?? undefined);
    }
}
</script>

<template>
    <b-modal
        id="covidRapidTestModal"
        v-model="isVisible"
        data-testid="covid-rapid-test-modal"
        content-class="mt-5"
        title="Submit Rapid Test Result"
        size="lg"
        header-bg-variant="primary"
        header-text-variant="light"
        centered
    >
        <div v-if="isSuccess">
            <b-alert
                data-testid="post-rapid-test-success-banner"
                variant="success"
                class="mb-3 p-3"
                show
                dismissible
            >
                <h2 class="h4">
                    Your COVID rapid test result has been submitted.
                </h2>
            </b-alert>
        </div>
        <div>
            <b-alert
                data-testid="post-rapid-test-error-banner"
                variant="danger"
                dismissible
                :show="showErrorBanner"
                @dismissed="errorMessage = ''"
            >
                <h4>Something went wrong!</h4>
                <span>
                    We were unable to submit your rapid test result. Please
                    refresh your browser and try again.
                </span>
            </b-alert>
        </div>
        <b-row class="col-12 mb-2">
            <b-col>
                <b-row>
                    <b-col>
                        <p>Please provide the following:</p>
                    </b-col>
                </b-row>
                <b-row>
                    <b-col>
                        <b-form-group label="Name:" label-for="patientName">
                            <strong
                                ><label data-testid="fullname-label">{{
                                    fullName
                                }}</label></strong
                            >
                        </b-form-group>
                    </b-col>
                </b-row>
                <b-row>
                    <b-col>
                        <b-form-group
                            label="Date of Birth:"
                            label-for="dateOfBirth"
                        >
                            <strong
                                ><label data-testid="birthdate-label">{{
                                    birthDate
                                }}</label></strong
                            >
                        </b-form-group>
                    </b-col>
                </b-row>
                <b-row>
                    <b-col>
                        <b-form-group
                            label="Serial Number:"
                            label-for="serialNumber"
                            :state="isValid($v.rapidTest.labSerialNumber)"
                        >
                            <b-form-input
                                id="serialNumber"
                                ref="serialNumber"
                                v-model="rapidTest.labSerialNumber"
                                class="col-lg-4 col-12 mb-2"
                                data-testid="serial-number-input"
                                aria-label="Serial Number"
                                :state="isValid($v.rapidTest.labSerialNumber)"
                                :disabled="isDisabled"
                                @blur="$v.rapidTest.labSerialNumber.$touch()"
                            />
                            <b-form-invalid-feedback
                                :state="isValid($v.rapidTest.labSerialNumber)"
                                aria-label="Invalid Serial Number"
                                data-testid="feedback-serialnumber-isrequired"
                            >
                                Serial Number is required.
                            </b-form-invalid-feedback>
                        </b-form-group>
                    </b-col>
                </b-row>
                <b-row>
                    <b-col class="col-lg-5 col-12">
                        <b-form-group
                            label="Date of Test:"
                            label-for="dateOfRapidTest"
                            :state="isValid($v.rapidTest.dateTestTaken)"
                        >
                            <hg-date-dropdown
                                id="dateOfRapidTest"
                                v-model="rapidTest.dateTestTaken"
                                :state="isValid($v.rapidTest.dateTestTaken)"
                                :allow-future="false"
                                :min-year="2020"
                                :disabled="isDisabled"
                                data-testid="dateOf-rapid-test"
                                aria-label="Date of Rapid Test"
                                @blur="$v.rapidTest.dateTestTaken.$touch()"
                            />
                            <b-form-invalid-feedback
                                v-if="
                                    $v.rapidTest.dateTestTaken.$dirty &&
                                    !$v.rapidTest.dateTestTaken.required
                                "
                                aria-label="Invalid Date of Test"
                                data-testid="feedback-dateof-rapid-isrequired"
                                force-show
                            >
                                A valid date of test is required.
                            </b-form-invalid-feedback>
                        </b-form-group>
                    </b-col>
                </b-row>
                <b-row>
                    <b-col>
                        <b-form-group
                            label="Result:"
                            label-for="resultOptionSelected"
                            :state="isValid($v.rapidTest.positive)"
                        >
                            <b-form-radio-group
                                id="resultOptionSelected"
                                ref="resultOptionSelected"
                                v-model="rapidTest.positive"
                                aria-label="Result"
                                :options="resultOptions"
                                :state="isValid($v.rapidTest.positive)"
                                data-testid="result-selected-option"
                                :disabled="isDisabled"
                                @blur="$v.rapidTest.result.$touch()"
                            >
                                <b-form-invalid-feedback
                                    :state="isValid($v.rapidTest.positive)"
                                    aria-label="Invalid Result Option"
                                    data-testid="feedback-result-option-isrequired"
                                >
                                    Please select one.
                                </b-form-invalid-feedback>
                            </b-form-radio-group>
                        </b-form-group>
                    </b-col>
                </b-row>
            </b-col>
        </b-row>
        <template #modal-footer>
            <b-row>
                <div class="mr-2">
                    <hg-button
                        v-if="!isSuccess"
                        data-testid="rapid-test-cancel-btn"
                        variant="secondary"
                        @click="hideModal"
                        >Cancel</hg-button
                    >
                </div>
                <div>
                    <hg-button
                        v-if="!isSuccess"
                        data-testid="rapid-test-submit-btn"
                        variant="primary"
                        aria-label="Submit"
                        @click="handleSubmit"
                        >Submit</hg-button
                    >
                </div>
                <div>
                    <hg-button
                        v-if="isSuccess"
                        variant="secondary"
                        aria-label="Close"
                        @click="hideModal()"
                    >
                        Close
                    </hg-button>
                </div>
            </b-row>
        </template>
        <LoadingComponent :is-loading="isLoading"></LoadingComponent>
    </b-modal>
</template>
