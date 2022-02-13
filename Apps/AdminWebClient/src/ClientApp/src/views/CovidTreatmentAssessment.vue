<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faEye, faEyeSlash } from "@fortawesome/free-solid-svg-icons";
import { DateTime } from "luxon";
import { extend, ValidationObserver, ValidationProvider } from "vee-validate";
import { required } from "vee-validate/dist/rules";
import Vue from "vue";
import { Component } from "vue-property-decorator";

import Card from "@/components/covidTreatmentAssessment/Card.vue";
import RadioButton from "@/components/covidTreatmentAssessment/RadioButton.vue";
import { CovidTreatmentAssessmentOption } from "@/constants/CovidTreatmentAssessmentOption";
import CovidTreatmentAssessmentRequest from "@/models/CovidTreatmentAssessmentRequest";
library.add(faEye, faEyeSlash);

extend("required", {
    ...required,
    message: "{_field_} is required",
});

@Component({
    components: { Card, RadioButton, ValidationProvider, ValidationObserver },
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

    private today = DateTime.local();
    private covidTreatmentAssessmentRequest: CovidTreatmentAssessmentRequest = {
        phn: "9233238391",
        firstName: "Princess",
        lastName: "Agustin",
        phoneNumber: "778-222-3369",
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

    private get patientFullName() {
        return `${this.covidTreatmentAssessmentRequest.firstName} ${this.covidTreatmentAssessmentRequest.lastName} `;
    }

    private dailyDataDatesModal = false;

    private selectedDates: string[] = [this.today.toISO()];

    private submit() {
        this.$refs.observer.validate();
        //alert(this.covidTreatmentAssessmentRequest.identifiesIndigenous);
        //alert(this.covidTreatmentAssessmentRequest.hasAFamilyDoctorOrNp);
        //alert(this.covidTreatmentAssessmentRequest.symptomOnSetDate);
    }
}
</script>
<template>
    <v-container>
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
                                    :rules="{ required: true }"
                                    v-bind="$attrs"
                                    name="Phone Number"
                                >
                                    <v-text-field
                                        v-if="
                                            covidTreatmentAssessmentRequest
                                                .phoneNumber.length > 0
                                        "
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
                            <RadioButton
                                v-model="
                                    covidTreatmentAssessmentRequest.identifiesIndigenous
                                "
                                :question-sequence="questionSequenceA"
                                :has-additional-response="false"
                            />
                        </Card>
                        <Card :question-sequence="questionSequenceB">
                            <RadioButton
                                v-model="
                                    covidTreatmentAssessmentRequest.hasAFamilyDoctorOrNp
                                "
                                :question-sequence="questionSequenceB"
                            />
                        </Card>
                        <Card
                            :question-sequence="questionSequence1"
                            :have-additional-info="true"
                            additional-info="This citizen is 71 years old."
                        >
                            <RadioButton
                                v-model="
                                    covidTreatmentAssessmentRequest.confirmsOver12
                                "
                                :question-sequence="questionSequence1"
                                :has-not-sure-option="false"
                                :has-additional-response="true"
                            />
                        </Card>
                        <Card :question-sequence="questionSequence2">
                            <RadioButton
                                v-model="
                                    covidTreatmentAssessmentRequest.testedPositiveInPast7Days
                                "
                                :question-sequence="questionSequence2"
                                :has-not-sure-option="false"
                                :has-additional-response="true"
                            />
                        </Card>
                        <Card :question-sequence="questionSequence3">
                            <RadioButton
                                v-model="
                                    covidTreatmentAssessmentRequest.hasSevereCovid19Symptoms
                                "
                                :question-sequence="questionSequence3"
                                :has-not-sure-option="true"
                            />
                        </Card>
                        <Card :question-sequence="questionSequence4">
                            <RadioButton
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
                            <RadioButton
                                v-model="
                                    covidTreatmentAssessmentRequest.hasImmunityCompromisingMedicalConditionAntiViralTri
                                "
                                :question-sequence="questionSequence6"
                                :has-not-sure-option="true"
                                :has-additional-response="true"
                            />
                        </Card>
                        <Card
                            :question-sequence="questionSequence7"
                            have-additional-info="true"
                            additional-info="Citizen has had 3 doses of vaccine for more than 14 days."
                        >
                            <RadioButton
                                v-model="
                                    covidTreatmentAssessmentRequest.reports3DosesC19Vaccine
                                "
                                :question-sequence="questionSequence7"
                                :has-not-sure-option="true"
                                :has-additional-response="true"
                            />
                        </Card>
                        <Card
                            :question-sequence="questionSequence8"
                            have-additional-info="true"
                            additional-info="Citizen has a chronic condition."
                        >
                            <RadioButton
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
