<script lang="ts">
import Vue from "vue";
import LoadingComponent from "@/components/loading.vue";
import { Component } from "vue-property-decorator";
import { Validation } from "vuelidate/vuelidate";
import { sameAs, required, minLength } from "vuelidate/lib/validators";
import { DateWrapper } from "@/models/dateWrapper";
import { Duration } from "luxon";
import { IDependentService } from "@/services/interfaces";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import ErrorTranslator from "@/utility/errorTranslator";
import BannerError from "@/models/bannerError";
import { Action } from "vuex-class";
import { ResultError } from "@/models/requestResult";

export enum GenderType {
    NotSelected = "",
    Male = "Male",
    Female = "Female",
    Other = "NotSpecified",
}

@Component({
    components: {
        LoadingComponent,
    },
})
export default class NewDependentComponent extends Vue {
    @Action("addError", { namespace: "errorBanner" })
    addError!: (error: BannerError) => void;

    private dependentService!: IDependentService;
    private isVisible = false;
    private isLoading = true;
    private firstName = "";
    private lastName = "";
    private birthdate = "";
    private PHN = "";
    private gender = GenderType.NotSelected;
    private accepted = false;
    private agreement = `
        I confirm that I am the parent or
        guardian for this child, pursuant to
        Part 4, Division 3 of the Family Law
        Act, the Adoption Act, and/or the Child,
        Family and Community Services
        Act.
    `;
    private genderOptions = [
        { value: GenderType.NotSelected, text: "Please select an option" },
        { value: GenderType.Male, text: "Male" },
        { value: GenderType.Female, text: "Female" },
        { value: GenderType.Other, text: "X" },
    ];

    private validations() {
        return {
            firstName: {
                required: required,
            },
            lastName: {
                required: required,
            },
            birthdate: {
                required: required,
                minLength: minLength(10),
                minValue: (value: string) =>
                    new DateWrapper(value).toJSDate() > this.minBirthdate,
            },
            gender: {
                required: required,
            },
            PHN: {
                required: required,
                minLength: minLength(12),
            },
            accepted: { isChecked: sameAs(() => true) },
        };
    }

    private isValid(param: Validation): boolean | undefined {
        return param.$dirty ? !param.$invalid : undefined;
    }

    private get minBirthdate(): Date {
        let mindate = new DateWrapper()
            .subtract(Duration.fromObject({ years: 19 }))
            .toJSDate();
        return mindate;
    }

    public showModal(): void {
        this.isVisible = true;
    }

    public hideModal(): void {
        this.$v.$reset();
        this.isVisible = false;
    }

    private mounted() {
        this.dependentService = container.get<IDependentService>(
            SERVICE_IDENTIFIER.DependentService
        );
        this.isLoading = false;
    }

    private handleOk(bvModalEvt: Event) {
        // Prevent modal from closing
        bvModalEvt.preventDefault();
        this.$v.$touch();
        if (this.$v.$invalid) {
        } else {
            this.$v.$reset();
            this.addDependent();
        }
    }

    private addDependent() {
        this.dependentService
            .addDependent({
                firstName: this.firstName,
                lastName: this.lastName,
                dateOfBirth: new DateWrapper(this.birthdate).toISODate(),
                PHN: this.PHN.replace(/\s/g, ""),
                gender: this.gender,
                version: 0,
            })
            .then(() => {
                this.handleSubmit();
            })
            .catch((err: ResultError) => {
                this.addError(
                    ErrorTranslator.toBannerError(
                        "Error adding dependent. Please review fields and try again.",
                        err
                    )
                );
                this.handleSubmit();
            })
            .finally(() => {
                this.firstName = "";
                this.lastName = "";
                this.birthdate = "";
                this.PHN = "";
                this.gender = GenderType.NotSelected;
                this.accepted = false;
            });
    }

    private handleSubmit() {
        // Hide the modal manually
        this.$nextTick(() => {
            this.hideModal();
        });
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
    >
        <b-row>
            <b-col>
                <form>
                    <b-row data-testid="newDependentModalText">
                        <b-col>
                            <b-row>
                                <b-col class="col-12 col-md-4 mb-2">
                                    <label for="firstName">First Name</label>
                                    <b-form-input
                                        id="firstName"
                                        v-model="firstName"
                                        data-testid="firstNameInput"
                                        placeholder="John"
                                        :state="isValid($v.firstName)"
                                        @blur.native="$v.firstName.$touch()"
                                    ></b-form-input>
                                    <b-form-invalid-feedback
                                        :state="isValid($v.firstName)"
                                    >
                                        First name is required
                                    </b-form-invalid-feedback>
                                </b-col>
                                <b-col class="col-12 col-md-4 mb-2">
                                    <label for="lastName">Last Name</label>
                                    <b-form-input
                                        id="lastName"
                                        v-model="lastName"
                                        data-testid="lastNameInput"
                                        placeholder="Doe"
                                        :state="isValid($v.lastName)"
                                        @blur.native="$v.lastName.$touch()"
                                    ></b-form-input>
                                    <b-form-invalid-feedback
                                        :state="isValid($v.lastName)"
                                    >
                                        Last name is required
                                    </b-form-invalid-feedback>
                                </b-col>
                                <b-col class="col-12 col-md-4 mb-2">
                                    <label for="birthdate">Date of Birth</label>
                                    <b-form-input
                                        id="birthdate"
                                        v-model="birthdate"
                                        v-mask="'####-##-##'"
                                        masked="false"
                                        data-testid="birthdateInput"
                                        placeholder="YYYY-MM-DD"
                                        type="text"
                                        :state="isValid($v.birthdate)"
                                        @blur.native="$v.birthdate.$touch()"
                                    ></b-form-input>
                                    <b-form-invalid-feedback
                                        :state="isValid($v.birthdate)"
                                    >
                                        Dependent must be under the age of 19
                                    </b-form-invalid-feedback>
                                </b-col>
                            </b-row>
                            <b-row class="mb-2">
                                <b-col class="col-12 col-md-6 mb-2">
                                    <label for="phn">PHN</label>
                                    <b-form-input
                                        id="phn"
                                        v-model="PHN"
                                        v-mask="'#### ### ###'"
                                        data-testid="phnInput"
                                        placeholder="1234 567 890"
                                        :state="isValid($v.PHN)"
                                        @blur.native="$v.PHN.$touch()"
                                    ></b-form-input>
                                    <b-form-invalid-feedback
                                        :state="isValid($v.PHN)"
                                    >
                                        Valid PHN is required
                                    </b-form-invalid-feedback>
                                </b-col>
                                <b-col class="col-12 col-md-6 mb-2">
                                    <b-row>
                                        <b-col>
                                            <label for="gender">Gender</label>
                                            <b-form-select
                                                id="gender"
                                                v-model="gender"
                                                data-testid="genderInput"
                                                :options="genderOptions"
                                                :state="isValid($v.gender)"
                                                @blur.native="
                                                    $v.gender.$touch()
                                                "
                                            >
                                            </b-form-select>
                                            <b-form-invalid-feedback
                                                :state="isValid($v.gender)"
                                            >
                                                Please select from one of the
                                                options
                                            </b-form-invalid-feedback>
                                        </b-col>
                                    </b-row>
                                </b-col>
                            </b-row>
                            <b-row class="mb-2">
                                <b-col
                                    ><b-checkbox
                                        id="termsCheckbox"
                                        v-model="accepted"
                                        data-testid="termsCheckbox"
                                        :state="isValid($v.accepted)"
                                        >{{ agreement }}</b-checkbox
                                    ></b-col
                                >
                            </b-row>
                        </b-col>
                    </b-row>
                </form>
            </b-col>
        </b-row>
        <template #modal-footer>
            <b-row>
                <div class="mr-2">
                    <b-btn variant="primary" @click="handleOk"
                        >Register dependent</b-btn
                    >
                </div>
                <div>
                    <b-btn variant="secondary" @click="hideModal">Cancel</b-btn>
                </div>
            </b-row>
        </template>
        <LoadingComponent :is-loading="isLoading"></LoadingComponent>
    </b-modal>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";
</style>
