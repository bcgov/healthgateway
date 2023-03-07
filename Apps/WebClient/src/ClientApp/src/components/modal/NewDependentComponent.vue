<script lang="ts">
import { Duration } from "luxon";
import Vue from "vue";
import { Component, Emit } from "vue-property-decorator";
import { minLength, required, sameAs } from "vuelidate/lib/validators";
import { Validation } from "vuelidate/vuelidate";
import { Action, Getter } from "vuex-class";

import DatePickerComponent from "@/components/DatePickerComponent.vue";
import LoadingComponent from "@/components/LoadingComponent.vue";
import TooManyRequestsComponent from "@/components/TooManyRequestsComponent.vue";
import AddDependentRequest from "@/models/addDependentRequest";
import type { WebClientConfiguration } from "@/models/configData";
import { DateWrapper } from "@/models/dateWrapper";
import { ResultError } from "@/models/errors";
import User from "@/models/user";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { IDependentService } from "@/services/interfaces";
import PHNValidator from "@/utility/phnValidator";

const validPersonalHealthNumber = (value: string) => {
    let phn = value.replace(/\D/g, "");
    return PHNValidator.IsValid(phn);
};

// eslint-disable-next-line @typescript-eslint/no-explicit-any
const options: any = {
    components: {
        LoadingComponent,
        DatePickerComponent,
        TooManyRequestsComponent,
    },
};

@Component(options)
export default class NewDependentComponent extends Vue {
    @Getter("user", { namespace: "user" }) user!: User;

    @Getter("webClient", { namespace: "config" })
    webClientConfig!: WebClientConfiguration;

    @Action("setTooManyRequestsError", { namespace: "errorBanner" })
    setTooManyRequestsError!: (params: { key: string }) => void;

    private dependentService!: IDependentService;
    private isVisible = false;
    private isLoading = true;
    private isDateOfBirthValidDate = true;
    private errorMessage = "";
    private dependent: AddDependentRequest = {
        firstName: "",
        lastName: "",
        dateOfBirth: "",
        PHN: "",
    };
    private accepted = false;
    private maxDate = new DateWrapper();

    private validations(): unknown {
        return {
            dependent: {
                firstName: {
                    required,
                },
                lastName: {
                    required,
                },
                dateOfBirth: {
                    required,
                    minLength: minLength(10),
                    minValue: (value: string) =>
                        new DateWrapper(value).isAfter(this.minBirthdate),
                    maxValue: (value: string) =>
                        new DateWrapper(value).isBefore(new DateWrapper()),
                },
                PHN: {
                    required,
                    minLength: minLength(12),
                    validPersonalHealthNumber,
                },
            },
            accepted: { isChecked: sameAs(() => true) },
        };
    }

    private isValid(param: Validation): boolean | undefined {
        return param.$dirty ? !param.$invalid : undefined;
    }

    private get minBirthdate(): DateWrapper {
        return new DateWrapper().subtract(
            Duration.fromObject({ years: this.webClientConfig.maxDependentAge })
        );
    }

    public showModal(): void {
        this.isVisible = true;
    }

    public hideModal(): void {
        this.isVisible = false;
    }

    private created(): void {
        this.dependentService = container.get<IDependentService>(
            SERVICE_IDENTIFIER.DependentService
        );
        this.isLoading = false;
    }

    private handleOk(bvModalEvt: Event): void {
        // Prevent modal from closing
        bvModalEvt.preventDefault();
        this.$v.$touch();
        if (!this.$v.$invalid && this.isDateOfBirthValidDate) {
            this.$v.$reset();
            this.addDependent();
        }
    }

    private addDependent(): void {
        this.dependentService
            .addDependent(this.user.hdid, {
                ...this.dependent,
                PHN: this.dependent.PHN.replace(/\D/g, ""),
            })
            .then(() => {
                this.errorMessage = "";
                this.handleSubmit();
            })
            .catch((err: ResultError) => {
                if (err.statusCode === 429) {
                    this.setTooManyRequestsError({ key: "addDependentModal" });
                } else {
                    this.errorMessage = err.resultMessage;
                }
            });
    }

    @Emit()
    private handleSubmit(): void {
        // Hide the modal manually
        this.$nextTick(() => this.hideModal());
    }

    private clear(): void {
        this.dependent = {
            firstName: "",
            lastName: "",
            dateOfBirth: "",
            PHN: "",
        };
        this.accepted = false;
        this.errorMessage = "";
        this.$v.$reset();
    }
}
</script>

<template>
    <b-modal
        id="new-dependent-modal"
        v-model="isVisible"
        data-testid="newDependentModal"
        content-class="mt-5"
        title="Dependent Registration"
        size="lg"
        header-bg-variant="primary"
        header-text-variant="light"
        centered
        @hidden="clear"
    >
        <TooManyRequestsComponent location="addDependentModal" />
        <b-alert
            data-testid="dependentErrorBanner"
            variant="danger"
            class="no-print"
            :show="!!errorMessage"
        >
            <p data-testid="dependentErrorText">{{ errorMessage }}</p>
            <span>
                If you continue to have issues, please contact
                HealthGateway@gov.bc.ca.
            </span>
        </b-alert>
        <b-row>
            <b-col>
                <form>
                    <b-row data-testid="newDependentModalText">
                        <b-col>
                            <b-row>
                                <b-col class="col-12 col-md-4 mb-2">
                                    <label for="firstName">Given Names</label>
                                    <b-form-input
                                        id="firstName"
                                        v-model.trim="dependent.firstName"
                                        data-testid="firstNameInput"
                                        class="dependentCardInput"
                                        placeholder="John Alexander"
                                        :state="isValid($v.dependent.firstName)"
                                        @blur.native="
                                            $v.dependent.firstName.$touch()
                                        "
                                    ></b-form-input>
                                    <b-form-invalid-feedback
                                        :state="isValid($v.dependent.firstName)"
                                    >
                                        Given names are required
                                    </b-form-invalid-feedback>
                                </b-col>
                                <b-col class="col-12 col-md-4 mb-2">
                                    <label for="lastName">Last Name</label>
                                    <b-form-input
                                        id="lastName"
                                        v-model.trim="dependent.lastName"
                                        data-testid="lastNameInput"
                                        class="dependentCardInput"
                                        placeholder="Doe"
                                        :state="isValid($v.dependent.lastName)"
                                        @blur.native="
                                            $v.dependent.lastName.$touch()
                                        "
                                    ></b-form-input>
                                    <b-form-invalid-feedback
                                        :state="isValid($v.dependent.lastName)"
                                    >
                                        Last name is required
                                    </b-form-invalid-feedback>
                                </b-col>
                                <b-col class="col-12 col-md-4 mb-2">
                                    <label for="dateOfBirth"
                                        >Date of Birth</label
                                    >
                                    <DatePickerComponent
                                        id="dateOfBirth"
                                        v-model="dependent.dateOfBirth"
                                        data-testid="dateOfBirthInput"
                                        :max-date="maxDate"
                                        :state="
                                            isValid($v.dependent.dateOfBirth)
                                        "
                                        @blur.native="
                                            $v.dependent.dateOfBirth.$touch()
                                        "
                                        @change.native="
                                            $v.dependent.dateOfBirth.$touch()
                                        "
                                        @is-date-valid="
                                            isDateOfBirthValidDate = $event
                                        "
                                    />
                                    <b-form-invalid-feedback
                                        :state="
                                            !$v.dependent.dateOfBirth.$dirty ||
                                            ($v.dependent.dateOfBirth
                                                .required &&
                                                $v.dependent.dateOfBirth
                                                    .minLength &&
                                                $v.dependent.dateOfBirth
                                                    .maxValue)
                                        "
                                    >
                                        Invalid date
                                    </b-form-invalid-feedback>
                                    <b-form-invalid-feedback
                                        :state="
                                            !$v.dependent.dateOfBirth.$dirty ||
                                            $v.dependent.dateOfBirth.minValue
                                        "
                                    >
                                        Dependent must be under the age of
                                        {{ webClientConfig.maxDependentAge }}
                                    </b-form-invalid-feedback>
                                </b-col>
                            </b-row>
                            <b-row class="mb-2">
                                <b-col class="col-12 col-md-6 mb-2">
                                    <label for="phn">PHN</label>
                                    <b-form-input
                                        id="phn"
                                        v-model="dependent.PHN"
                                        v-mask="'#### ### ###'"
                                        data-testid="phnInput"
                                        class="dependentCardInput"
                                        placeholder="1234 567 890"
                                        :state="isValid($v.dependent.PHN)"
                                        @blur.native="$v.dependent.PHN.$touch()"
                                    ></b-form-input>
                                    <b-form-invalid-feedback
                                        :state="isValid($v.dependent.PHN)"
                                    >
                                        Valid PHN is required
                                    </b-form-invalid-feedback>
                                </b-col>
                            </b-row>
                            <b-row class="mb-2">
                                <b-col>
                                    <b-checkbox
                                        id="termsCheckbox"
                                        v-model="accepted"
                                        data-testid="termsCheckbox"
                                        :state="isValid($v.accepted)"
                                    >
                                        <p>
                                            By providing the child’s name, date
                                            of birth, and personal health
                                            number, I declare that I am the
                                            child’s guardian and that I have the
                                            authority to request and receive
                                            health information respecting the
                                            child from third parties.
                                        </p>
                                        <p>
                                            If I either: (a) cease to be
                                            guardian of this child; (b) or lose
                                            the right to request or receive
                                            health information from third
                                            parties respecting this child, I
                                            will remove them as a dependent
                                            under my Health Gateway account
                                            immediately.
                                        </p>
                                        <p>
                                            I understand that I will no longer
                                            be able to access my child’s health
                                            records once they are 12 years of
                                            age.
                                        </p>
                                    </b-checkbox>
                                </b-col>
                            </b-row>
                        </b-col>
                    </b-row>
                </form>
            </b-col>
        </b-row>
        <template #modal-footer>
            <b-row>
                <div class="mr-2">
                    <hg-button
                        data-testid="cancelRegistrationBtn"
                        variant="secondary"
                        @click="hideModal"
                        >Cancel</hg-button
                    >
                </div>
                <div>
                    <hg-button
                        data-testid="registerDependentBtn"
                        variant="primary"
                        @click="handleOk"
                        >Register Dependent</hg-button
                    >
                </div>
            </b-row>
        </template>
        <LoadingComponent :is-loading="isLoading" />
    </b-modal>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

.dependentCardDateInput {
    color: #e0e0e0;
}

::placeholder {
    color: #e0e0e0;
}
</style>
