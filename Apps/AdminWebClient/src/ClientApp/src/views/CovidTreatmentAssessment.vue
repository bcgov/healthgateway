<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faEye, faEyeSlash } from "@fortawesome/free-solid-svg-icons";
import { DateTime } from "luxon";
import { extend, ValidationObserver, ValidationProvider } from "vee-validate";
import { oneOf, regex, required } from "vee-validate/dist/rules";
import Vue from "vue";
import { Component } from "vue-property-decorator";

import BannerFeedbackComponent from "@/components/core/BannerFeedback.vue";
import Card from "@/components/covidTreatmentAssessment/Card.vue";
import OptionDetails from "@/components/covidTreatmentAssessment/OptionDetails.vue";
import { CovidTreatmentAssessmentOption } from "@/constants/CovidTreatmentAssessmentOption";
import { ResultType } from "@/constants/resulttype";
import { SnackbarPosition } from "@/constants/snackbarPosition";
import BannerFeedback from "@/models/bannerFeedback";
import CovidTreatmentAssessmentRequest from "@/models/CovidTreatmentAssessmentRequest";
library.add(faEye, faEyeSlash);

extend("regex", regex);

extend("oneOf", {
    ...oneOf,
    message: "Choose one",
});

extend("required", {
    ...required,
    message: "{_field_} is required",
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
export default class CovidTreatmentAssessment extends Vue {
    //Define the questioner sequence.
    questionSequenceA = "a";
    questionSequenceB = "b";
    questionSequence1 = "1";
    questionSequence2 = "2";
    questionSequence3 = "3";
    questionSequence4 = "4";
    questionSequence5 = "5";
    questionSequence6 = "6";
    questionSequence7 = "7";
    questionSequence8 = "8";

    private maskPhoneNumber = true;
    private showFeedback = false;

    private today = DateTime.local();
    private covidTreatmentAssessmentRequest: CovidTreatmentAssessmentRequest = {
        phn: "9233238391",
        firstName: "Princess",
        lastName: "Agustin",
        phoneNumber: "",
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
        streetAddress: "",
        provOrState: "",
        postalCode: "",
        country: "",
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

    private dailyDataDatesModal = false;
    private showPhoneNumber(): boolean {
        if (this.covidTreatmentAssessmentRequest.phoneNumber !== undefined) {
            return this.covidTreatmentAssessmentRequest.phoneNumber.length > 0;
        }
        return false;
    }
    private bannerFeedback: BannerFeedback = {
        type: ResultType.NONE,
        title: "",
        message: "",
    };

    private get snackbarPosition(): string {
        return SnackbarPosition.Bottom;
    }

    private async submit() {
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

                setTimeout(
                    () => this.$router.push({ path: "/covidcard" }),
                    2000
                );
            } else {
                console.log("Error validation");
            }
        }

        //alert(this.covidTreatmentAssessmentRequest.identifiesIndigenous);
        //alert(this.covidTreatmentAssessmentRequest.hasAFamilyDoctorOrNp);
        //alert(this.covidTreatmentAssessmentRequest.symptomOnSetDate);
    }

    private cancel() {
        if (this.$refs.observer !== undefined) {
            (this.$refs.observer as Vue & { reset: () => boolean }).reset();
            this.covidTreatmentAssessmentRequest =
                this.resetCovidTreatmentAssessmentRequest();
        }
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
            <v-col cols="12" sm="12" md="10">
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
                                        required: true,
                                        regex: /^[2-9]\d{2}[2-9]\d{2}\d{4}$/,
                                    }"
                                    v-bind="$attrs"
                                    name="Phone Number"
                                >
                                    <v-text-field
                                        v-if="showPhoneNumber()"
                                        v-model="
                                            covidTreatmentAssessmentRequest.phoneNumber
                                        "
                                        :value="
                                            maskPhoneNumber
                                                ? '        '
                                                : covidTreatmentAssessmentRequest.phoneNumber
                                        "
                                        :type="
                                            maskPhoneNumber
                                                ? 'password'
                                                : 'text'
                                        "
                                        :append-outer-icon="
                                            maskPhoneNumber
                                                ? 'fa-eye-slash'
                                                : 'fa-eye'
                                        "
                                        dense
                                        label="Phone Number"
                                        @click:append-outer="
                                            maskPhoneNumber = !maskPhoneNumber
                                        "
                                    />
                                    <v-text-field
                                        v-else
                                        v-model="
                                            covidTreatmentAssessmentRequest.phoneNumber
                                        "
                                        dense
                                        label="No Phone Number"
                                    />
                                    <span class="error-message">{{
                                        errors[0]
                                    }}</span>
                                </ValidationProvider>
                            </v-col>
                        </v-row>
                        <Card :question-sequence="questionSequenceA">
                            <OptionDetails
                                v-model="
                                    covidTreatmentAssessmentRequest.identifiesIndigenous
                                "
                                :question-sequence="questionSequenceA"
                                :has-additional-response="false"
                            />
                        </Card>
                        <Card :question-sequence="questionSequenceB">
                            <ValidationProvider
                                ref="hasAFamilyDoctorOrNp"
                                v-slot="{ errors }"
                                rules="oneOf:Yes,No"
                                v-bind="$attrs"
                            >
                                <OptionDetails
                                    v-model="
                                        covidTreatmentAssessmentRequest.hasAFamilyDoctorOrNp
                                    "
                                    :question-sequence="questionSequenceB"
                                />
                                <span class="error-message">{{
                                    errors[0]
                                }}</span>
                            </ValidationProvider>
                        </Card>
                        <Card
                            :question-sequence="questionSequence1"
                            :have-additional-info="true"
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
                                    v-model="
                                        covidTreatmentAssessmentRequest.confirmsOver12
                                    "
                                    :question-sequence="questionSequence1"
                                    :has-not-sure-option="false"
                                    :has-additional-response="true"
                                />
                                <span class="error-message">{{
                                    errors[0]
                                }}</span>
                            </ValidationProvider>
                        </Card>
                        <Card :question-sequence="questionSequence2">
                            <ValidationProvider
                                ref="testedPositiveInPast7Days"
                                v-slot="{ errors }"
                                rules="oneOf:Yes,No,NotSure"
                                v-bind="$attrs"
                            >
                                <OptionDetails
                                    v-model="
                                        covidTreatmentAssessmentRequest.testedPositiveInPast7Days
                                    "
                                    :question-sequence="questionSequence2"
                                    :has-not-sure-option="true"
                                    :has-additional-response="true"
                                />
                                <span class="error-message">{{
                                    errors[0]
                                }}</span>
                            </ValidationProvider>
                        </Card>
                        <Card :question-sequence="questionSequence3">
                            <ValidationProvider
                                ref="hasSevereCovid19Symptoms"
                                v-slot="{ errors }"
                                rules="oneOf:Yes,No,NotSure"
                                v-bind="$attrs"
                            >
                                <OptionDetails
                                    v-model="
                                        covidTreatmentAssessmentRequest.hasSevereCovid19Symptoms
                                    "
                                    :question-sequence="questionSequence3"
                                    :has-not-sure-option="true"
                                />
                                <span class="error-message">{{
                                    errors[0]
                                }}</span>
                            </ValidationProvider>
                        </Card>
                        <Card :question-sequence="questionSequence4">
                            <OptionDetails
                                v-model="
                                    covidTreatmentAssessmentRequest.hasMildOrModerateCovid19Symptoms
                                "
                                :question-sequence="questionSequence4"
                                :has-not-sure-option="true"
                                :has-additional-response="true"
                            />
                        </Card>
                        <Card :question-sequence="questionSequence5">
                            <div style="width: 250px">
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
                            :question-sequence="questionSequence6"
                            have-additional-info="true"
                            additional-info="Citizen is considered immunocompromised."
                        >
                            <ValidationProvider
                                ref="hasMedicalCondition"
                                v-slot="{ errors }"
                                rules="oneOf:Yes,No,NotSure"
                                v-bind="$attrs"
                            >
                                <OptionDetails
                                    v-model="
                                        covidTreatmentAssessmentRequest.hasImmunityCompromisingMedicalConditionAntiViralTri
                                    "
                                    :question-sequence="questionSequence6"
                                    :has-not-sure-option="true"
                                    :has-additional-response="true"
                                />
                                <span class="error-message">{{
                                    errors[0]
                                }}</span>
                            </ValidationProvider>
                        </Card>
                        <Card
                            :question-sequence="questionSequence7"
                            have-additional-info="true"
                            additional-info="Citizen has had 3 doses of vaccine for more than 14 days."
                        >
                            <ValidationProvider
                                ref="reports3DosesC19Vaccine"
                                v-slot="{ errors }"
                                rules="oneOf:Yes,No,NotSure"
                                v-bind="$attrs"
                            >
                                <OptionDetails
                                    v-model="
                                        covidTreatmentAssessmentRequest.reports3DosesC19Vaccine
                                    "
                                    :question-sequence="questionSequence7"
                                    :has-not-sure-option="true"
                                    :has-additional-response="true"
                                />
                                <span class="error-message">{{
                                    errors[0]
                                }}</span>
                            </ValidationProvider>
                        </Card>
                        <Card
                            :question-sequence="questionSequence8"
                            have-additional-info="true"
                            additional-info="Citizen has a chronic condition."
                        >
                            <OptionDetails
                                v-model="
                                    covidTreatmentAssessmentRequest.hasChronicConditionDiagnoses
                                "
                                :question-sequence="questionSequence8"
                                :has-not-sure-option="true"
                                :has-additional-response="true"
                            />
                        </Card>
                        <Card>
                            <div>Notes</div>
                            <div class="pt-2">
                                <b-form-textarea
                                    v-model="
                                        covidTreatmentAssessmentRequest.agentComments
                                    "
                                    class="comment-input"
                                    rows="6"
                                    max-rows="15"
                                    maxlength="2000"
                                    style="overflow: auto"
                                ></b-form-textarea>
                            </div>
                        </Card>
                    </v-form>
                </ValidationObserver>
                <v-card-actions>
                    <v-row>
                        <v-col align="right">
                            <v-btn
                                color="secondary"
                                class="font-weight-light mr-4"
                                @click="cancel"
                            >
                                Cancel
                            </v-btn>

                            <v-btn
                                color="success"
                                class="font-weight-light"
                                @click="submit"
                                >Submit</v-btn
                            >
                        </v-col>
                    </v-row>
                </v-card-actions>
            </v-col>
        </v-row>
    </v-container>
</template>

<style scoped lang="scss">
.comment-input {
    border-right: 0px;
    background-color: white;
    width: 560px;
}
.error-message {
    color: #ff5252 !important;
}
</style>
