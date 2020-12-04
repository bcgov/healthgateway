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
import { IAuthenticationService } from "@/services/interfaces";
import type { WebClientConfiguration } from "@/models/configData";
import { Getter } from "vuex-class";

@Component({
    components: {
        PageTitleComponent,
        MessageModalComponent,
        MedicationHistoryReportComponent,
        MSPVisitsReportComponent,
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

    private fullName = "";
    private logger!: ILogger;
    private reportType = "";
    private reportTypeOptions = [{ value: "", text: "Select a service" }];

    private mounted() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        if (this.config.modules["Medication"]) {
            this.reportTypeOptions.push({ value: "MED", text: "Medications" });
        }
        if (this.config.modules["Encounter"]) {
            this.reportTypeOptions.push({ value: "MSP", text: "MSP Visits" });
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
                    <div class="my-5 p-5 form">
                        <b-row>
                            <b-col class="col-md-6 col-12">
                                <label for="reportType">Report Type</label>
                                <b-form-select
                                    id="reportType"
                                    v-model="reportType"
                                    data-testid="reportType"
                                    :options="reportTypeOptions"
                                >
                                </b-form-select>
                            </b-col>
                            <b-col class="pt-4 col-md-6 col-12">
                                <b-button
                                    variant="primary"
                                    data-testid="exportRecordBtn"
                                    class="mx-auto mt-1 d-block"
                                    :disabled="!reportType"
                                    @click="showConfirmationModal"
                                >
                                    Export Records
                                </b-button>
                            </b-col>
                        </b-row>
                    </div>
                    <div
                        v-if="reportType == 'MED'"
                        data-testid="medicationReportSample"
                    >
                        <b-col>
                            <div class="mx-auto sample">
                                <div class="scale">
                                    <MedicationHistoryReportComponent
                                        ref="medicationHistoryReport"
                                        :name="fullName"
                                    />
                                </div>
                            </div>
                        </b-col>
                    </div>
                    <div
                        v-else-if="reportType == 'MSP'"
                        data-testid="mspVisitsReportSample"
                    >
                        <b-col>
                            <div class="mx-auto sample">
                                <div class="scale">
                                    <MSPVisitsReportComponent
                                        ref="mspVisitsReport"
                                        :name="fullName"
                                    />
                                </div>
                            </div>
                        </b-col>
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
                                    Select a service above to create a report
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
    border: 2px $lightGrey solid;
    padding: 15px;
    width: 300px;
    height: 400px;
    overflow: hidden;
}
.form {
    background-color: $soft_background;
    border: $lightGrey solid 1px;
    border-radius: 5px 5px 5px 5px;
}
.scale {
    zoom: 0.5;
    -moz-transform: scale(0.5);
}
</style>
