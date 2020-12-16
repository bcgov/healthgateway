<script lang="ts">
import Vue from "vue";
import { Component, Ref } from "vue-property-decorator";
import PageTitleComponent from "@/components/pageTitle.vue";
import { ILogger } from "@/services/interfaces";
import container from "@/plugins/inversify.config";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import MessageModalComponent from "@/components/modal/genericMessage.vue";
import MedicationHistoryReportComponent from "@/components/report/medicationHistory.vue";
import MSPVisitsReportComponent from "@/components/report/mspVisits.vue";
import COVID19ReportComponent from "@/components/report/covid19.vue";
import ImmunizationHistoryReportComponent from "@/components/report/immunizationHistory.vue";
import { IAuthenticationService } from "@/services/interfaces";
import type { WebClientConfiguration } from "@/models/configData";
import { Getter } from "vuex-class";

@Component({
    components: {
        PageTitleComponent,
        MessageModalComponent,
        MedicationHistoryReportComponent,
        MSPVisitsReportComponent,
        COVID19ReportComponent,
        ImmunizationHistoryReportComponent,
    },
})
export default class ReportsView extends Vue {
    @Getter("webClient", { namespace: "config" })
    config!: WebClientConfiguration;

    @Ref("messageModal")
    readonly messageModal!: MessageModalComponent;
    @Ref("medicationHistoryReport")
    readonly medicationHistoryReport!: MedicationHistoryReportComponent;
    @Ref("mspVisitsReport")
    readonly mspVisitsReport!: MSPVisitsReportComponent;
    @Ref("covid19Report")
    readonly covid19Report!: COVID19ReportComponent;
    @Ref("immunizationHistoryReport")
    readonly immunizationHistoryReport!: ImmunizationHistoryReportComponent;

    private fullName = "";
    private phn = "";
    private dateOfBirth = "";
    private logger!: ILogger;
    private reportType = "";
    private reportTypeOptions = [{ value: "", text: "Select" }];

    private mounted() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        if (this.config.modules["Medication"]) {
            this.reportTypeOptions.push({ value: "MED", text: "Medications" });
        }
        if (this.config.modules["Encounter"]) {
            this.reportTypeOptions.push({ value: "MSP", text: "MSP Visits" });
        }
        if (this.config.modules["Laboratory"]) {
            this.reportTypeOptions.push({
                value: "COVID-19",
                text: "COVID-19 Test Results",
            });
        }
        if (this.config.modules["Immunization"]) {
            this.reportTypeOptions.push({
                value: "Immunization",
                text: "Immunizations",
            });
        }
        this.loadName();
    }

    private showConfirmationModal() {
        this.messageModal.showModal();
    }

    private downloadPdf() {
        switch (this.reportType) {
            case "MED":
                this.medicationHistoryReport.generatePdf();
                break;
            case "MSP":
                this.mspVisitsReport.generatePdf();
                break;
            case "COVID-19":
                this.covid19Report.generatePdf();
                break;
            case "Immunization":
                this.immunizationHistoryReport.generatePdf();
                break;
        }
    }

    private loadName(): void {
        // Load the user name and current email
        let authenticationService = container.get<IAuthenticationService>(
            SERVICE_IDENTIFIER.AuthenticationService
        );
        authenticationService.getOidcUserProfile().then((oidcUser) => {
            if (oidcUser) {
                this.fullName =
                    oidcUser.given_name + " " + oidcUser.family_name;
            }
        });
    }
}
</script>

<template>
    <div>
        <div class="my-3 fluid">
            <b-row>
                <b-col
                    id="healthInsights"
                    class="col-12 col-md-10 col-lg-9 column-wrapper"
                >
                    <PageTitleComponent :title="`Export Records`" />
                    <div class="my-3 px-5 py-4 form">
                        <b-row>
                            <b-col>
                                <label for="reportType">Record Type</label>
                            </b-col>
                        </b-row>
                        <b-row>
                            <b-col>
                                <b-form-select
                                    id="reportType"
                                    v-model="reportType"
                                    data-testid="reportType"
                                    :options="reportTypeOptions"
                                >
                                </b-form-select>
                            </b-col>
                            <b-col>
                                <b-button
                                    variant="primary"
                                    data-testid="exportRecordBtn"
                                    class="ml-auto d-block"
                                    :disabled="!reportType"
                                    @click="showConfirmationModal"
                                >
                                    Download PDF
                                </b-button>
                            </b-col>
                        </b-row>
                    </div>
                    <div
                        v-if="reportType == 'MED'"
                        data-testid="medicationReportSample"
                        class="sample"
                    >
                        <MedicationHistoryReportComponent
                            ref="medicationHistoryReport"
                            :name="fullName"
                        />
                    </div>
                    <div
                        v-else-if="reportType == 'MSP'"
                        data-testid="mspVisitsReportSample"
                        class="sample"
                    >
                        <MSPVisitsReportComponent
                            ref="mspVisitsReport"
                            :name="fullName"
                        />
                    </div>
                    <div
                        v-else-if="reportType == 'COVID-19'"
                        data-testid="covid19ReportSample"
                        class="sample"
                    >
                        <COVID19ReportComponent
                            ref="covid19Report"
                            :name="fullName"
                        />
                    </div>
                    <div
                        v-else-if="reportType == 'Immunization'"
                        data-testid="immunizationHistoryReportSample"
                        class="sample"
                    >
                        <ImmunizationHistoryReportComponent
                            ref="immunizationHistoryReport"
                            :name="fullName"
                        />
                    </div>
                    <div v-else>
                        <b-row>
                            <b-col>
                                <img
                                    class="mx-auto d-block"
                                    src="@/assets/images/reports/reports.png"
                                    data-testid="infoImage"
                                    width="200"
                                    height="auto"
                                    alt="..."
                                />
                            </b-col>
                        </b-row>
                        <b-row>
                            <b-col class="text-center">
                                <h5 data-testid="infoText">
                                    Select a record type above to create a
                                    report
                                </h5>
                            </b-col>
                        </b-row>
                    </div>
                </b-col>
            </b-row>
        </div>
        <MessageModalComponent
            ref="messageModal"
            title="Sensitive Document Download"
            message="The file that you are downloading contains personal information. If you are on a public computer, please ensure that the file is deleted before you log off."
            @submit="downloadPdf"
        />
    </div>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";
.column-wrapper {
    border: 1px;
}

#pageTitle {
    color: $primary;
}

#pageTitle hr {
    border-top: 2px solid $primary;
}

.sample {
    padding: 0px 10px;
    width: 100%;
    height: 400px;
    overflow: scroll;
}
.form {
    background-color: $soft_background;
    border: $lightGrey solid 1px;
    border-radius: 5px 5px 5px 5px;
}
</style>
