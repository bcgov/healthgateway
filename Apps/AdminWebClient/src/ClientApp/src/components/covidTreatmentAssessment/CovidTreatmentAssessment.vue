<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faEye, faEyeSlash } from "@fortawesome/free-solid-svg-icons";
import { DateTime } from "luxon";
import { extend, ValidationObserver, ValidationProvider } from "vee-validate";
import { oneOf, regex, required } from "vee-validate/dist/rules";
import Vue from "vue";
import { Component, Emit } from "vue-property-decorator";

import BannerFeedbackComponent from "@/components/core/BannerFeedback.vue";
import Card from "@/components/covidTreatmentAssessment/Card.vue";
import OptionDetails from "@/components/covidTreatmentAssessment/OptionDetails.vue";
import { CovidTreatmentAssessmentOption } from "@/constants/CovidTreatmentAssessmentOption";
import { ResultType } from "@/constants/resulttype";
import { SnackbarPosition } from "@/constants/snackbarPosition";
import BannerFeedback from "@/models/bannerFeedback";
import CovidTreatmentAssessmentRequest from "@/models/CovidTreatmentAssessmentRequest";
import { Mask, phoneNumberMaskTemplate } from "@/utility/masks";

library.add(faEye, faEyeSlash);

const errorMessage = "This is a required field. Please select an answer.";

extend("regex", regex);

extend("oneOf", {
    ...oneOf,
    message: errorMessage,
});

extend("required", {
    ...required,
    message: errorMessage,
});

extend("requiredPhoneNumber", {
    ...required,
    message: "This is a required field. Please enter a phone number.",
});

@Component({
    components: {
        Card,
        OptionDetails,
        ValidationProvider,
        ValidationObserver,
        BannerFeedbackComponent,
    },
})
export default class CovidTreatmentAssessmentComponent extends Vue {
    private showFeedback = false;
    private today = DateTime.local();
    private dailyDataDatesModal = false;

    private covidTreatmentAssessmentRequest: CovidTreatmentAssessmentRequest = {
        phn: "9233238391",
        firstName: "Angel",
        lastName: "Leonardo",
        phoneNumber: "7782223688",
        identifiesIndigenous: CovidTreatmentAssessmentOption.Unspecified,
        hasAFamilyDoctorOrNp: CovidTreatmentAssessmentOption.Unspecified,
        confirmsOver12: false,
        testedPositiveInPast7Days: CovidTreatmentAssessmentOption.Unspecified,
        hasSevereCovid19Symptoms: CovidTreatmentAssessmentOption.Unspecified,
        hasMildOrModerateCovid19Symptoms: false,
        symptomOnSetDate: "",
        hasImmunityCompromisingMedicalConditionAntiViralTri:
            CovidTreatmentAssessmentOption.Unspecified,
        reports3DosesC19Vaccine: CovidTreatmentAssessmentOption.Unspecified,
        hasChronicConditionDiagnoses:
            CovidTreatmentAssessmentOption.Unspecified,
        agentComments: "",
        streetAddress: "3082 E 2nd Avenue Vancouver",
        provOrState: "BC",
        postalCode: "V5M 1E7",
        country: "Canada",
        changeAddressFlag: false,
    };

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
            hasMildOrModerateCovid19Symptoms: false,
            symptomOnSetDate: "",
            hasImmunityCompromisingMedicalConditionAntiViralTri:
                CovidTreatmentAssessmentOption.Unspecified,
            reports3DosesC19Vaccine: CovidTreatmentAssessmentOption.Unspecified,
            hasChronicConditionDiagnoses:
                CovidTreatmentAssessmentOption.Unspecified,
            agentComments: "",
            streetAddress: "",
            provOrState: "",
            postalCode: "",
            country: "",
            changeAddressFlag: false,
        };
    }

    private get patientFullName() {
        return `${this.covidTreatmentAssessmentRequest.firstName} ${this.covidTreatmentAssessmentRequest.lastName} `;
    }

    private get snackbarPosition(): string {
        return SnackbarPosition.Bottom;
    }

    private get phoneNumberMask(): Mask {
        return phoneNumberMaskTemplate;
    }

    private bannerFeedback: BannerFeedback = {
        type: ResultType.NONE,
        title: "",
        message: "",
    };

    private resetForm() {
        if (this.$refs.observer !== undefined) {
            (this.$refs.observer as Vue & { reset: () => boolean }).reset();
            this.covidTreatmentAssessmentRequest =
                this.resetCovidTreatmentAssessmentRequest();
        }
    }

    private async onSubmit() {
        if (this.$refs.observer !== undefined) {
            const isValid = await (
                this.$refs.observer as Vue & { validate: () => boolean }
            ).validate();

            if (isValid) {
                this.showFeedback = true;
                this.bannerFeedback = {
                    type: ResultType.Success,
                    title: "Success",
                    message:
                        "COVID-19 Treatment Assessment Form is Successfully Submitted.",
                };
                this.$emit("on-submit");
            } else {
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
        <BannerFeedbackComponent
            :show-feedback.sync="showFeedback"
            :feedback="bannerFeedback"
            :position="snackbarPosition"
        />
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
                                <div>Name</div>
                                <div>
                                    <label for="name">{{
                                        patientFullName
                                    }}</label>
                                </div>
                            </v-col>
                            <v-col>
                                <div>Birthdate</div>
                                <div>
                                    <label for="birthdate">1950-03-24</label>
                                </div>
                            </v-col>
                            <v-col>
                                <div>PHN</div>
                                <div>
                                    <label for="phn">{{
                                        covidTreatmentAssessmentRequest.phn
                                    }}</label>
                                </div>
                            </v-col>
                        </v-row>
                        <br />
                        <v-row dense>
                            <v-col cols="auto">
                                <ValidationProvider
                                    ref="phoneNumber"
                                    v-slot="{ errors }"
                                    :rules="{
                                        requiredPhoneNumber: true,
                                        regex: /^[2-9]\d{2}[2-9]\d{2}\d{4}$/,
                                    }"
                                    v-bind="$attrs"
                                    name="Phone Number"
                                >
                                    <v-text-field
                                        v-model="
                                            covidTreatmentAssessmentRequest.phoneNumber
                                        "
                                        dense
                                        label="Phone Number"
                                    />
                                    <span class="error-message">
                                        {{ errors[0] }}
                                    </span>
                                </ValidationProvider>
                            </v-col>
                        </v-row>
                        <Card title="Do you identify as Indigenous?">
                            <OptionDetails
                                :has-additional-response="false"
                                :value.sync="
                                    covidTreatmentAssessmentRequest.identifiesIndigenous
                                "
                            />
                        </Card>
                        <Card
                            title="Do you have a family doctor or nurse practitioner?"
                        >
                            <ValidationProvider
                                ref="hasAFamilyDoctorOrNp"
                                v-slot="{ errors }"
                                rules="oneOf:Yes,No"
                            >
                                <OptionDetails
                                    :value.sync="
                                        covidTreatmentAssessmentRequest.hasAFamilyDoctorOrNp
                                    "
                                />
                                <span class="error-message">
                                    {{ errors[0] }}
                                </span>
                            </ValidationProvider>
                        </Card>
                        <Card
                            title="1. Please confirm that you are over 12 years or older."
                            additional-info="This citizen is 71 years old."
                        >
                            <ValidationProvider
                                ref="confirmsOver12"
                                v-slot="{ errors }"
                                rules="oneOf:Yes,No"
                                v-bind="$attrs"
                                name="Confirms Over 12"
                            >
                                <OptionDetails
                                    :value.sync="
                                        covidTreatmentAssessmentRequest.confirmsOver12
                                    "
                                    :has-not-sure-option="false"
                                    :has-additional-response="true"
                                />
                                <span class="error-message">
                                    {{ errors[0] }}
                                </span>
                            </ValidationProvider>
                        </Card>
                        <Card
                            title="2. Have you recently tested positive for COVID-19 in the last 7 days?"
                        >
                            <ValidationProvider
                                ref="testedPositiveInPast7Days"
                                v-slot="{ errors }"
                                rules="oneOf:Yes,No,NotSure"
                                v-bind="$attrs"
                            >
                                <OptionDetails
                                    :value.sync="
                                        covidTreatmentAssessmentRequest.testedPositiveInPast7Days
                                    "
                                    :has-additional-response="true"
                                />
                                <span class="error-message">
                                    {{ errors[0] }}
                                </span>
                            </ValidationProvider>
                        </Card>
                        <Card
                            title="3. Do you have any severe symptoms of COVID-19?"
                        >
                            <ValidationProvider
                                ref="hasSevereCovid19Symptoms"
                                v-slot="{ errors }"
                                rules="oneOf:Yes,No,NotSure"
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
                            title="4. COVID-19 symptoms can range from mild to moderate. Mild and moderate symptoms are symptoms that can be managed at home. Do you have any symptoms of COVID-19?"
                        >
                            <OptionDetails
                                :value.sync="
                                    covidTreatmentAssessmentRequest.hasMildOrModerateCovid19Symptoms
                                "
                                :has-not-sure-option="true"
                                :has-additional-response="true"
                            />
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
                                        <v-row>
                                            <v-col>
                                                <v-text-field
                                                    v-model="
                                                        covidTreatmentAssessmentRequest.symptomOnSetDate
                                                    "
                                                    label="Date"
                                                    prepend-icon="mdi-calendar"
                                                    readonly
                                                    v-bind="attrs"
                                                    v-on="on"
                                                ></v-text-field>
                                            </v-col>
                                        </v-row>
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
                        </Card>
                        <Card
                            title="6. Do you have a medical condition or are you taking medications that suppress or weaken your immune system?"
                            additional-info="Citizen is considered immunocompromised."
                        >
                            <ValidationProvider
                                ref="hasMedicalCondition"
                                v-slot="{ errors }"
                                rules="oneOf:Yes,No,NotSure"
                                v-bind="$attrs"
                            >
                                <OptionDetails
                                    :value.sync="
                                        covidTreatmentAssessmentRequest.hasImmunityCompromisingMedicalConditionAntiViralTri
                                    "
                                    :has-not-sure-option="true"
                                    :has-additional-response="true"
                                />
                                <span class="error-message">
                                    {{ errors[0] }}
                                </span>
                            </ValidationProvider>
                        </Card>
                        <Card
                            title="7. Have you had 3 doses of the vaccine?"
                            additional-info="Citizen has had 3 doses of vaccine for more than 14 days."
                        >
                            <ValidationProvider
                                ref="reports3DosesC19Vaccine"
                                v-slot="{ errors }"
                                rules="oneOf:Yes,No,NotSure"
                                v-bind="$attrs"
                            >
                                <OptionDetails
                                    :value.sync="
                                        covidTreatmentAssessmentRequest.reports3DosesC19Vaccine
                                    "
                                    :has-not-sure-option="true"
                                    :has-additional-response="true"
                                />
                                <span class="error-message">
                                    {{ errors[0] }}
                                </span>
                            </ValidationProvider>
                        </Card>
                        <Card
                            title="8. Have you been diagnosed by a health care provider with a chronic condition?"
                            additional-info="Citizen has a chronic condition."
                        >
                            <OptionDetails
                                :value.sync="
                                    covidTreatmentAssessmentRequest.hasChronicConditionDiagnoses
                                "
                                :has-not-sure-option="true"
                                :has-additional-response="true"
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
</style>
