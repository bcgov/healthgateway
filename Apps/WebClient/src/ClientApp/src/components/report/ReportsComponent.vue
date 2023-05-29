<script lang="ts">
import { saveAs } from "file-saver";
import Vue, { Component } from "vue";
import {
    Component as ComponentDecorator,
    Prop,
    Ref,
} from "vue-property-decorator";
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
import { EntryType, entryTypeMap } from "@/constants/entryType";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import type { WebClientConfiguration } from "@/models/configData";
import { DateWrapper, StringISODate } from "@/models/dateWrapper";
import { Dependent } from "@/models/dependent";
import { ResultError } from "@/models/errors";
import MedicationStatementHistory from "@/models/medicationStatementHistory";
import MedicationSummary from "@/models/medicationSummary";
import Patient from "@/models/patient";
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

const reportComponentMap = new Map<EntryType, Component>([
    [EntryType.Medication, MedicationHistoryReportComponent],
    [EntryType.HealthVisit, MSPVisitsReportComponent],
    [EntryType.Covid19TestResult, Covid19ReportComponent],
    [EntryType.Immunization, ImmunizationHistoryReportComponent],
    [EntryType.SpecialAuthorityRequest, MedicationRequestReportComponent],
    [EntryType.Note, NotesReportComponent],
    [EntryType.LabResult, LaboratoryReportComponent],
    [EntryType.HospitalVisit, HospitalVisitReportComponent],
]);

// eslint-disable-next-line @typescript-eslint/no-explicit-any
const options: any = {
    components: {
        LoadingComponent,
        MessageModalComponent,
        MedicationHistoryReportComponent,
        MSPVisitsReportComponent,
        Covid19ReportComponent,
        ImmunizationHistoryReportComponent,
        MedicationRequestReportComponent,
        DatePickerComponent,
        MultiSelectComponent,
        NotesReportComponent,
        LaboratoryReportComponent,
        HospitalVisitReportComponent,
    },
};

@ComponentDecorator(options)
export default class ReportsComponent extends Vue {
    @Prop({ required: true })
    hdid!: string;

    @Prop({ default: false }) private isDependent!: boolean;

    @Getter("webClient", { namespace: "config" })
    config!: WebClientConfiguration;

    @Getter("laboratoryOrdersAreQueued", { namespace: "laboratory" })
    laboratoryOrdersAreQueued!: (hdid: string) => boolean;

    @Getter("medications", { namespace: "medication" })
    medications!: (hdid: string) => MedicationStatementHistory[];

    @Getter("patient", { namespace: "user" })
    patient!: Patient;

    @Getter("dependents", { namespace: "dependent" })
    private dependents!: Dependent[];

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
    selectedEntryType: EntryType | "" = "";
    reportTypeOptions = [{ value: "", text: "Select" }];

    selectedStartDate: StringISODate | null = null;
    selectedEndDate: StringISODate | null = null;
    selectedMedicationOptions: string[] = [];

    hasRecords = false;

    reportFilter = ReportFilterBuilder.create().build();
    isReportFilterStartDateValidDate = true;
    isReportFilterEndDateValidDate = true;

    logger!: ILogger;

    get selectedReportComponent(): Component | string {
        if (!this.selectedEntryType) {
            return "";
        }

        return reportComponentMap.get(this.selectedEntryType) ?? "";
    }

    get showLaboratoryOrderQueuedMessage(): boolean {
        return (
            this.selectedEntryType === EntryType.LabResult &&
            this.laboratoryOrdersAreQueued(this.hdid)
        );
    }

    get headerData(): ReportHeader {
        const dependent = this.dependents.find(
            (d) => d.dependentInformation.hdid === this.hdid
        );
        if (dependent) {
            return {
                phn: dependent.dependentInformation.PHN,
                dateOfBirth: this.formatDate(
                    dependent.dependentInformation.dateOfBirth || ""
                ),
                name: dependent.dependentInformation
                    ? dependent.dependentInformation.firstname +
                      " " +
                      dependent.dependentInformation.lastname
                    : "",
                isRedacted: this.reportFilter.hasMedicationsFilter(),
                datePrinted: new DateWrapper(
                    new DateWrapper().toISO()
                ).format(),
                filterText: this.reportFilter.filterText,
            };
        } else {
            return {
                phn: this.patient.personalHealthNumber,
                dateOfBirth: this.formatDate(this.patient.birthdate || ""),
                name: this.patient
                    ? this.patient.preferredName.givenName +
                      " " +
                      this.patient.preferredName.surname
                    : "",
                isRedacted: this.reportFilter.hasMedicationsFilter(),
                datePrinted: new DateWrapper(
                    new DateWrapper().toISO()
                ).format(),
                filterText: this.reportFilter.filterText,
            };
        }
    }

    get isMedicationReport(): boolean {
        return this.selectedEntryType === EntryType.Medication;
    }

    get medicationOptions(): SelectOption[] {
        const records = this.medications(this.hdid).reduce<MedicationSummary[]>(
            (acumulator: MedicationSummary[], current) => {
                const med = current.medicationSummary;
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
        return (
            this.isLoading ||
            !this.selectedEntryType ||
            !this.patient.hdid ||
            !this.hasRecords
        );
    }

    formatDate(date: string): string {
        return DateWrapper.format(date);
    }

    created(): void {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);

        const isEnabled = this.isDependent
            ? ConfigUtil.isDependentDatasetEnabled
            : ConfigUtil.isDatasetEnabled;

        for (const [entryType] of reportComponentMap) {
            if (isEnabled(entryType)) {
                this.reportTypeOptions.push({
                    value: entryType,
                    text: entryTypeMap.get(entryType)?.name ?? "",
                });
            }
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
        const index = this.selectedMedicationOptions.indexOf(medicationName);
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
        if (!this.selectedEntryType) {
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
        switch (this.selectedEntryType) {
            case EntryType.Medication:
                reportName = "Medication";
                break;
            case EntryType.HealthVisit:
                reportName = "Health Visits";
                break;
            case EntryType.Covid19TestResult:
                reportName = "COVID‑19 Test";
                break;
            case EntryType.Immunization:
                reportName = "Immunization";
                break;
            case EntryType.SpecialAuthorityRequest:
                reportName = "Special Authority Requests";
                break;
            case EntryType.Note:
                reportName = "Notes";
                break;
            case EntryType.LabResult:
                reportName = "Laboratory Tests";
                break;
            case EntryType.HospitalVisit:
                reportName = "Hospital Visits";
                break;
            default:
                return;
        }

        const formatTypeName = ReportFormatType[this.reportFormatType];
        const eventName = `${reportName} (${formatTypeName})`;

        if (!this.isDependent) {
            EventTracker.downloadReport(eventName);
        } else {
            EventTracker.downloadReport(`Dependent_${eventName}`);
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
                        v-model="selectedEntryType"
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
                        <DatePickerComponent
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
                        <DatePickerComponent
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
            v-if="selectedEntryType"
            data-testid="reportSample"
            :class="{ preview: !isDependent }"
        >
            <component
                :is="selectedReportComponent"
                ref="report"
                :hdid="hdid"
                :filter="reportFilter"
                :is-dependent="isDependent"
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

        <MessageModalComponent
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
