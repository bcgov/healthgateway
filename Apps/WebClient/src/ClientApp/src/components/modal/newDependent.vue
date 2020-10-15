<script lang="ts">
import Vue from "vue";
import LoadingComponent from "@/components/loading.vue";
import { Component } from "vue-property-decorator";
import { Validation } from "vuelidate/vuelidate";
import { sameAs } from "vuelidate/lib/validators";
import { DateWrapper } from "@/models/dateWrapper";
import { DateTime, Duration } from "luxon";

export enum GenderType {
    NotSelected = "",
    Male = "male",
    Female = "female",
    Other = "other",
    None = "none",
}

@Component({
    components: {
        LoadingComponent,
    },
})
export default class NewDependentComponent extends Vue {
    private isVisible = false;
    private isLoading = true;
    private firstName = "";
    private lastName = "";
    private birthdate = "2005-12-03";
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
        { value: GenderType.Other, text: "Other" },
        { value: GenderType.None, text: "Prefer not to say" },
    ];
    private validations() {
        return {
            name: {
                required: true,
            },
            birthdate: {
                required: true,
                underage: this.isUnderage(),
            },
            gender: {
                required: true,
            },
            PHN: {
                required: true,
            },
            accepted: { isChecked: sameAs(() => true) },
        };
    }

    private isValid(param: Validation): boolean | undefined {
        return param.$dirty ? !param.$invalid : undefined;
    }

    private isUnderage() {
        console.log(this.birthdate);
        let dob: DateWrapper = new DateWrapper(
            DateTime.fromISO(this.birthdate)
        );
        if (
            dob.isAfter(
                new DateWrapper().subtract(Duration.fromObject({ years: 19 }))
            )
        ) {
            console.log("Underage!");
            return true;
        }
        console.log("Not underage.");
        return false;
    }

    public showModal(): void {
        this.isVisible = true;
    }

    public hideModal(): void {
        this.isVisible = false;
    }

    private mounted() {
        this.isLoading = false;
    }

    private handleOk(bvModalEvt: Event) {
        // Prevent modal from closing
        bvModalEvt.preventDefault();
        this.$v.$touch();
        if (this.$v.$invalid) {
            console.log("One or more fields are invalid.");
        }

        // Trigger submit handler
        // this.handleSubmit();
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
        title="New Dependent"
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
                            <b-row class="mb-2">
                                <b-col>
                                    <label for="firstName">First Name</label>
                                    <b-form-input
                                        id="firstName"
                                        data-testid="firstNameInput"
                                        placeholder="John"
                                        :state="isValid($v.name)"
                                    ></b-form-input>
                                    <b-form-invalid-feedback
                                        :state="isValid($v.name)"
                                    >
                                        First name is required
                                    </b-form-invalid-feedback>
                                </b-col>
                                <b-col>
                                    <label for="lastName">Last Name</label>
                                    <b-form-input
                                        id="lastName"
                                        data-testid="lastNameInput"
                                        placeholder="Doe"
                                        :state="isValid($v.name)"
                                    ></b-form-input>
                                    <b-form-invalid-feedback
                                        :state="isValid($v.name)"
                                    >
                                        Last name is required
                                    </b-form-invalid-feedback>
                                </b-col>
                                <b-col>
                                    <label for="birthdate">Date of Birth</label>
                                    <b-form-input
                                        id="birthdate"
                                        v-mask="'####-##-##'"
                                        data-testid="birthdateInput"
                                        placeholder="YYYY-MM-DD"
                                        type="text"
                                        :state="isValid($v.birthdate)"
                                    ></b-form-input>
                                    <b-form-invalid-feedback
                                        :state="isValid($v.birthdate)"
                                    >
                                        Valid birthdate is required
                                    </b-form-invalid-feedback>
                                    <b-form-invalid-feedback
                                        :state="$v.birthdate.$underage"
                                    >
                                        Dependent must be under the age of 19
                                    </b-form-invalid-feedback>
                                </b-col>
                            </b-row>
                            <b-row class="mb-4">
                                <b-col>
                                    <label for="phn">PHN</label>
                                    <b-form-input
                                        id="phn"
                                        v-mask="'#### ### ###'"
                                        data-testid="phnInput"
                                        placeholder="1234 567 890"
                                        :state="isValid($v.PHN)"
                                    ></b-form-input>
                                    <b-form-invalid-feedback
                                        :state="isValid($v.PHN)"
                                    >
                                        Valid PHN is required
                                    </b-form-invalid-feedback>
                                </b-col>
                                <b-col>
                                    <b-row>
                                        <label for="gender">Gender</label>
                                    </b-row>
                                    <b-row>
                                        <b-col>
                                            <b-form-select
                                                id="gender"
                                                v-model="gender"
                                                data-testid="genderInput"
                                                :options="genderOptions"
                                                :state="isValid($v.gender)"
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
