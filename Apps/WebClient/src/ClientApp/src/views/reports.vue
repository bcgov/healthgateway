<script lang="ts">
import { saveAs } from "file-saver";
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
import ResourceCentreComponent from "@/components/resourceCentre.vue";
import type { WebClientConfiguration } from "@/models/configData";
import { DateWrapper, StringISODate } from "@/models/dateWrapper";
import MedicationStatementHistory from "@/models/medicationStatementHistory";
import MedicationSummary from "@/models/medicationSummary";
import PatientData from "@/models/patientData";
import Report from "@/models/report";
import ReportFilter, { ReportFilterBuilder } from "@/models/reportFilter";
import ReportHeader from "@/models/reportHeader";
import { ReportFormatType } from "@/models/reportRequest";
import RequestResult from "@/models/requestResult";
import SnowPlow from "@/utility/snowPlow";

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
        "resource-centre": ResourceCentreComponent,
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

    @Ref("report")
    readonly report!: {
        generateReport: (
            reportFormatType: ReportFormatType,
            headerData: ReportHeader
        ) => Promise<RequestResult<Report>>;
    };

    @Action("getPatientData", { namespace: "user" })
    getPatientData!: () => Promise<void>;

    private ReportFormatType: unknown = ReportFormatType;
    private isLoading = false;
    private isGeneratingReport = false;
    private reportFormatType = ReportFormatType.PDF;
    private reportComponent = "";
    private reportTypeOptions = [{ value: "", text: "Select" }];

    private selectedStartDate: StringISODate | null = null;
    private selectedEndDate: StringISODate | null = null;
    private selectedMedicationOptions: string[] = [];

    private reportFilter: ReportFilter = ReportFilterBuilder.create().build();

    private get headerData(): ReportHeader {
        return {
            phn: this.patientData.personalhealthnumber,
            dateOfBirth: this.formatDate(this.patientData.birthdate || ""),
            name: this.patientData
                ? this.patientData.firstname + " " + this.patientData.lastname
                : "",
            isRedacted: this.reportFilter.hasMedicationsFilter(),
            datePrinted: new DateWrapper(new DateWrapper().toISO()).format(),
            filterText: this.reportFilter.filterText,
        };
    }

    private get isMedicationReport() {
        return this.reportComponent == "MedicationHistoryReportComponent";
    }

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
        return DateWrapper.format(date);
    }

    private created() {
        this.getPatientData();

        if (this.config.modules["Medication"]) {
            this.reportTypeOptions.push({
                value: "MedicationHistoryReportComponent",
                text: "Medications",
            });
        }
        if (this.config.modules["Encounter"]) {
            this.reportTypeOptions.push({
                value: "MSPVisitsReportComponent",
                text: "Health Visits",
            });
        }
        if (this.config.modules["Laboratory"]) {
            this.reportTypeOptions.push({
                value: "COVID19ReportComponent",
                text: "COVID-19 Test Results",
            });
        }
        if (this.config.modules["Immunization"]) {
            this.reportTypeOptions.push({
                value: "ImmunizationHistoryReportComponent",
                text: "Immunizations",
            });
        }
        if (this.config.modules["MedicationRequest"]) {
            this.reportTypeOptions.push({
                value: "MedicationRequestReportComponent",
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

    private showConfirmationModal(reportFormatType: ReportFormatType) {
        this.reportFormatType = reportFormatType;
        this.messageModal.showModal();
    }

    private downloadReport() {
        this.isGeneratingReport = true;
        SnowPlow.trackEvent({
            action: "download_report",
            text: `${this.reportComponent} ${this.reportFormatType}`,
        });

        this.report
            .generateReport(this.reportFormatType, this.headerData)
            .then((result: RequestResult<Report>) => {
                const mimeType = this.getMimeType(this.reportFormatType);
                const downloadLink = `data:${mimeType};base64,${result.resourcePayload.data}`;
                fetch(downloadLink).then((res) => {
                    res.blob().then((blob) => {
                        saveAs(blob, result.resourcePayload.fileName);
                    });
                });
            })
            .finally(() => {
                this.isGeneratingReport = false;
            });
    }

    private getMimeType(reportFormatType: ReportFormatType) {
        switch (reportFormatType) {
            case ReportFormatType.PDF:
                return "application/pdf";
            case ReportFormatType.CSV:
                return "text/csv";
            case ReportFormatType.XLSX:
                return "application/vnd.openxmlformats";
            default:
                return "";
        }
    }
}
</script>

<template>
    <div class="m-3 flex-grow-1 d-flex flex-column">
        <b-row>
            <b-col class="col-12 col-lg-9 column-wrapper">
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
                                v-model="reportComponent"
                                data-testid="reportType"
                                :options="reportTypeOptions"
                            >
                            </b-form-select>
                        </b-col>
                        <b-col class="p-0 px-3" cols="auto">
                            <hg-button
                                v-b-toggle.advanced-panel
                                variant="link"
                                data-testid="advancedBtn"
                            >
                                Advanced
                            </hg-button>
                            <b-dropdown
                                id="exportRecordBtn"
                                text="Download"
                                class="mb-1 ml-2"
                                variant="primary"
                                data-testid="exportRecordBtn"
                                :disabled="
                                    !reportComponent ||
                                    isLoading ||
                                    !patientData.hdid
                                "
                            >
                                <b-dropdown-item
                                    @click="
                                        showConfirmationModal(
                                            ReportFormatType.PDF
                                        )
                                    "
                                    >PDF</b-dropdown-item
                                >
                                <b-dropdown-item
                                    @click="
                                        showConfirmationModal(
                                            ReportFormatType.CSV
                                        )
                                    "
                                    >CSV</b-dropdown-item
                                >
                                <b-dropdown-item
                                    @click="
                                        showConfirmationModal(
                                            ReportFormatType.XLSX
                                        )
                                    "
                                    >XLSX</b-dropdown-item
                                >
                            </b-dropdown>
                        </b-col>
                    </b-row>
                    <b-row v-if="reportFilter.hasActiveFilter()" class="pb-2">
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
                                            ? `From ${formatDate(
                                                  reportFilter.startDate
                                              )}`
                                            : ""
                                    }}
                                    {{
                                        reportFilter.endDate
                                            ? ` Up To ${formatDate(
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
                                isMedicationReport
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
                        <b-row v-show="isMedicationReport" class="pt-3">
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
                                <hg-button
                                    v-b-toggle.advanced-panel
                                    variant="secondary"
                                    data-testid="clearBtn"
                                    class="mb-1 mr-1"
                                    :disabled="isLoading"
                                    @click="cancelFilter"
                                >
                                    Cancel
                                </hg-button>
                                <hg-button
                                    v-b-toggle.advanced-panel
                                    variant="primary"
                                    data-testid="applyFilterBtn"
                                    class="mb-1 ml-1"
                                    :disabled="isLoading"
                                    @click="updateFilter"
                                >
                                    Apply
                                </hg-button>
                            </b-col>
                        </b-row>
                    </b-collapse>
                </div>
                <LoadingComponent
                    :is-loading="isLoading || isGeneratingReport"
                    :is-custom="!isGeneratingReport"
                    :full-screen="false"
                ></LoadingComponent>
                <div
                    v-if="reportComponent"
                    data-testid="reportSample"
                    class="sample d-none d-md-block"
                >
                    <component
                        :is="reportComponent"
                        ref="report"
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
                                Select a record type above to create a report
                            </h5>
                        </b-col>
                    </b-row>
                </div>
            </b-col>
        </b-row>

        <resource-centre />
        <message-modal
            ref="messageModal"
            data-testid="messageModal"
            title="Sensitive Document Download"
            message="The file that you are downloading contains personal information. If you are on a public computer, please ensure that the file is deleted before you log off."
            @submit="downloadReport"
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
