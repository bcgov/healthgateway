<script setup lang="ts">
import { saveAs } from "file-saver";
import { computed, ref } from "vue";

import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import HgDatePickerComponent from "@/components/common/HgDatePickerComponent.vue";
import LoadingComponent from "@/components/common/LoadingComponent.vue";
import MessageModalComponent from "@/components/common/MessageModalComponent.vue";
import Covid19ReportComponent from "@/components/private/reports/Covid19ReportComponent.vue";
import HospitalVisitReportComponent from "@/components/private/reports/HospitalVisitReportComponent.vue";
import ImmunizationHistoryReportComponent from "@/components/private/reports/ImmunizationHistoryReportComponent.vue";
import LaboratoryReportComponent from "@/components/private/reports/LaboratoryReportComponent.vue";
import MedicationHistoryReportComponent from "@/components/private/reports/MedicationHistoryReportComponent.vue";
import MedicationRequestReportComponent from "@/components/private/reports/MedicationRequestReportComponent.vue";
import MSPVisitsReportComponent from "@/components/private/reports/MSPVisitsReportComponent.vue";
import NotesReportComponent from "@/components/private/reports/NotesReportComponent.vue";
import { EntryType, entryTypeMap } from "@/constants/entryType";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { DateWrapper, StringISODate } from "@/models/dateWrapper";
import { ResultError } from "@/models/errors";
import MedicationSummary from "@/models/medicationSummary";
import Report from "@/models/report";
import ReportFilter, { ReportFilterBuilder } from "@/models/reportFilter";
import ReportHeader from "@/models/reportHeader";
import { ReportFormatType } from "@/models/reportRequest";
import RequestResult from "@/models/requestResult";
import SelectOption from "@/models/selectOption";
import { ILogger } from "@/services/interfaces";
import { useDependentStore } from "@/stores/dependent";
import { useErrorStore } from "@/stores/error";
import { useLabResultStore } from "@/stores/labResult";
import { useMedicationStore } from "@/stores/medication";
import { useUserStore } from "@/stores/user";
import ConfigUtil from "@/utility/configUtil";
import EventTracker from "@/utility/eventTracker";

interface Props {
    hdid: string;
    isDependent?: boolean;
}
const props = withDefaults(defineProps<Props>(), {
    isDependent: false,
});

const reportComponentMap = new Map<EntryType, unknown>([
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
const medicationStore = useMedicationStore();
const userStore = useUserStore();
const dependentStore = useDependentStore();
const labResultsStore = useLabResultStore();
const errorStore = useErrorStore();

const isLoading = ref(false);
const isGeneratingReport = ref(false);
const reportFormatType = ref(ReportFormatType.PDF);
const selectedEntryType = ref<EntryType>();
const reportTypeOptions = ref<{ value: EntryType; title: string }[]>([]);
const selectedStartDate = ref<StringISODate>("");
const selectedEndDate = ref<StringISODate>("");
const selectedMedicationOptions = ref<string[]>([]);
const hasRecords = ref(false);
const reportFilter = ref<ReportFilter>(ReportFilterBuilder.create().build());
const isReportFilterStartDateValidDate = ref(true);
const isReportFilterEndDateValidDate = ref(true);
const isAdvancedOpen = ref(false);

const messageModal = ref<InstanceType<typeof MessageModalComponent>>();
const reportComponent = ref<{
    generateReport: (
        reportFormatType: ReportFormatType,
        headerData: ReportHeader
    ) => Promise<RequestResult<Report>>;
}>();

const laboratoryOrdersAreQueued = computed(() =>
    labResultsStore.laboratoryOrdersAreQueued(props.hdid)
);
const medications = computed(() => medicationStore.medications(props.hdid));
const patient = computed(() => userStore.patient);
const dependents = computed(() => dependentStore.dependents);

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
            title: x.brandName,
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
const reportFilterDateString = computed(() => {
    const start = reportFilter.value.startDate
        ? `From ${formatDate(reportFilter.value.startDate)}`
        : "";
    const end = reportFilter.value.endDate
        ? `Up To ${formatDate(reportFilter.value.endDate)}`
        : "";
    return `${start} ${end}`;
});

function formatDate(date: string): string {
    return DateWrapper.format(date);
}

function clearFilterDates(): void {
    selectedStartDate.value = "";
    selectedEndDate.value = "";
    updateFilter();
}

function clearFilterMedication(medicationName: string): void {
    const index = selectedMedicationOptions.value.indexOf(medicationName);
    if (index >= 0) {
        selectedMedicationOptions.value.splice(index, 1);
        updateFilter();
    }
}

function cancelFilterChanges(): void {
    selectedStartDate.value = reportFilter.value.startDate || "";
    selectedEndDate.value = reportFilter.value.endDate || "";
    selectedMedicationOptions.value = reportFilter.value.medications;
    isAdvancedOpen.value = false;
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

function toggleAdvanced(): void {
    isAdvancedOpen.value = !isAdvancedOpen.value;
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
                errorStore.setTooManyRequestsError("page");
            } else {
                errorStore.addError(
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
    let reportEventName =
        entryTypeMap.get(selectedEntryType.value)?.reportEventName ?? "";

    const formatTypeName = ReportFormatType[reportFormatType.value];
    const eventName = `${reportEventName} (${formatTypeName})`;

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
            title: entryTypeMap.get(entryType)?.name ?? "",
        });
    }
}
</script>

<template>
    <div>
        <v-alert
            v-show="showLaboratoryOrderQueuedMessage"
            closable
            type="info"
            class="d-print-none"
            data-testid="laboratory-orders-queued-alert-message"
            text="We are getting your lab results. It may take up to 48 hours until
            you can see them."
        />
        <v-sheet color="grey-lighten-4" class="pa-4 my-4">
            <v-row align="center">
                <v-col cols="12" sm="auto" class="flex-grow-1">
                    <v-select
                        id="reportType"
                        v-model="selectedEntryType"
                        label="Record Type"
                        hint="Select a record type"
                        data-testid="reportType"
                        hide-details
                        :items="reportTypeOptions"
                        :disabled="isLoading"
                        density="compact"
                    />
                </v-col>
                <v-col cols="12" sm="auto">
                    <HgButtonComponent
                        id="advanceFilterBtn"
                        variant="link"
                        data-testid="advancedBtn"
                        text="Advanced"
                        @click="toggleAdvanced"
                    />
                    <v-menu
                        data-testid="exportRecordBtn"
                        :disabled="isDownloadDisabled"
                    >
                        <template #activator="{ props }">
                            <HgButtonComponent
                                id="exportRecordBtn"
                                text="Download"
                                variant="primary"
                                data-testid="exportRecordBtn"
                                v-bind="props"
                                :disabled="isDownloadDisabled"
                            />
                        </template>
                        <v-list>
                            <v-list-item
                                title="PDF"
                                @click="
                                    showConfirmationModal(ReportFormatType.PDF)
                                "
                            />
                            <v-list-item
                                title="CSV"
                                @click="
                                    showConfirmationModal(ReportFormatType.CSV)
                                "
                            />
                            <v-list-item
                                title="XLSX"
                                @click="
                                    showConfirmationModal(ReportFormatType.XLSX)
                                "
                            />
                        </v-list>
                    </v-menu>
                </v-col>
            </v-row>
            <v-row v-if="reportFilter.hasActiveFilter()">
                <v-col v-if="reportFilter.hasDateFilter()">
                    <div><strong>Date Range</strong></div>
                    <v-chip
                        color="secondary"
                        data-testid="clearFilter"
                        closable
                        @click:close="clearFilterDates"
                    >
                        <span data-testid="selectedDatesFilter">{{
                            reportFilterDateString
                        }}</span>
                    </v-chip>
                </v-col>
                <v-col
                    v-if="
                        reportFilter.hasMedicationsFilter() &&
                        isMedicationReport
                    "
                    data-testid="medicationFilter"
                >
                    <div><strong>Exclude</strong></div>
                    <v-chip
                        v-for="item in reportFilter.medications"
                        :key="item"
                        closable
                        :title="item"
                        :data-testid="item + '-clearFilter'"
                        @click:close="clearFilterMedication(item)"
                    >
                        <span :data-testid="item + '-excluded'">{{
                            item
                        }}</span>
                    </v-chip>
                </v-col>
            </v-row>
            <v-expand-transition>
                <div v-show="isAdvancedOpen" class="pt-4">
                    <v-row>
                        <v-col cols="12" md="6" xl="4">
                            <HgDatePickerComponent
                                id="start-date"
                                v-model="selectedStartDate"
                                label="From"
                                data-testid="startDateInput"
                                density="compact"
                                hide-details
                                :disabled="isLoading"
                                @is-date-valid="
                                    isReportFilterStartDateValidDate = $event
                                "
                            />
                        </v-col>
                        <v-col cols="12" md="6" xl="4">
                            <HgDatePickerComponent
                                id="end-date"
                                v-model="selectedEndDate"
                                label="To"
                                data-testid="endDateInput"
                                density="compact"
                                hide-details
                                :disabled="isLoading"
                                @is-date-valid="
                                    isReportFilterEndDateValidDate = $event
                                "
                            />
                        </v-col>
                        <v-col v-if="isMedicationReport" cols="12">
                            <h6 class="text-h6 font-weight-bold">
                                Exclude These Records
                            </h6>
                            <p>Medications:</p>
                            <v-select
                                v-model="selectedMedicationOptions"
                                hint="Choose a medication"
                                multiple
                                hide-selected
                                chips
                                eager
                                :items="medicationOptions"
                                :disabled="isLoading"
                                data-testid="medicationExclusionFilter"
                                density="compact"
                            />
                        </v-col>
                        <v-col cols="12" class="d-flex justify-end">
                            <HgButtonComponent
                                variant="secondary"
                                data-testid="clearBtn"
                                :disabled="isLoading"
                                text="Cancel"
                                class="mr-4"
                                @click="cancelFilterChanges"
                            />
                            <HgButtonComponent
                                variant="primary"
                                data-testid="applyFilterBtn"
                                :disabled="
                                    isLoading ||
                                    !isReportFilterStartDateValidDate ||
                                    !isReportFilterEndDateValidDate
                                "
                                text="Apply"
                                @click="updateFilter"
                            />
                        </v-col>
                    </v-row>
                </div>
            </v-expand-transition>
        </v-sheet>
        <LoadingComponent :is-loading="isGeneratingReport" />
        <div v-if="selectedEntryType" data-testid="reportSample">
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
        <div v-else class="pa-4">
            <v-img
                class="mx-auto py-4"
                src="@/assets/images/reports/reports.png"
                data-testid="infoImage"
                width="200"
                alt="..."
            />
            <h5 data-testid="infoText" class="text-center text-h5">
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
