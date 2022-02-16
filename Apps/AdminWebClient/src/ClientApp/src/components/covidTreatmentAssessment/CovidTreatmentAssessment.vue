<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faEye, faEyeSlash } from "@fortawesome/free-solid-svg-icons";
import { DateTime } from "luxon";
import { extend, ValidationObserver, ValidationProvider } from "vee-validate";
import { oneOf, regex, required } from "vee-validate/dist/rules";
import Vue from "vue";
import { Component, Emit, Prop } from "vue-property-decorator";

import AddressComponent from "@/components/core/Address.vue";
import Card from "@/components/covidTreatmentAssessment/Card.vue";
import OptionDetails from "@/components/covidTreatmentAssessment/OptionDetails.vue";
import { CovidTreatmentAssessmentOption } from "@/constants/covidTreatmentAssessmentOption";
import type Address from "@/models/address";
import type CovidTreatmentAssessmentDetails from "@/models/covidTreatmentAssessmentDetails";
import type CovidTreatmentAssessmentRequest from "@/models/covidTreatmentAssessmentRequest";
import { DateWrapper } from "@/models/dateWrapper";
import type PatientData from "@/models/patientData";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { ICovidSupportService } from "@/services/interfaces";
import { Mask, phoneNumberMaskTemplate } from "@/utility/masks";

library.add(faEye, faEyeSlash);

const errorMessage = "This is a required field";

extend("regex", regex);

extend("oneOf", {
    ...oneOf,
    message: errorMessage,
});

extend("required", {
    ...required,
    message: errorMessage,
});

@Component({
    components: {
        Card,
        OptionDetails,
        ValidationProvider,
        ValidationObserver,
        AddressComponent,
    },
})
export default class CovidTreatmentAssessmentComponent extends Vue {
    @Prop({ required: true }) details!: CovidTreatmentAssessmentDetails;
    @Prop({ required: true }) defaultAddress!: Address;
    @Prop({ required: true }) patient!: PatientData;
    private covidSupportService!: ICovidSupportService;

    private address: Address = {
        streetLines: [],
        city: "",
        state: "",
        postalCode: "",
        country: "",
    };
    private isEditingAddress = false;
    private today = DateTime.local();
    private dailyDataDatesModal = false;

    private covidTreatmentAssessmentRequest: CovidTreatmentAssessmentRequest = {
        phn: "",
        firstName: "",
        lastName: "",
        phoneNumber: "",
        identifiesIndigenous: CovidTreatmentAssessmentOption.Unspecified,
        hasAFamilyDoctorOrNp: CovidTreatmentAssessmentOption.Unspecified,
        confirmsOver12: false,
        testedPositiveInPast7Days: CovidTreatmentAssessmentOption.Unspecified,
        hasSevereCovid19Symptoms: CovidTreatmentAssessmentOption.Unspecified,
        hasMildOrModerateCovid19Symptoms:
            CovidTreatmentAssessmentOption.Unspecified,
        symptomOnSetDate: null,
        hasImmunityCompromisingMedicalConditionAntiViralTri:
            CovidTreatmentAssessmentOption.Unspecified,
        reports3DosesC19Vaccine: CovidTreatmentAssessmentOption.Unspecified,
        hasChronicConditionDiagnoses:
            CovidTreatmentAssessmentOption.Unspecified,
        agentComments: "",
        streetAddress: [],
        provOrState: "",
        postalCode: "",
        country: "",
        changeAddressFlag: false,
        positiveCovidLabData: "",
        covidVaccinationHistory: "",
        cevGroupDetails: "",
        submitted: new DateWrapper().toISO(true),
    };

    private mounted(): void {
        this.address = { ...this.defaultAddress };
        this.covidSupportService = container.get(
            SERVICE_IDENTIFIER.CovidSupportService
        );
    }

    private resetCovidTreatmentAssessmentRequest(): CovidTreatmentAssessmentRequest {
        return {
            phn: "",
            firstName: "",
            lastName: "",
            phoneNumber: "",
            identifiesIndigenous: CovidTreatmentAssessmentOption.Unspecified,
            hasAFamilyDoctorOrNp: CovidTreatmentAssessmentOption.Unspecified,
            confirmsOver12: false,
            testedPositiveInPast7Days:
                CovidTreatmentAssessmentOption.Unspecified,
            hasSevereCovid19Symptoms:
                CovidTreatmentAssessmentOption.Unspecified,
            hasMildOrModerateCovid19Symptoms:
                CovidTreatmentAssessmentOption.Unspecified,
            symptomOnSetDate: null,
            hasImmunityCompromisingMedicalConditionAntiViralTri:
                CovidTreatmentAssessmentOption.Unspecified,
            reports3DosesC19Vaccine: CovidTreatmentAssessmentOption.Unspecified,
            hasChronicConditionDiagnoses:
                CovidTreatmentAssessmentOption.Unspecified,
            agentComments: "",
            streetAddress: [],
            provOrState: "",
            postalCode: "",
            country: "",
            changeAddressFlag: false,
            positiveCovidLabData: "",
            covidVaccinationHistory: "",
            cevGroupDetails: "",
            submitted: new DateWrapper().toISODate(),
        };
    }

    private get age(): number {
        const today = new DateWrapper();
        const birthdate = new DateWrapper(this.patient.birthdate);
        return Math.floor(today.diff(birthdate, "years").years);
    }

    private get formattedBirthdate(): string {
        return new DateWrapper(this.patient.birthdate).format();
    }

    private get patientFullName(): string {
        this.covidTreatmentAssessmentRequest.firstName = this.patient.firstname;
        this.covidTreatmentAssessmentRequest.lastName = this.patient.lastname;
        return `${this.patient.firstname} ${this.patient.lastname} `;
    }

    private get patientPersonalHealthNumber(): string {
        this.covidTreatmentAssessmentRequest.phn =
            this.patient.personalhealthnumber;
        return this.patient.personalhealthnumber;
    }

    private get phoneNumberMask(): Mask {
        return phoneNumberMaskTemplate;
    }

    private get symptomsOnsetGreaterThanTenDays(): boolean {
        const symptomOnsetDate =
            this.covidTreatmentAssessmentRequest.symptomOnSetDate;

        if (!symptomOnsetDate) {
            return false;
        }
        const symptomOnsetDateWrapper = new DateWrapper(symptomOnsetDate, {
            isUtc: true,
        });
        const lastWeek = new DateWrapper().subtract({ days: 10 });
        return symptomOnsetDateWrapper.isBefore(lastWeek);
    }

    private onEditAddressChange(): void {
        if (!this.isEditingAddress) {
            this.address = { ...this.defaultAddress };
        }
    }

    private resetForm() {
        if (this.$refs.observer !== undefined) {
            (this.$refs.observer as Vue & { reset: () => boolean }).reset();
            this.covidTreatmentAssessmentRequest =
                this.resetCovidTreatmentAssessmentRequest();
        }
    }

    private setAddressRequest(): void {
        this.covidTreatmentAssessmentRequest.country = this.address.country;
        this.covidTreatmentAssessmentRequest.postalCode =
            this.address.postalCode;
        this.covidTreatmentAssessmentRequest.provOrState = this.address.state;
        this.covidTreatmentAssessmentRequest.streetAddress =
            this.address.streetLines;
    }

    private submitCovidTreatmentAssessment(): void {
        this.setAddressRequest();
        this.covidSupportService
            .submitCovidTreatmentAssessment(
                this.covidTreatmentAssessmentRequest
            )
            .then((string) => {
                this.$emit("on-submit-success");
                console.log(string);
            })
            .catch((err) => {
                this.$emit("on-submit-failure");
                console.log(err);
            })
            .finally(() => {
                console.log("finally");
            });
    }

    private async onSubmit() {
        if (this.$refs.observer !== undefined) {
            const isValid = await (
                this.$refs.observer as Vue & { validate: () => boolean }
            ).validate();
            if (isValid) {
                console.log("test");
                this.submitCovidTreatmentAssessment();
            } else {
                this.$emit("on-submit-failure");
                console.log("Error validation");
            }
        }
    }

    @Emit()
    private onCancel() {
        this.resetForm();
    }
}
</script>
<template>
    <v-container>
        <v-row no-gutters>
            <v-col cols="12" sm="12" md="10" offset-md="1">
                <ValidationObserver ref="observer">
                    <v-form ref="form" lazy-validation>
                        <v-row>
                            <v-col>
                                <h2>Patient Information</h2>
                                <br />
                            </v-col>
                        </v-row>
                        <v-row dense>
                            <v-col>
                                <v-text-field
                                    :value="patientFullName"
                                    label="Name"
                                    disabled
                                />
                            </v-col>
                            <v-col>
                                <v-text-field
                                    :value="formattedBirthdate"
                                    label="Birthdate"
                                    disabled
                                />
                            </v-col>
                            <v-col>
                                <v-text-field
                                    :value="patientPersonalHealthNumber"
                                    label="Personal Health Number"
                                    disabled
                                />
                            </v-col>
                        </v-row>
                        <br />
                        <v-row dense>
                            <v-col cols="auto">
                                <ValidationProvider
                                    v-slot="{ errors }"
                                    :rules="{
                                        required: true,
                                        regex: /^[2-9]\d{2}[2-9]\d{2}\d{4}$|^\([2-9]\d{2}\) [2-9]\d{2}-\d{4}$/,
                                    }"
                                    v-bind="$attrs"
                                    name="Phone Number"
                                >
                                    <v-text-field
                                        v-model="
                                            covidTreatmentAssessmentRequest.phoneNumber
                                        "
                                        v-mask="phoneNumberMask"
                                        dense
                                        label="Phone Number*"
                                    />
                                    <span class="error-message">
                                        {{ errors[0] }}
                                    </span>
                                </ValidationProvider>
                            </v-col>
                        </v-row>
                        <Card title="Do you identify as Indigenous?">
                            <OptionDetails
                                :value.sync="
                                    covidTreatmentAssessmentRequest.identifiesIndigenous
                                "
                            />
                        </Card>
                        <Card
                            title="Do you have a family doctor or nurse practitioner?"
                        >
                            <OptionDetails
                                :value.sync="
                                    covidTreatmentAssessmentRequest.hasAFamilyDoctorOrNp
                                "
                            />
                        </Card>
                        <Card
                            title="1. Please confirm that you are over 12 years or older.*"
                            :additional-info="`This citizen is ${age} years old.`"
                            display-additional-info="true"
                        >
                            <ValidationProvider
                                v-slot="{ errors }"
                                rules="oneOf:Yes,No"
                                v-bind="$attrs"
                                name="Confirms Over 12"
                            >
                                <OptionDetails
                                    :value.sync="
                                        covidTreatmentAssessmentRequest.confirmsOver12
                                    "
                                    :show-message-when-no-is-selected="true"
                                />
                                <span class="error-message">
                                    {{ errors[0] }}
                                </span>
                            </ValidationProvider>
                        </Card>
                        <Card
                            title="2. Have you recently tested positive for COVID-19 in the last 7 days?*"
                            additional-info="This citizen has tested positive for COVID-19 within the last 7 days."
                            :display-additional-info="
                                details.hasKnownPositiveC19Past7Days
                            "
                        >
                            <ValidationProvider
                                v-slot="{ errors }"
                                rules="oneOf:Yes,No"
                                v-bind="$attrs"
                            >
                                <OptionDetails
                                    :value.sync="
                                        covidTreatmentAssessmentRequest.testedPositiveInPast7Days
                                    "
                                    :show-message-when-no-is-selected="true"
                                />
                                <span class="error-message">
                                    {{ errors[0] }}
                                </span>
                            </ValidationProvider>
                        </Card>
                        <Card
                            title="3. Do you have any severe symptoms of COVID-19?*"
                        >
                            <ValidationProvider
                                v-slot="{ errors }"
                                rules="oneOf:Yes,No"
                                v-bind="$attrs"
                            >
                                <OptionDetails
                                    :value.sync="
                                        covidTreatmentAssessmentRequest.hasSevereCovid19Symptoms
                                    "
                                />
                                <span class="error-message">
                                    {{ errors[0] }}
                                </span>
                            </ValidationProvider>
                        </Card>
                        <Card
                            title="4. COVID-19 symptoms can range from mild to moderate. Mild and moderate symptoms are symptoms that can be managed at home. Do you have any symptoms of COVID-19?*"
                        >
                            <ValidationProvider
                                v-slot="{ errors }"
                                rules="oneOf:Yes,No,NotSure"
                                v-bind="$attrs"
                            >
                                <OptionDetails
                                    :value.sync="
                                        covidTreatmentAssessmentRequest.hasMildOrModerateCovid19Symptoms
                                    "
                                    :has-not-sure-option="true"
                                    :show-message-when-no-is-selected="true"
                                />
                                <span class="error-message">
                                    {{ errors[0] }}
                                </span>
                            </ValidationProvider>
                        </Card>
                        <Card title="5. When did your symptoms first start?">
                            <div style="max-width: 250px" class="pt-2">
                                <v-dialog
                                    ref="dailyDialog"
                                    v-model="dailyDataDatesModal"
                                    :return-value.sync="
                                        covidTreatmentAssessmentRequest.symptomOnSetDate
                                    "
                                    persistent
                                    width="290px"
                                >
                                    <template #activator="{ on, attrs }">
                                        <v-text-field
                                            v-model="
                                                covidTreatmentAssessmentRequest.symptomOnSetDate
                                            "
                                            label="Date"
                                            prepend-icon="mdi-calendar"
                                            readonly
                                            v-bind="attrs"
                                            v-on="on"
                                        />
                                    </template>
                                    <v-date-picker
                                        v-model="
                                            covidTreatmentAssessmentRequest.symptomOnSetDate
                                        "
                                        :max="today.toISO()"
                                        scrollable
                                        no-title
                                    >
                                        <v-spacer></v-spacer>
                                        <v-btn
                                            text
                                            color="primary"
                                            @click="dailyDataDatesModal = false"
                                        >
                                            Cancel
                                        </v-btn>
                                        <v-btn
                                            text
                                            color="primary"
                                            @click="
                                                $refs.dailyDialog.save(
                                                    covidTreatmentAssessmentRequest.symptomOnSetDate
                                                )
                                            "
                                        >
                                            OK
                                        </v-btn>
                                    </v-date-picker>
                                </v-dialog>
                            </div>
                            <div
                                v-if="symptomsOnsetGreaterThanTenDays"
                                class="option-message-color"
                            >
                                Citizen would likely not benefit from COVID-19
                                treatment.
                            </div>
                        </Card>
                        <Card
                            title="6. Do you have a medical condition or are you taking medications that suppress or weaken your immune system?*"
                            additional-info="Citizen is considered immunocompromised."
                            :display-additional-info="
                                details.citizenIsConsideredImmunoCompromised
                            "
                        >
                            <ValidationProvider
                                v-slot="{ errors }"
                                rules="oneOf:Yes,No,NotSure"
                                v-bind="$attrs"
                            >
                                <OptionDetails
                                    :value.sync="
                                        covidTreatmentAssessmentRequest.hasImmunityCompromisingMedicalConditionAntiViralTri
                                    "
                                    :has-not-sure-option="true"
                                    :show-message-when-yes-is-selected="true"
                                />
                                <span class="error-message">
                                    {{ errors[0] }}
                                </span>
                            </ValidationProvider>
                        </Card>
                        <Card
                            title="7. Have you had 3 doses of the vaccine?"
                            additional-info="Citizen has had 3 doses of vaccine for more than 14 days."
                            :display-additional-info="
                                details.has3DoseMoreThan14Days
                            "
                        >
                            <OptionDetails
                                :value.sync="
                                    covidTreatmentAssessmentRequest.reports3DosesC19Vaccine
                                "
                                :has-not-sure-option="true"
                                :has-selected-yes-with-no-benifit="true"
                            />
                        </Card>
                        <Card
                            title="8. Have you been diagnosed by a health care provider with a chronic condition?"
                            additional-info="Citizen has a chronic condition."
                            :display-additional-info="
                                details.hasDocumentedChronicCondition
                            "
                        >
                            <OptionDetails
                                :value.sync="
                                    covidTreatmentAssessmentRequest.hasChronicConditionDiagnoses
                                "
                                :has-not-sure-option="true"
                                :show-message-when-yes-is-selected="true"
                            />
                        </Card>
                        <Card title="Notes">
                            <div class="pt-2">
                                <v-textarea
                                    maxlength="2000"
                                    counter="2000"
                                    filled
                                    auto-grow
                                    rows="4"
                                    :value="
                                        covidTreatmentAssessmentRequest.agentComments
                                    "
                                />
                            </div>
                        </Card>
                        <AddressComponent
                            v-bind.sync="address"
                            :is-disabled="!isEditingAddress"
                            class="mt-6"
                        />
                        <v-row class="pt-3">
                            <v-col cols="12" sm="6" lg="4">
                                <v-checkbox
                                    v-model="isEditingAddress"
                                    label="Mail to a different address"
                                    @change="onEditAddressChange"
                                />
                            </v-col>
                            <v-col cols="12" sm="6" lg="4">
                                <v-text-field
                                    :value="
                                        covidTreatmentAssessmentRequest.phoneNumber
                                    "
                                    label="Confirm Phone Number"
                                    disabled
                                />
                            </v-col>
                        </v-row>
                    </v-form>
                </ValidationObserver>
                <v-row class="py-3">
                    <v-col align="right">
                        <v-btn
                            color="secondary"
                            class="font-weight-light mr-4"
                            @click="onCancel"
                        >
                            Cancel
                        </v-btn>

                        <v-btn
                            color="success"
                            class="font-weight-light"
                            @click="onSubmit"
                            >Submit</v-btn
                        >
                    </v-col>
                </v-row>
            </v-col>
        </v-row>
    </v-container>
</template>

<style scoped lang="scss">
.comment-input {
    background-color: white;
}
.error-message {
    color: #ff5252 !important;
}
.option-message-color {
    color: #ff9800;
}
</style>
