<script lang="ts">
import { saveAs } from "file-saver";
import Vue from "vue";
import { Component, Prop, Ref } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import DatePickerComponent from "@/components/DatePickerComponent.vue";
import LoadingComponent from "@/components/LoadingComponent.vue";
import MessageModalComponent from "@/components/modal/MessageModalComponent.vue";
import MultiSelectComponent, {
    SelectOption,
} from "@/components/MultiSelectComponent.vue";
import Covid19ReportComponent from "@/components/report/Covid19ReportComponent.vue";
import HospitalVisitReportComponent from "@/components/report/HospitalVisitReportComponent.vue";
import ImmunizationHistoryReportComponent from "@/components/report/ImmunizationHistoryReportComponent.vue";
import LaboratoryReportComponent from "@/components/report/LaboratoryReportComponent.vue";
import MedicationHistoryReportComponent from "@/components/report/MedicationHistoryReportComponent.vue";
import MedicationRequestReportComponent from "@/components/report/MedicationRequestReportComponent.vue";
import MSPVisitsReportComponent from "@/components/report/MSPVisitsReportComponent.vue";
import NotesReportComponent from "@/components/report/NotesReportComponent.vue";
import { EntryType } from "@/constants/entryType";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import type { WebClientConfiguration } from "@/models/configData";
import { DateWrapper, StringISODate } from "@/models/dateWrapper";
import { ResultError } from "@/models/errors";
import MedicationStatementHistory from "@/models/medicationStatementHistory";
import MedicationSummary from "@/models/medicationSummary";
import PatientData from "@/models/patientData";
import Report from "@/models/report";
import { ReportFilterBuilder } from "@/models/reportFilter";
import ReportHeader from "@/models/reportHeader";
import { ReportFormatType } from "@/models/reportRequest";
import RequestResult from "@/models/requestResult";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger } from "@/services/interfaces";
import ConfigUtil from "@/utility/configUtil";
import EventTracker from "@/utility/eventTracker";

const medicationReport = "medication-report";
const mspVisitReport = "msp-visit-report";
const covid19Report = "covid19-report";
const immunizationReport = "immunization-report";
const medicationRequestReport = "medication-request-report";
const noteReport = "note-report";
const laboratoryReport = "laboratory-report";
const hospitalVisitReport = "hospital-visit-report";

// eslint-disable-next-line @typescript-eslint/no-explicit-any
const options: any = {
    components: {
        LoadingComponent,
        "message-modal": MessageModalComponent,
        medicationReport: MedicationHistoryReportComponent,
        mspVisitReport: MSPVisitsReportComponent,
        covid19Report: Covid19ReportComponent,
        immunizationReport: ImmunizationHistoryReportComponent,
        medicationRequestReport: MedicationRequestReportComponent,
        "hg-date-picker": DatePickerComponent,
        MultiSelectComponent,
        noteReport: NotesReportComponent,
        laboratoryReport: LaboratoryReportComponent,
        HospitalVisitReport: HospitalVisitReportComponent,
    },
};

@Component(options)
export default class ReportsComponent extends Vue {
    @Prop({ required: true })
    hdid!: string;

    @Getter("webClient", { namespace: "config" })
    config!: WebClientConfiguration;

    @Getter("laboratoryOrdersAreQueued", { namespace: "laboratory" })
    laboratoryOrdersAreQueued!: (hdid: string) => boolean;

    @Getter("medications", { namespace: "medication" })
    medications!: (hdid: string) => MedicationStatementHistory[];

    @Getter("patientData", { namespace: "user" })
    patientData!: PatientData;

    @Ref("messageModal")
    readonly messageModal!: MessageModalComponent;

    @Ref("report")
    readonly report!: {
        generateReport: (
            reportFormatType: ReportFormatType,
            headerData: ReportHeader
        ) => Promise<RequestResult<Report>>;
    };

    @Action("addError", { namespace: "errorBanner" })
    addError!: (params: {
        errorType: ErrorType;
        source: ErrorSourceType;
        traceId: string | undefined;
    }) => void;

    @Action("setTooManyRequestsError", { namespace: "errorBanner" })
    setTooManyRequestsError!: (params: { key: string }) => void;

    ReportFormatType: unknown = ReportFormatType;
    isLoading = false;
    isGeneratingReport = false;
    reportFormatType = ReportFormatType.PDF;
    reportComponentName = "";
    reportTypeOptions = [{ value: "", text: "Select" }];

    selectedStartDate: StringISODate | null = null;
    selectedEndDate: StringISODate | null = null;
    selectedMedicationOptions: string[] = [];

    hasRecords = false;

    reportFilter = ReportFilterBuilder.create().build();
    isReportFilterStartDateValidDate = true;
    isReportFilterEndDateValidDate = true;

    logger!: ILogger;

    get showLaboratoryOrderQueuedMessage(): boolean {
        return (
            this.reportComponentName === laboratoryReport &&
            this.laboratoryOrdersAreQueued(this.hdid)
        );
    }

    get headerData(): ReportHeader {
        return {
            phn: this.patientData.personalHealthNumber,
            dateOfBirth: this.formatDate(this.patientData.birthdate || ""),
            name: this.patientData
                ? this.patientData.preferredName.givenName +
                  " " +
                  this.patientData.preferredName.surname
                : "",
            isRedacted: this.reportFilter.hasMedicationsFilter(),
            datePrinted: new DateWrapper(new DateWrapper().toISO()).format(),
            filterText: this.reportFilter.filterText,
        };
    }

    get isMedicationReport(): boolean {
        return this.reportComponentName === medicationReport;
    }

    get medicationOptions(): SelectOption[] {
        let records = this.medications(this.hdid).reduce<MedicationSummary[]>(
            (acumulator: MedicationSummary[], current) => {
                let med = current.medicationSummary;
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

        records.sort((a, b) => a.brandName.localeCompare(b.brandName));

        return records.map<SelectOption>((x) => ({
            text: x.brandName,
            value: x.brandName,
        }));
    }

    get isDownloadDisabled(): boolean {
        this.logger.debug(`Report Component Name: ${this.reportComponentName}`);
        return (
            this.isLoading ||
            !this.reportComponentName ||
            !this.patientData.hdid ||
            !this.hasRecords
        );
    }

    formatDate(date: string): string {
        return DateWrapper.format(date);
    }

    created(): void {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);

        if (ConfigUtil.isDatasetEnabled(EntryType.Medication)) {
            this.reportTypeOptions.push({
                value: medicationReport,
                text: "Medications",
            });
        }
        if (ConfigUtil.isDatasetEnabled(EntryType.HealthVisit)) {
            this.reportTypeOptions.push({
                value: mspVisitReport,
                text: "Health Visits",
            });
        }
        if (ConfigUtil.isDatasetEnabled(EntryType.Covid19TestResult)) {
            this.reportTypeOptions.push({
                value: covid19Report,
                text: "COVID‑19 Test Results",
            });
        }
        if (ConfigUtil.isDatasetEnabled(EntryType.Immunization)) {
            this.reportTypeOptions.push({
                value: immunizationReport,
                text: "Immunizations",
            });
        }
        if (ConfigUtil.isDatasetEnabled(EntryType.SpecialAuthorityRequest)) {
            this.reportTypeOptions.push({
                value: medicationRequestReport,
                text: "Special Authority Requests",
            });
        }
        if (ConfigUtil.isDatasetEnabled(EntryType.Note)) {
            this.reportTypeOptions.push({
                value: noteReport,
                text: "My Notes",
            });
        }
        if (ConfigUtil.isDatasetEnabled(EntryType.LabResult)) {
            this.reportTypeOptions.push({
                value: laboratoryReport,
                text: "Laboratory Tests",
            });
        }
        if (ConfigUtil.isDatasetEnabled(EntryType.HospitalVisit)) {
            this.reportTypeOptions.push({
                value: hospitalVisitReport,
                text: "Hospital Visits",
            });
        }
    }

    clearFilter(): void {
        this.selectedStartDate = null;
        this.selectedEndDate = null;
        this.selectedMedicationOptions = [];
        this.updateFilter();
    }

    clearFilterDates(): void {
        this.selectedStartDate = null;
        this.selectedEndDate = null;
        this.updateFilter();
    }

    clearFilterMedication(medicationName: string): void {
        let index = this.selectedMedicationOptions.indexOf(medicationName);
        if (index >= 0) {
            this.selectedMedicationOptions.splice(index, 1);
            this.updateFilter();
        }
    }

    cancelFilter(): void {
        this.selectedStartDate = this.convertEmptyStringDateToNull(
            this.reportFilter.startDate
        );
        this.selectedEndDate = this.convertEmptyStringDateToNull(
            this.reportFilter.endDate
        );
        this.selectedMedicationOptions = this.reportFilter.medications;
    }

    updateFilter(): void {
        this.reportFilter = ReportFilterBuilder.create()
            .withStartDate(
                this.convertEmptyStringDateToNull(this.selectedStartDate)
            )
            .withEndDate(
                this.convertEmptyStringDateToNull(this.selectedEndDate)
            )
            .withMedications(this.selectedMedicationOptions)
            .build();
    }

    convertEmptyStringDateToNull(date: StringISODate | null): string | null {
        return !date ? null : date;
    }

    showConfirmationModal(reportFormatType: ReportFormatType): void {
        this.reportFormatType = reportFormatType;
        this.messageModal.showModal();
    }

    downloadReport(): void {
        if (this.reportComponentName === "") {
            return;
        }

        this.isGeneratingReport = true;

        this.trackDownload();

        this.report
            .generateReport(this.reportFormatType, this.headerData)
            .then((result: RequestResult<Report>) => {
                const mimeType = this.getMimeType(this.reportFormatType);
                const downloadLink = `data:${mimeType};base64,${result.resourcePayload.data}`;
                fetch(downloadLink).then((res) =>
                    res
                        .blob()
                        .then((blob) =>
                            saveAs(blob, result.resourcePayload.fileName)
                        )
                );
            })
            .catch((err: ResultError) => {
                this.logger.error(err.resultMessage);
                if (err.statusCode === 429) {
                    this.setTooManyRequestsError({ key: "page" });
                } else {
                    this.addError({
                        errorType: ErrorType.Download,
                        source: ErrorSourceType.ExportRecords,
                        traceId: err.traceId,
                    });
                }
            })
            .finally(() => {
                this.isGeneratingReport = false;
            });
    }

    getMimeType(reportFormatType: ReportFormatType): string {
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

    trackDownload(): void {
        let reportName = "";
        switch (this.reportComponentName) {
            case medicationReport:
                reportName = "Medication";
                break;
            case mspVisitReport:
                reportName = "Health Visits";
                break;
            case covid19Report:
                reportName = "COVID‑19 Test";
                break;
            case immunizationReport:
                reportName = "Immunization";
                break;
            case medicationRequestReport:
                reportName = "Special Authority Requests";
                break;
            case noteReport:
                reportName = "Notes";
                break;
            case laboratoryReport:
                reportName = "Laboratory Tests";
                break;
            case hospitalVisitReport:
                reportName = "Hospital Visits";
                break;
        }
        if (reportName !== "") {
            const formatTypeName = ReportFormatType[this.reportFormatType];
            const eventName = `${reportName} (${formatTypeName})`;

            EventTracker.downloadReport(eventName);
        }
    }
}
</script>

<template>
    <div>
        <b-alert
            v-if="showLaboratoryOrderQueuedMessage"
            show
            dismissible
            variant="info"
            class="no-print"
            data-testid="laboratory-orders-queued-alert-message"
        >
            <span>
                We are getting your lab results. It may take up to 48 hours
                until you can see them.
            </span>
        </b-alert>
        <div class="my-3 px-3 py-4 form">
            <label for="reportType">Record Type</label>
            <b-row align-h="between" class="py-2">
                <b-col class="mb-2" sm="">
                    <b-form-select
                        id="reportType"
                        v-model="reportComponentName"
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
                        >Advanced
                    </hg-button>
                    <b-dropdown
                        id="exportRecordBtn"
                        text="Download"
                        class="mb-1 ml-2"
                        variant="primary"
                        data-testid="exportRecordBtn"
                        :disabled="isDownloadDisabled"
                    >
                        <b-dropdown-item
                            @click="showConfirmationModal(ReportFormatType.PDF)"
                            >PDF</b-dropdown-item
                        >
                        <b-dropdown-item
                            @click="showConfirmationModal(ReportFormatType.CSV)"
                            >CSV</b-dropdown-item
                        >
                        <b-dropdown-item
                            @click="
                                showConfirmationModal(ReportFormatType.XLSX)
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
                        variant="light"
                        :title="item"
                        :data-testid="item + '-clearFilter'"
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
                            @is-date-valid="
                                isReportFilterStartDateValidDate = $event
                            "
                        />
                    </b-col>
                    <b-col class="col-12 col-lg-4 pt-3">
                        <label for="end-date">To</label>
                        <hg-date-picker
                            id="end-date"
                            v-model="selectedEndDate"
                            data-testid="endDateInput"
                            @is-date-valid="
                                isReportFilterEndDateValidDate = $event
                            "
                        />
                    </b-col>
                </b-row>
                <div v-show="isMedicationReport" class="pt-3">
                    <div>
                        <strong>Exclude These Records</strong>
                    </div>
                    <div>Medications:</div>
                    <MultiSelectComponent
                        v-model="selectedMedicationOptions"
                        placeholder="Choose a medication"
                        :options="medicationOptions"
                        data-testid="medicationExclusionFilter"
                    />
                </div>
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
                            :disabled="
                                isLoading ||
                                !isReportFilterStartDateValidDate ||
                                !isReportFilterEndDateValidDate
                            "
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
        />
        <div
            v-if="reportComponentName"
            data-testid="reportSample"
            class="preview"
        >
            <component
                :is="reportComponentName"
                ref="report"
                :hdid="hdid"
                :filter="reportFilter"
                @on-is-loading-changed="isLoading = $event"
                @on-is-empty-changed="hasRecords = !$event"
            />
        </div>
        <div v-else>
            <img
                class="mx-auto d-block"
                src="@/assets/images/reports/reports.png"
                data-testid="infoImage"
                width="200"
                height="auto"
                alt="..."
            />
            <h5 data-testid="infoText" class="text-center">
                Select a record type above to create a report
            </h5>
        </div>

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

.preview {
    width: 100%;
    height: 600px;
    overflow-y: scroll;
    overflow-x: scroll;
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
