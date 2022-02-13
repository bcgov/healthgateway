<script lang="ts">
import Vue from "vue";
import { Component } from "vue-property-decorator";

import Card from "@/components/covidTreatmentAssessment/Card.vue";
import RadioButton from "@/components/covidTreatmentAssessment/RadioButton.vue";
import { CovidTherapyAssessmentOption } from "@/constants/covidTherapyAssessmentOption";
import CovidTherapyAssessmentRequest from "@/models/CovidTherapyAssessmentRequest";

@Component({
    components: { Card, RadioButton },
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

    private covidTherapyAssessmentRequest: CovidTherapyAssessmentRequest = {
        phn: "9233238391",
        firstName: "Princess",
        lastName: "Agustin",
        phoneNumber: "",
        identifiesIndigenous: CovidTherapyAssessmentOption.Unspecified,
        hasAFamilyDoctorOrNp: CovidTherapyAssessmentOption.Unspecified,
        confirmsOver12: false,
        testedPositiveInPast7Days: CovidTherapyAssessmentOption.Unspecified,
        hasSevereCovid19Symptoms: CovidTherapyAssessmentOption.Unspecified,
        hasMildOrModerateCovid19Symptoms: false,
        symptomOnSetDate: "",
        hasImmunityCompromisingMedicalConditionAntiViralTri:
            CovidTherapyAssessmentOption.Unspecified,
        reports3DosesC19Vaccine: CovidTherapyAssessmentOption.Unspecified,
        hasChronicConditionDiagnoses: CovidTherapyAssessmentOption.Unspecified,
        agentComments: "",
        streetAddress: "",
        provOrState: "",
        postalCode: "",
        country: "",
        changeAddressFlag: false,
    };

    private get patientFullName() {
        return `${this.covidTherapyAssessmentRequest.firstName} ${this.covidTherapyAssessmentRequest.lastName} `;
    }

    private submit() {
        alert(this.covidTherapyAssessmentRequest.identifiesIndigenous);
        alert(this.covidTherapyAssessmentRequest.hasAFamilyDoctorOrNp);
    }
}
</script>
<template>
    <v-container>
        <v-row no-gutters>
            <v-col cols="12" sm="12" md="10">
                <v-form autocomplete="false">
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
                                <label for="name">{{ patientFullName }}</label>
                            </div>
                        </v-col>
                        <v-col>
                            <div>Birthdate</div>
                            <div><label for="birthdate">1950-03-24</label></div>
                        </v-col>
                        <v-col>
                            <div>PHN</div>
                            <div>
                                <label for="phn">{{
                                    covidTherapyAssessmentRequest.phn
                                }}</label>
                            </div>
                        </v-col>
                    </v-row>
                    <br />
                    <v-row dense>
                        <v-col cols="auto">
                            <v-text-field readonly label="Phone Number" />
                        </v-col>
                    </v-row>
                    <Card :question-sequence="questionSequenceA">
                        <RadioButton
                            v-model="
                                covidTherapyAssessmentRequest.identifiesIndigenous
                            "
                            :question-sequence="questionSequenceA"
                            :has-additional-response="false"
                        />
                    </Card>
                    <Card :question-sequence="questionSequenceB">
                        <RadioButton
                            v-model="
                                covidTherapyAssessmentRequest.hasAFamilyDoctorOrNp
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
                                covidTherapyAssessmentRequest.confirmsOver12
                            "
                            :question-sequence="questionSequence1"
                            :has-not-sure-option="false"
                            :has-additional-response="true"
                        />
                    </Card>
                    <Card :question-sequence="questionSequence2">
                        <RadioButton
                            v-model="
                                covidTherapyAssessmentRequest.testedPositiveInPast7Days
                            "
                            :question-sequence="questionSequence2"
                            :has-not-sure-option="false"
                            :has-additional-response="true"
                        />
                    </Card>
                    <Card :question-sequence="questionSequence3">
                        <RadioButton
                            v-model="
                                covidTherapyAssessmentRequest.hasSevereCovid19Symptoms
                            "
                            :question-sequence="questionSequence3"
                            :has-not-sure-option="true"
                        />
                    </Card>
                    <Card :question-sequence="questionSequence4">
                        <RadioButton
                            v-model="
                                covidTherapyAssessmentRequest.hasMildOrModerateCovid19Symptoms
                            "
                            :question-sequence="questionSequence4"
                            :has-not-sure-option="true"
                            :has-additional-response="true"
                        />
                    </Card>
                    <Card :question-sequence="questionSequence5">
                        <div style="width: 250px">
                            <v-dialog ref="dailyDialog" persistent>
                                <template #activator="{ on, attrs }">
                                    <v-row>
                                        <v-col>
                                            <v-text-field
                                                label="Date"
                                                prepend-icon="mdi-calendar"
                                                readonly
                                                v-bind="attrs"
                                                v-on="on"
                                            ></v-text-field>
                                        </v-col>
                                    </v-row>
                                </template>
                                <v-date-picker range scrollable no-title>
                                    <v-spacer></v-spacer>
                                    <v-btn text color="primary"> Cancel </v-btn>
                                    <v-btn text color="primary"> OK </v-btn>
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
                                covidTherapyAssessmentRequest.hasImmunityCompromisingMedicalConditionAntiViralTri
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
                                covidTherapyAssessmentRequest.reports3DosesC19Vaccine
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
                                covidTherapyAssessmentRequest.hasChronicConditionDiagnoses
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
                                    covidTherapyAssessmentRequest.agentComments
                                "
                                class="comment-input"
                                rows="6"
                                max-rows="15"
                                maxlength="2000"
                                style="overflow: auto"
                            ></b-form-textarea>
                        </div>
                    </Card>
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
                </v-form>
            </v-col>
        </v-row>
    </v-container>
</template>

<style lang="scss">
.comment-input {
    border-right: 0px;
    background-color: white;
    width: 560px;
}
</style>
