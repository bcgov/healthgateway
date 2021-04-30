<script lang="ts">
import Vue from "vue";
import { Component, Ref } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import DatePickerComponent from "@/components/datePicker.vue";
import LoadingComponent from "@/components/loading.vue";
import MessageModalComponent from "@/components/modal/genericMessage.vue";
import MultiSelectComponent, {
    SelectOption,
} from "@/components/multiSelect.vue";
import COVID19ReportComponent from "@/components/report/covid19.vue";
import ImmunizationHistoryReportComponent from "@/components/report/immunizationHistory.vue";
import MedicationHistoryReportComponent from "@/components/report/medicationHistory.vue";
import MedicationRequestReportComponent from "@/components/report/medicationRequest.vue";
import MSPVisitsReportComponent from "@/components/report/mspVisits.vue";
import type { WebClientConfiguration } from "@/models/configData";
import { DateWrapper, StringISODate } from "@/models/dateWrapper";
import MedicationStatementHistory from "@/models/medicationStatementHistory";
import MedicationSummary from "@/models/medicationSummary";
import PatientData from "@/models/patientData";
import ReportFilter, { ReportFilterBuilder } from "@/models/reportFilter";

@Component({
    components: {
        LoadingComponent,
        "message-modal": MessageModalComponent,
        MedicationHistoryReportComponent,
        MSPVisitsReportComponent,
        COVID19ReportComponent,
        ImmunizationHistoryReportComponent,
        MedicationRequestReportComponent,
        "hg-date-picker": DatePickerComponent,
        MultiSelectComponent,
    },
})
export default class ReportsView extends Vue {
    @Getter("webClient", { namespace: "config" })
    config!: WebClientConfiguration;

    @Getter("patientData", { namespace: "user" })
    patientData!: PatientData;
    @Getter("medicationStatements", { namespace: "medication" })
    medicationStatements!: MedicationStatementHistory[];

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
    @Ref("medicationRequestReport")
    readonly medicationRequestReport!: MedicationRequestReportComponent;

    @Action("getPatientData", { namespace: "user" })
    getPatientData!: () => Promise<void>;

    private isLoading = false;
    private isGeneratingReport = false;
    private reportType = "";
    private reportTypeOptions = [{ value: "", text: "Select" }];

    private selectedStartDate: StringISODate | null = null;
    private selectedEndDate: StringISODate | null = null;
    private selectedMedicationOptions: string[] = [];

    private reportFilter: ReportFilter = ReportFilterBuilder.create().build();

    private get medicationOptions(): SelectOption[] {
        var medications = this.medicationStatements.reduce<MedicationSummary[]>(
            (acumulator: MedicationSummary[], current) => {
                var med = current.medicationSummary;
                if (
                    acumulator.findIndex((x) => x.brandName === med.brandName) <
                    0
                ) {
                    acumulator.push(med);
                }
                return acumulator;
            },
            []
        );

        medications.sort((a, b) => a.brandName.localeCompare(b.brandName));

        return medications.map<SelectOption>((x) => {
            return {
                text: x.brandName,
                value: x.brandName,
            };
        });
    }

    private formatDate(date: string): string {
        return new DateWrapper(date).format();
    }

    private created() {
        this.getPatientData();

        if (this.config.modules["Medication"]) {
            this.reportTypeOptions.push({ value: "MED", text: "Medications" });
        }
        if (this.config.modules["Encounter"]) {
            this.reportTypeOptions.push({
                value: "MSP",
                text: "Health Visits",
            });
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
        if (this.config.modules["MedicationRequest"]) {
            this.reportTypeOptions.push({
                value: "MedicationRequest",
                text: "Special Authority Requests",
            });
        }
    }

    private clearFilter() {
        this.selectedStartDate = null;
        this.selectedEndDate = null;
        this.selectedMedicationOptions = [];
        this.updateFilter();
    }

    private clearFilterDates() {
        this.selectedStartDate = null;
        this.selectedEndDate = null;
        this.updateFilter();
    }

    private clearFilterMedication(medicationName: string) {
        var index = this.selectedMedicationOptions.indexOf(medicationName);
        if (index >= 0) {
            this.selectedMedicationOptions.splice(index, 1);
            this.updateFilter();
        }
    }

    private cancelFilter() {
        this.selectedStartDate = this.reportFilter.startDate;
        this.selectedEndDate = this.reportFilter.endDate;
        this.selectedMedicationOptions = this.reportFilter.medications;
    }

    private updateFilter() {
        this.reportFilter = ReportFilterBuilder.create()
            .withStartDate(this.selectedStartDate)
            .withEndDate(this.selectedEndDate)
            .withMedications(this.selectedMedicationOptions)
            .build();
    }

    private formatDateLong(date: string): string {
        return DateWrapper.format(date);
    }

    private showConfirmationModal() {
        this.messageModal.showModal();
    }

    private downloadPdf() {
        this.isGeneratingReport = true;
        let generatePromise: Promise<void>;
        switch (this.reportType) {
            case "MED":
                generatePromise = this.medicationHistoryReport.generatePdf();
                break;
            case "MSP":
                generatePromise = this.mspVisitsReport.generatePdf();
                break;
            case "COVID-19":
                generatePromise = this.covid19Report.generatePdf();
                break;
            case "Immunization":
                generatePromise = this.immunizationHistoryReport.generatePdf();
                break;
            case "MedicationRequest":
                generatePromise = this.medicationRequestReport.generatePdf();
                break;
            default:
                generatePromise = Promise.resolve();
        }
        generatePromise.then(() => {
            this.isGeneratingReport = false;
        });
    }
}
</script>

<template>
    <div class="m-3">
        <div>
            <b-row>
                <b-col class="col-12 col-md-10 col-lg-9 column-wrapper">
                    <page-title title="Export Records" />
                    <div class="my-3 px-3 py-4 form">
                        <b-row>
                            <b-col>
                                <label for="reportType"> Record Type </label>
                            </b-col>
                        </b-row>
                        <b-row align-h="between" class="py-2">
                            <b-col class="mb-2" md="12" lg="">
                                <b-form-select
                                    id="reportType"
                                    v-model="reportType"
                                    data-testid="reportType"
                                    :options="reportTypeOptions"
                                >
                                </b-form-select>
                            </b-col>
                            <b-col class="p-0 px-3" cols="auto">
                                <b-button
                                    v-b-toggle.advanced-panel
                                    variant="link"
                                    data-testid="advancedBtn"
                                >
                                    Advanced
                                </b-button>
                                <b-button
                                    variant="primary"
                                    data-testid="exportRecordBtn"
                                    class="mb-1 ml-2"
                                    :disabled="
                                        !reportType ||
                                        isLoading ||
                                        !patientData.hdid
                                    "
                                    @click="showConfirmationModal"
                                >
                                    Download PDF
                                </b-button>
                            </b-col>
                        </b-row>
                        <b-row
                            v-if="reportFilter.hasActiveFilter()"
                            class="pb-2"
                        >
                            <b-col v-if="reportFilter.hasDateFilter()">
                                <div><strong>Date Range</strong></div>
                                <b-form-tag
                                    variant="light"
                                    class="filter-selected"
                                    title="From"
                                    data-testid="clearFilter"
                                    :pill="true"
                                    @remove="clearFilterDates"
                                >
                                    <span data-testid="selectedDatesFilter"
                                        >{{
                                            reportFilter.startDate
                                                ? `From ${formatDateLong(
                                                      reportFilter.startDate
                                                  )}`
                                                : ""
                                        }}
                                        {{
                                            reportFilter.endDate
                                                ? ` To ${formatDateLong(
                                                      reportFilter.endDate
                                                  )}`
                                                : ""
                                        }}</span
                                    >
                                </b-form-tag>
                            </b-col>

                            <b-col
                                v-if="
                                    reportFilter.hasMedicationsFilter() &&
                                    reportType == 'MED'
                                "
                                data-testid="medicationFilter"
                            >
                                <div><strong>Exclude</strong></div>
                                <b-form-tag
                                    v-for="item in reportFilter.medications"
                                    :key="item"
                                    variant="danger"
                                    :title="item"
                                    :data-testid="item + '-clearFilter'"
                                    :pill="true"
                                    @remove="clearFilterMedication(item)"
                                >
                                    <span :data-testid="item + '-excluded'">{{
                                        item
                                    }}</span>
                                </b-form-tag>
                            </b-col>
                        </b-row>
                        <b-collapse
                            id="advanced-panel"
                            data-testid="advancedPanel"
                            class="border-top"
                        >
                            <b-row>
                                <b-col class="col-12 col-lg-4 pt-3">
                                    <label for="start-date">From</label>
                                    <hg-date-picker
                                        id="start-date"
                                        v-model="selectedStartDate"
                                        data-testid="startDateInput"
                                    />
                                </b-col>
                                <b-col class="col-12 col-lg-4 pt-3">
                                    <label for="end-date">To</label>
                                    <hg-date-picker
                                        id="end-date"
                                        v-model="selectedEndDate"
                                        data-testid="endDateInput"
                                    />
                                </b-col>
                            </b-row>
                            <b-row v-show="reportType == 'MED'" class="pt-3">
                                <b-col>
                                    <div>
                                        <strong>Exclude These Records</strong>
                                    </div>
                                    <div>Medications:</div>
                                    <MultiSelectComponent
                                        v-model="selectedMedicationOptions"
                                        placeholder="Choose a medication"
                                        :options="medicationOptions"
                                        data-testid="medicationExclusionFilter"
                                    ></MultiSelectComponent>
                                </b-col>
                            </b-row>
                            <b-row align-h="end" class="pt-4">
                                <b-col cols="auto">
                                    <b-button
                                        v-b-toggle.advanced-panel
                                        variant="link"
                                        data-testid="clearBtn"
                                        class="mb-1 mr-2 text-muted"
                                        :disabled="isLoading"
                                        @click="cancelFilter"
                                    >
                                        Cancel
                                    </b-button>
                                    <b-button
                                        v-b-toggle.advanced-panel
                                        variant="primary"
                                        data-testid="applyFilterBtn"
                                        class="mb-1 ml-2"
                                        :disabled="isLoading"
                                        @click="updateFilter"
                                    >
                                        Apply
                                    </b-button>
                                </b-col>
                            </b-row>
                        </b-collapse>
                    </div>
                    <LoadingComponent
                        v-if="isLoading || isGeneratingReport"
                        :is-loading="isLoading || isGeneratingReport"
                        :is-custom="!isGeneratingReport"
                        :backdrop="false"
                    ></LoadingComponent>
                    <div
                        v-if="reportType == 'MED'"
                        data-testid="medicationReportSample"
                        class="sample d-none d-md-block"
                    >
                        <MedicationHistoryReportComponent
                            ref="medicationHistoryReport"
                            :filter="reportFilter"
                            @on-is-loading-changed="isLoading = $event"
                        />
                    </div>
                    <div
                        v-else-if="reportType == 'MSP'"
                        data-testid="mspVisitsReportSample"
                        class="sample d-none d-md-block"
                    >
                        <MSPVisitsReportComponent
                            ref="mspVisitsReport"
                            :filter="reportFilter"
                            @on-is-loading-changed="isLoading = $event"
                        />
                    </div>
                    <div
                        v-else-if="reportType == 'COVID-19'"
                        data-testid="covid19ReportSample"
                        class="sample d-none d-md-block"
                    >
                        <COVID19ReportComponent
                            ref="covid19Report"
                            :filter="reportFilter"
                            @on-is-loading-changed="isLoading = $event"
                        />
                    </div>
                    <div
                        v-else-if="reportType == 'Immunization'"
                        data-testid="immunizationHistoryReportSample"
                        class="sample d-none d-md-block"
                    >
                        <ImmunizationHistoryReportComponent
                            ref="immunizationHistoryReport"
                            :filter="reportFilter"
                            @on-is-loading-changed="isLoading = $event"
                        />
                    </div>
                    <div
                        v-else-if="reportType == 'MedicationRequest'"
                        data-testid="medicationRequestReportSample"
                        class="sample d-none d-md-block"
                    >
                        <MedicationRequestReportComponent
                            ref="medicationRequestReport"
                            :filter="reportFilter"
                            @on-is-loading-changed="isLoading = $event"
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
        <message-modal
            ref="messageModal"
            data-testid="messageModal"
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
    height: 600px;
    overflow-y: scroll;
    overflow-x: hidden;
}
.form {
    background-color: $soft_background;
    border: $lightGrey solid 1px;
    border-radius: 5px 5px 5px 5px;
}
.filter-selected {
    background-color: $aquaBlue;
    color: white;
}
</style>
