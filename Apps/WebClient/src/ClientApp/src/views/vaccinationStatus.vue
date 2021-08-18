<script lang="ts">
import Vue from "vue";
import { Component, Watch } from "vue-property-decorator";
import { required } from "vuelidate/lib/validators";
import { Validation } from "vuelidate/vuelidate";
import { Action, Getter } from "vuex-class";

import DatePickerComponent from "@/components/datePicker.vue";
import VaccinationStatusResultComponent from "@/components/vaccinationStatusResult.vue";
import { DateWrapper, StringISODate } from "@/models/dateWrapper";
import VaccinationStatus from "@/models/vaccinationStatus";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import { ILogger } from "@/services/interfaces";
import PHNValidator from "@/utility/phnValidator";

const validPersonalHealthNumber = (value: string): boolean => {
    var phn = value.replace(/\D/g, "");
    return PHNValidator.IsValid(phn);
};

@Component({
    components: {
        "vaccination-status-result": VaccinationStatusResultComponent,
        "date-picker": DatePickerComponent,
    },
})
export default class VaccinationStatusView extends Vue {
    @Action("retrieve", { namespace: "vaccinationStatus" })
    retrieveVaccinationStatus!: (params: {
        phn: string;
        dateOfBirth: StringISODate;
    }) => Promise<void>;

    @Getter("vaccinationStatus", { namespace: "vaccinationStatus" }) status!:
        | VaccinationStatus
        | undefined;

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
    <div class="fill-height d-flex flex-column">
        <div class="header">
            <img
                class="img-fluid m-3"
                src="@/assets/images/gov/bcid-logo-rev-en.svg"
                width="181"
                alt="BC Mark"
            />
        </div>
        <vaccination-status-result v-if="displayResult" />
        <div v-else>
            <form class="container my-3" @submit.prevent="handleSubmit">
                <h1>COVID-19 Vaccination Status</h1>
                <hr />
                <h2>Please Provide the Following</h2>
                <b-row>
                    <b-col cols="12" sm="auto">
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
                            <b-form-invalid-feedback v-if="!$v.phn.required">
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
                <b-row>
                    <b-col cols="12" sm="auto">
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
                <hr />
                <div class="text-center">
                    <hg-button variant="secondary" class="mr-2" to="/">
                        Cancel
                    </hg-button>
                    <hg-button variant="primary" type="submit">
                        Get Status
                    </hg-button>
                </div>
            </form>
        </div>
    </div>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

.header {
    background-color: $hg-brand-primary;
}
</style>
