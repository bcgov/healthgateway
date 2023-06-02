<script setup lang="ts">
import { saveAs } from "file-saver";
import { Component, computed, ref } from "vue";
import { useStore } from "vue-composition-wrapper";

import DatePickerComponent from "@/components/DatePickerComponent.vue";
import LoadingComponent from "@/components/LoadingComponent.vue";
import MessageModalComponent from "@/components/modal/MessageModalComponent.vue";
import MultiSelectComponent from "@/components/MultiSelectComponent.vue";
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
import { DateWrapper, StringISODate } from "@/models/dateWrapper";
import { Dependent } from "@/models/dependent";
import { ResultError } from "@/models/errors";
import MedicationStatementHistory from "@/models/medicationStatementHistory";
import MedicationSummary from "@/models/medicationSummary";
import Patient from "@/models/patient";
import Report from "@/models/report";
import ReportFilter, { ReportFilterBuilder } from "@/models/reportFilter";
import ReportHeader from "@/models/reportHeader";
import { ReportFormatType } from "@/models/reportRequest";
import RequestResult from "@/models/requestResult";
import SelectOption from "@/models/selectOption";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger } from "@/services/interfaces";
import ConfigUtil from "@/utility/configUtil";
import EventTracker from "@/utility/eventTracker";

interface Props {
    hdid: string;
    isDependent?: boolean;
}
const props = withDefaults(defineProps<Props>(), {
    isDependent: false,
});

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

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const store = useStore();

const isLoading = ref(false);
const isGeneratingReport = ref(false);
const reportFormatType = ref(ReportFormatType.PDF);
const selectedEntryType = ref<EntryType | "">("");
const reportTypeOptions = ref([{ value: "", text: "Select" }]);
const selectedStartDate = ref<StringISODate | null>(null);
const selectedEndDate = ref<StringISODate | null>(null);
const selectedMedicationOptions = ref<string[]>([]);
const hasRecords = ref(false);
const reportFilter = ref<ReportFilter>(ReportFilterBuilder.create().build());
const isReportFilterStartDateValidDate = ref(true);
const isReportFilterEndDateValidDate = ref(true);

const messageModal = ref<MessageModalComponent>();
const reportComponent = ref<{
    generateReport: (
        reportFormatType: ReportFormatType,
        headerData: ReportHeader
    ) => Promise<RequestResult<Report>>;
}>();

const laboratoryOrdersAreQueued = computed<boolean>(() =>
    store.getters["laboratory/laboratoryOrdersAreQueued"](props.hdid)
);
const medications = computed<MedicationStatementHistory[]>(() =>
    store.getters["medication/medications"](props.hdid)
);
const patient = computed<Patient>(() => store.getters["user/patient"]);
const dependents = computed<Dependent[]>(
    () => store.getters["dependent/dependents"]
);

const selectedReportComponent = computed(() => {
    if (!selectedEntryType.value) {
        return "";
    }

    return reportComponentMap.get(selectedEntryType.value) ?? "";
});
const showLaboratoryOrderQueuedMessage = computed(
    () =>
        selectedEntryType.value === EntryType.LabResult &&
        laboratoryOrdersAreQueued.value
);
const headerData = computed<ReportHeader>(() => {
    const dependent = dependents.value.find(
        (d) => d.dependentInformation.hdid === props.hdid
    );
    if (dependent) {
        return {
            phn: dependent.dependentInformation.PHN,
            dateOfBirth: formatDate(
                dependent.dependentInformation.dateOfBirth || ""
            ),
            name: dependent.dependentInformation
                ? dependent.dependentInformation.firstname +
                  " " +
                  dependent.dependentInformation.lastname
                : "",
            isRedacted: reportFilter.value.hasMedicationsFilter(),
            datePrinted: new DateWrapper(new DateWrapper().toISO()).format(),
            filterText: reportFilter.value.filterText,
        };
    } else {
        return {
            phn: patient.value.personalHealthNumber,
            dateOfBirth: formatDate(patient.value.birthdate || ""),
            name: patient.value
                ? patient.value.preferredName.givenName +
                  " " +
                  patient.value.preferredName.surname
                : "",
            isRedacted: reportFilter.value.hasMedicationsFilter(),
            datePrinted: new DateWrapper(new DateWrapper().toISO()).format(),
            filterText: reportFilter.value.filterText,
        };
    }
});
const isMedicationReport = computed(
    () => selectedEntryType.value === EntryType.Medication
);
const medicationOptions = computed(() =>
    medications.value
        .reduce<MedicationSummary[]>(
            (accumulator: MedicationSummary[], current) => {
                const med = current.medicationSummary;
                if (
                    accumulator.findIndex(
                        (x) => x.brandName === med.brandName
                    ) < 0
                ) {
                    accumulator.push(med);
                }
                return accumulator;
            },
            []
        )
        .sort((a, b) => a.brandName.localeCompare(b.brandName))
        .map<SelectOption>((x) => ({
            text: x.brandName,
            value: x.brandName,
        }))
);
const isDownloadDisabled = computed(
    () =>
        isLoading.value ||
        !selectedEntryType.value ||
        !patient.value.hdid ||
        !hasRecords.value
);

function addError(
    errorType: ErrorType,
    source: ErrorSourceType,
    traceId: string | undefined
): void {
    store.dispatch("errorBanner/addError", { errorType, source, traceId });
}

function setTooManyRequestsError(key: string): void {
    store.dispatch("errorBanner/setTooManyRequestsError", { key });
}

function formatDate(date: string): string {
    return DateWrapper.format(date);
}

function clearFilterDates(): void {
    selectedStartDate.value = null;
    selectedEndDate.value = null;
    updateFilter();
}

function clearFilterMedication(medicationName: string): void {
    const index = selectedMedicationOptions.value.indexOf(medicationName);
    if (index >= 0) {
        selectedMedicationOptions.value.splice(index, 1);
        updateFilter();
    }
}

function cancelFilter(): void {
    selectedStartDate.value = convertEmptyStringDateToNull(
        reportFilter.value.startDate
    );
    selectedEndDate.value = convertEmptyStringDateToNull(
        reportFilter.value.endDate
    );
    selectedMedicationOptions.value = reportFilter.value.medications;
}

function updateFilter(): void {
    reportFilter.value = ReportFilterBuilder.create()
        .withStartDate(convertEmptyStringDateToNull(selectedStartDate.value))
        .withEndDate(convertEmptyStringDateToNull(selectedEndDate.value))
        .withMedications(selectedMedicationOptions.value)
        .build();
}

function convertEmptyStringDateToNull(
    date: StringISODate | null
): string | null {
    return !date ? null : date;
}

function showConfirmationModal(type: ReportFormatType): void {
    reportFormatType.value = type;
    messageModal.value?.showModal();
}

function downloadReport(): void {
    if (!selectedEntryType.value || !reportComponent.value) {
        return;
    }

    isGeneratingReport.value = true;

    trackDownload();

    reportComponent.value
        .generateReport(reportFormatType.value, headerData.value)
        .then((result: RequestResult<Report>) => {
            const mimeType = getMimeType(reportFormatType.value);
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
            logger.error(err.resultMessage);
            if (err.statusCode === 429) {
                setTooManyRequestsError("page");
            } else {
                addError(
                    ErrorType.Download,
                    ErrorSourceType.ExportRecords,
                    err.traceId
                );
            }
        })
        .finally(() => {
            isGeneratingReport.value = false;
        });
}

function getMimeType(reportFormatType: ReportFormatType): string {
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

function trackDownload(): void {
    let reportName = "";
    switch (selectedEntryType.value) {
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

    const formatTypeName = ReportFormatType[reportFormatType.value];
    const eventName = `${reportName} (${formatTypeName})`;

    if (!props.isDependent) {
        EventTracker.downloadReport(eventName);
    } else {
        EventTracker.downloadReport(`Dependent_${eventName}`);
    }
}

const reportIsEnabled = props.isDependent
    ? ConfigUtil.isDependentDatasetEnabled
    : ConfigUtil.isDatasetEnabled;

for (const [entryType] of reportComponentMap) {
    if (reportIsEnabled(entryType)) {
        reportTypeOptions.value.push({
            value: entryType,
            text: entryTypeMap.get(entryType)?.name ?? "",
        });
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
                        :values="selectedMedicationOptions"
                        placeholder="Choose a medication"
                        :options="medicationOptions"
                        data-testid="medicationExclusionFilter"
                        @update:values="
                            (values) => (selectedMedicationOptions = values)
                        "
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
                ref="reportComponent"
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
