<script setup lang="ts">
import { saveAs } from "file-saver";
import { computed, ref, watch } from "vue";

import HgAlertComponent from "@/components/common/HgAlertComponent.vue";
import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import HgIconButtonComponent from "@/components/common/HgIconButtonComponent.vue";
import LoadingComponent from "@/components/common/LoadingComponent.vue";
import MessageModalComponent from "@/components/common/MessageModalComponent.vue";
import Covid19TestResultDescriptionComponent from "@/components/private/timeline/entry/Covid19TestResultDescriptionComponent.vue";
import { EntryType } from "@/constants/entryType";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { ClinicalDocument } from "@/models/clinicalDocument";
import {
    DateWrapper,
    StringISODate,
    StringISODateTime,
} from "@/models/dateWrapper";
import type { Dependent } from "@/models/dependent";
import EncodedMedia from "@/models/encodedMedia";
import { ResultError } from "@/models/errors";
import { ImmunizationAgent } from "@/models/immunizationModel";
import {
    Covid19LaboratoryOrder,
    Covid19LaboratoryTest,
    LaboratoryOrder,
} from "@/models/laboratory";
import Report from "@/models/report";
import ReportHeader from "@/models/reportHeader";
import { ReportFormatType, TemplateType } from "@/models/reportRequest";
import RequestResult from "@/models/requestResult";
import { LoadStatus } from "@/models/storeOperations";
import User from "@/models/user";
import {
    Action,
    Actor,
    Dataset,
    Destination,
    ExternalUrl,
    Format,
    Origin,
    Text,
    Type,
} from "@/plugins/extensions";
import {
    IClinicalDocumentService,
    ILaboratoryService,
    ILogger,
    IReportService,
    ITrackingService,
} from "@/services/interfaces";
import { useClinicalDocumentStore } from "@/stores/clinicalDocument";
import { useConfigStore } from "@/stores/config";
import { useCovid19TestResultStore } from "@/stores/covid19TestResult";
import { useDependentStore } from "@/stores/dependent";
import { useErrorStore } from "@/stores/error";
import { useImmunizationStore } from "@/stores/immunization";
import { useLabResultStore } from "@/stores/labResult";
import { useUserStore } from "@/stores/user";
import { useVaccinationStatusAuthenticatedStore } from "@/stores/vaccinationStatusAuthenticated";
import ConfigUtil from "@/utility/configUtil";
import DateSortUtility from "@/utility/dateSortUtility";
import EventDataUtility from "@/utility/eventDataUtility";

interface Props {
    dependent: Dependent;
}
const props = defineProps<Props>();

const emit = defineEmits<{
    (e: "needs-update"): void;
}>();

interface Covid19LaboratoryTestRow {
    id: string;
    reportAvailable: boolean;
    test: Covid19LaboratoryTest;
}

interface ImmunizationRow {
    date: string;
    immunization: string;
    agent: string;
    product: string;
    provider_clinic: string;
    lotNumber: string;
}

interface RecommendationRow {
    immunization: string;
    due_date: string;
}

const tabIndices = {
    profile: 0,
    covid19: 1,
    immunization: 2,
    labResults: 3,
    clinicalDocs: 4,
};

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const clinicalDocumentService = container.get<IClinicalDocumentService>(
    SERVICE_IDENTIFIER.ClinicalDocumentService
);
const laboratoryService = container.get<ILaboratoryService>(
    SERVICE_IDENTIFIER.LaboratoryService
);
const trackingService = container.get<ITrackingService>(
    SERVICE_IDENTIFIER.TrackingService
);
const userStore = useUserStore();
const configStore = useConfigStore();
const vaccinationStatusStore = useVaccinationStatusAuthenticatedStore();
const errorStore = useErrorStore();
const immunizationStore = useImmunizationStore();
const covid19TestResultStore = useCovid19TestResultStore();
const labResultStore = useLabResultStore();
const clinicalDocumentStore = useClinicalDocumentStore();
const dependentStore = useDependentStore();

const reportFormatType = ref(ReportFormatType.PDF);
const csvFormatType = ref(ReportFormatType.CSV);
const pdfFormatType = ref(ReportFormatType.PDF);
const xlsxFormatType = ref(ReportFormatType.XLSX);
const isReport = ref(false);
const isReportDownloading = ref(false);
const selectedTabIndex = ref(0);
const immunizationTabIndex = ref(0);
const selectedTestRow = ref<Covid19LaboratoryTestRow>();
const selectedLaboratoryOrderRow = ref<LaboratoryOrder>();
const selectedClinicalDocumentRow = ref<ClinicalDocument>();

const reportDownloadModal = ref<InstanceType<typeof MessageModalComponent>>();
const vaccineRecordResultModal =
    ref<InstanceType<typeof MessageModalComponent>>();
const deleteModal = ref<InstanceType<typeof MessageModalComponent>>();

const dependentHdid = computed(() => props.dependent.ownerId);
const user = computed<User>(() => userStore.user);
const webClientConfig = computed(() => configStore.webConfig);
const vaccineRecordState = computed(() =>
    vaccinationStatusStore.vaccineRecordState(dependentHdid.value)
);
const headerData = computed<ReportHeader>(() => {
    return {
        phn: props.dependent.dependentInformation.PHN,
        dateOfBirth: formatDate(
            props.dependent.dependentInformation.dateOfBirth
        ),
        name: props.dependent.dependentInformation
            ? props.dependent.dependentInformation.firstname +
              " " +
              props.dependent.dependentInformation.lastname
            : "",
        isRedacted: false,
        datePrinted: DateWrapper.now().format(),
        filterText: "",
    };
});
const isVaccineRecordDownloading = computed(
    () => vaccineRecordState.value.status === LoadStatus.REQUESTED
);
const isExpired = computed(
    () =>
        DateWrapper.today().diff(
            DateWrapper.fromIsoDate(
                props.dependent.dependentInformation.dateOfBirth
            ),
            "year"
        ).years > webClientConfig.value.maxDependentAge
);
// Lab Results
const isLaboratoryOrderTabShown = computed(() =>
    ConfigUtil.isDependentDatasetEnabled(EntryType.LabResult)
);
const labResultsAreLoading = computed(() =>
    labResultStore.labResultsAreLoading(dependentHdid.value)
);
const labResults = computed(() =>
    labResultStore
        .labResults(dependentHdid.value)
        .sort((a, b) =>
            DateSortUtility.descending(
                DateWrapper.fromIso(a.timelineDateTime),
                DateWrapper.fromIso(b.timelineDateTime)
            )
        )
);
// Clinical Documents
const isClinicalDocumentTabShown = computed(() =>
    ConfigUtil.isDependentDatasetEnabled(EntryType.ClinicalDocument)
);
const clinicalDocumentsAreLoading = computed(() =>
    clinicalDocumentStore.clinicalDocumentsAreLoading(dependentHdid.value)
);
const clinicalDocuments = computed(() =>
    clinicalDocumentStore
        .clinicalDocuments(dependentHdid.value)
        .sort((a, b) =>
            DateSortUtility.descending(
                DateWrapper.fromIsoDate(a.serviceDate),
                DateWrapper.fromIsoDate(b.serviceDate)
            )
        )
);
// Covid-19
const isCovid19TabShown = computed(() =>
    ConfigUtil.isDependentDatasetEnabled(EntryType.Covid19TestResult)
);
const covid19TestsAreLoading = computed(() =>
    covid19TestResultStore.covid19TestResultsAreLoading(dependentHdid.value)
);
const covid19TestResultRows = computed(
    () =>
        covid19TestResultStore
            .covid19TestResults(dependentHdid.value)
            .flatMap((o: Covid19LaboratoryOrder) =>
                o.labResults.map((r) => ({
                    id: o.id,
                    reportAvailable: o.reportAvailable,
                    test: r,
                }))
            ) ?? []
);
// Immunizations
const isImmunizationTabShown = computed(() =>
    ConfigUtil.isDependentDatasetEnabled(EntryType.Immunization)
);
const immunizationsAreLoading = computed(
    () =>
        immunizationStore.immunizationsAreLoading(dependentHdid.value) ||
        immunizationStore.immunizationsAreDeferred(dependentHdid.value)
);
const immunizationItems = computed(() =>
    immunizationStore
        .immunizations(dependentHdid.value)
        .sort((a, b) =>
            DateSortUtility.descending(
                DateWrapper.fromIsoDate(a.dateOfImmunization),
                DateWrapper.fromIsoDate(b.dateOfImmunization)
            )
        )
        .map<ImmunizationRow>((x) => ({
            date: DateWrapper.fromIsoDate(x.dateOfImmunization).format(),
            immunization: x.immunization.name,
            agent: getAgentNames(x.immunization.immunizationAgents),
            product: getProductNames(x.immunization.immunizationAgents),
            provider_clinic: x.providerOrClinic,
            lotNumber: getAgentLotNumbers(x.immunization.immunizationAgents),
        }))
);
const recommendationItems = computed(() =>
    immunizationStore
        .recommendations(dependentHdid.value)
        .map<RecommendationRow>((x) => ({
            immunization: x.recommendedVaccinations,
            due_date: DateWrapper.fromIsoDate(x.agentDueDate).format(),
        }))
);
const isDownloadImmunizationReportButtonDisabled = computed(() => {
    return (
        isReportDownloading.value ||
        selectedTabIndex.value !== tabIndices.immunization ||
        (immunizationItems.value.length == 0 &&
            recommendationItems.value.length == 0)
    );
});

function deleteDependent(): void {
    trackingService.trackEvent({
        action: Action.ButtonClick,
        text: Text.RemoveDependent,
        type: Type.Dependents,
    });
    dependentStore
        .removeDependent(user.value.hdid, props.dependent)
        .catch((err: ResultError) => {
            logger.error(err.message);
        })
        .finally(() => {
            emit("needs-update");
        });
}

function downloadCovid19Report(): void {
    if (selectedTestRow.value === undefined) {
        return;
    }

    isReportDownloading.value = true;
    const test = selectedTestRow.value.test;
    laboratoryService
        .getReportDocument(selectedTestRow.value.id, dependentHdid.value, true)
        .then((result) => {
            const report = result.resourcePayload;
            fetch(`data:${report.mediaType};${report.encoding},${report.data}`)
                .then((response) => response.blob())
                .then((blob) =>
                    saveAs(
                        blob,
                        `COVID_Result_${props.dependent.dependentInformation.firstname}${props.dependent.dependentInformation.lastname}_${test.collectedDateTime}.pdf`
                    )
                );
        })
        .catch((err: ResultError) => {
            logger.error(err.message);
            if (err.statusCode === 429) {
                errorStore.setTooManyRequestsError("page");
            } else {
                errorStore.addError(
                    ErrorType.Download,
                    ErrorSourceType.Covid19LaboratoryReport,
                    err.traceId
                );
            }
        })
        .finally(() => {
            isReportDownloading.value = false;
        });
}

function downloadLaboratoryOrderReport(): void {
    if (selectedLaboratoryOrderRow.value === undefined) {
        return;
    }

    isReportDownloading.value = true;
    trackingService.trackEvent({
        action: Action.Download,
        text: Text.Document,
        dataset: Dataset.LabResults,
        format: Format.Pdf,
        actor: Actor.Guardian,
    });

    const dateString = DateWrapper.fromIso(
        selectedLaboratoryOrderRow.value.timelineDateTime
    ).format("yyyy_MM_dd-HH_mm");

    laboratoryService
        .getReportDocument(
            selectedLaboratoryOrderRow.value.reportId,
            dependentHdid.value,
            false
        )
        .then((result) => {
            const report = result.resourcePayload;
            fetch(`data:${report.mediaType};${report.encoding},${report.data}`)
                .then((response) => response.blob())
                .then((blob) =>
                    saveAs(
                        blob,
                        `Laboratory_Report_${props.dependent.dependentInformation.firstname}_${props.dependent.dependentInformation.lastname}_${dateString}.pdf`
                    )
                );
        })
        .catch((err: ResultError) => {
            logger.error(err.message);
            if (err.statusCode === 429) {
                errorStore.setTooManyRequestsError("page");
            } else {
                errorStore.addError(
                    ErrorType.Download,
                    ErrorSourceType.LaboratoryReport,
                    err.traceId
                );
            }
        })
        .finally(() => {
            isReportDownloading.value = false;
        });
}

function downloadImmunizationReport(): void {
    isReportDownloading.value = true;

    trackingService.trackEvent({
        action: Action.Download,
        text:
            immunizationTabIndex.value === 0
                ? Text.DownloadDependentHistoricImmunizations
                : Text.DownloadDependentRecommendedImmunizations,
        dataset: Dataset.Immunizations,
        type: Type.Dependents,
        format: EventDataUtility.getFormat(reportFormatType.value),
    });

    generateReport(
        TemplateType.DependentImmunization,
        reportFormatType.value,
        headerData.value
    )
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
            logger.error(err.message);
            if (err.statusCode === 429) {
                errorStore.setTooManyRequestsError("page");
            } else {
                errorStore.addError(
                    ErrorType.Download,
                    ErrorSourceType.DependentImmunizationReport,
                    err.traceId
                );
            }
        })
        .finally(() => {
            isReportDownloading.value = false;
        });
}

function downloadClinicalDocument(): void {
    if (selectedClinicalDocumentRow.value === undefined) {
        return;
    }

    logger.debug("downloadClinicalDocument()");

    isReportDownloading.value = true;
    trackingService.trackEvent({
        action: Action.Download,
        text: Text.Document,
        dataset: Dataset.ClinicalDocuments,
        format: Format.Pdf,
        actor: Actor.Guardian,
    });

    clinicalDocumentService
        .getFile(selectedClinicalDocumentRow.value.fileId, dependentHdid.value)
        .then((result: RequestResult<EncodedMedia>) => {
            fetch(
                `data:${result.resourcePayload.mediaType};${result.resourcePayload.encoding},${result.resourcePayload.data}`
            )
                .then((response) => response.blob())
                .then((blob) =>
                    saveAs(
                        blob,
                        `Clinical_Document_${props.dependent.dependentInformation.firstname}_${props.dependent.dependentInformation.lastname}.pdf`
                    )
                );
        })
        .catch((err: ResultError) => {
            logger.error(err.message);
            if (err.statusCode === 429) {
                errorStore.setTooManyRequestsError("page");
            } else {
                errorStore.addError(
                    ErrorType.Download,
                    ErrorSourceType.ClinicalDocument,
                    err.traceId
                );
            }
        })
        .finally(() => {
            isReportDownloading.value = false;
        });
}

function downloadDocument(): void {
    if (isReport.value) {
        logger.debug(
            `Download document from dependent tab: ${selectedTabIndex.value}`
        );

        if (selectedTabIndex.value === tabIndices.covid19) {
            downloadCovid19Report();
        } else if (selectedTabIndex.value === tabIndices.immunization) {
            downloadImmunizationReport();
        } else if (selectedTabIndex.value === tabIndices.labResults) {
            downloadLaboratoryOrderReport();
        } else if (selectedTabIndex.value === tabIndices.clinicalDocs) {
            downloadClinicalDocument();
        }
    } else {
        downloadVaccinePdf();
    }
}

function downloadVaccinePdf(): void {
    logger.debug(`Downloading vaccine PDF for hdid: ${dependentHdid.value}`);
    vaccinationStatusStore.retrieveVaccineRecord(dependentHdid.value);
}

function formatDateTime(dateTime: StringISODateTime): string {
    return DateWrapper.fromIso(dateTime).format();
}

function formatDate(date: StringISODate): string {
    return DateWrapper.fromIsoDate(date).format();
}

function fetchClinicalDocuments(): void {
    logger.debug(
        `Fetching Clinical Documents for Hdid: ${dependentHdid.value}`
    );
    clinicalDocumentStore.retrieveClinicalDocuments(dependentHdid.value);
}

function fetchCovid19LaboratoryTests(): void {
    logger.debug(
        `Fetching COVID 19 Laboratory Tests for Hdid: ${dependentHdid.value}`
    );
    covid19TestResultStore.retrieveCovid19TestResults(dependentHdid.value);
}

function fetchLaboratoryOrders(): void {
    logger.debug(`Fetching Lab Results for Hdid: ${dependentHdid.value}`);
    labResultStore.retrieveLabResults(dependentHdid.value);
}

function fetchPatientImmunizations(): void {
    const hdid = dependentHdid.value;
    logger.debug(`Fetching Patient Immunizations for Hdid: ${hdid}`);
    immunizationStore.retrieveImmunizations(hdid);
}

function generateReport(
    templateType: TemplateType,
    reportFormatType: ReportFormatType,
    headerData: ReportHeader
): Promise<RequestResult<Report>> {
    const reportService = container.get<IReportService>(
        SERVICE_IDENTIFIER.ReportService
    );
    return reportService.generateReport({
        data: {
            header: headerData,
            records: immunizationItems.value,
            recommendations: recommendationItems.value,
        },
        template: templateType,
        type: reportFormatType,
    });
}

function getAgentLotNumbers(immunizationAgents: ImmunizationAgent[]): string {
    return immunizationAgents.map<string>((x) => x.lotNumber).join(", ");
}

function getAgentNames(immunizationAgents: ImmunizationAgent[]): string {
    return immunizationAgents.map<string>((x) => x.name).join(", ");
}

function getProductNames(immunizationAgents: ImmunizationAgent[]): string {
    return immunizationAgents.map<string>((x) => x.productName).join(", ");
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

function getOutcomeClasses(outcome: string): string[] {
    switch (outcome?.toUpperCase()) {
        case "NEGATIVE":
            return ["text-success"];
        case "POSITIVE":
            return ["text-danger"];
        default:
            return ["text-muted"];
    }
}

function showVaccineProofDownloadConfirmationModal(): void {
    isReport.value = false;
    reportDownloadModal.value?.showModal();
}

function showCovid19DownloadConfirmationModal(
    row: Covid19LaboratoryTestRow
): void {
    isReport.value = true;
    selectedTestRow.value = row;
    reportDownloadModal.value?.showModal();
}

function showImmunizationDownloadConfirmationModal(
    type: ReportFormatType
): void {
    isReport.value = true;
    reportFormatType.value = type;
    reportDownloadModal.value?.showModal();
}

function showClinicalDocumentDownloadConfirmationModal(
    row: ClinicalDocument
): void {
    isReport.value = true;
    selectedClinicalDocumentRow.value = row;
    reportDownloadModal.value?.showModal();
}

function showLaboratoryOrderDownloadConfirmationModal(
    row: LaboratoryOrder
): void {
    isReport.value = true;
    selectedLaboratoryOrderRow.value = row;
    reportDownloadModal.value?.showModal();
}

function showDeleteConfirmationModal(): void {
    deleteModal.value?.showModal();
}

watch(vaccineRecordState, () => {
    if (vaccineRecordState.value.resultMessage.length > 0) {
        vaccineRecordResultModal.value?.showModal();
    }

    if (
        vaccineRecordState.value.record !== undefined &&
        vaccineRecordState.value.status === LoadStatus.LOADED &&
        vaccineRecordState.value.download
    ) {
        const mimeType = vaccineRecordState.value.record.document.mediaType;
        const downloadLink = `data:${mimeType};base64,${vaccineRecordState.value.record.document.data}`;
        fetch(downloadLink).then((res) => {
            trackingService.trackEvent({
                action: Action.Download,
                text: Text.Document,
                type: Type.Covid19ProofOfVaccination,
                format: Format.Pdf,
                actor: Actor.Guardian,
            });
            res.blob().then((blob) => saveAs(blob, "VaccineProof.pdf"));
        });
        vaccinationStatusStore.stopVaccineRecordDownload(dependentHdid.value);
    }
});
</script>

<template>
    <div>
        <LoadingComponent
            :is-loading="isReportDownloading"
            :full-screen="false"
        />
        <LoadingComponent
            :is-loading="isVaccineRecordDownloading"
            :text="vaccineRecordState.statusMessage"
            :full-screen="false"
        />
        <v-card
            :data-testid="`dependent-card-${dependent.dependentInformation.PHN}`"
        >
            <v-card-title class="bg-grey-lighten-3 pa-4 pb-0">
                <v-row>
                    <v-col>
                        <h5 class="text-h6 mb-4" data-testid="dependentName">
                            {{ dependent.dependentInformation.firstname }}
                            {{ dependent.dependentInformation.lastname }}
                        </h5>
                    </v-col>
                    <v-col cols="auto">
                        <v-menu>
                            <template
                                #activator="{ props: dependentMenuProps }"
                            >
                                <HgIconButtonComponent
                                    icon="ellipsis-v"
                                    size="small"
                                    class="text-medium-emphasis"
                                    data-testid="dependentMenuBtn"
                                    v-bind="dependentMenuProps"
                                />
                            </template>
                            <v-list>
                                <v-list-item
                                    data-testid="deleteDependentMenuBtn"
                                    class="menuItem"
                                    title="Delete"
                                    @click="showDeleteConfirmationModal()"
                                />
                            </v-list>
                        </v-menu>
                    </v-col>
                </v-row>
                <v-tabs
                    v-model="selectedTabIndex"
                    color="primary"
                    selected-class="bg-white"
                >
                    <v-tab :value="0">Profile</v-tab>
                    <v-tab
                        v-if="isCovid19TabShown"
                        :disabled="isExpired"
                        data-testid="covid19TabTitle"
                        :value="1"
                        @click="fetchCovid19LaboratoryTests"
                    >
                        COVID-19
                    </v-tab>
                    <v-tab
                        v-if="isImmunizationTabShown"
                        :disabled="isExpired"
                        :data-testid="`immunization-tab-title-${dependent.ownerId}`"
                        :value="2"
                        @click="fetchPatientImmunizations"
                    >
                        Immunization
                    </v-tab>
                    <v-tab
                        v-if="isLaboratoryOrderTabShown"
                        :disabled="isExpired"
                        :data-testid="`lab-results-tab-title-${dependent.ownerId}`"
                        :value="3"
                        @click="fetchLaboratoryOrders"
                    >
                        Lab Results
                    </v-tab>
                    <v-tab
                        v-if="isClinicalDocumentTabShown"
                        :disabled="isExpired"
                        :data-testid="`clinical-document-tab-title-${dependent.ownerId}`"
                        :value="4"
                        @click="fetchClinicalDocuments"
                    >
                        Clinical Docs
                    </v-tab>
                </v-tabs>
            </v-card-title>
            <v-card-text class="pa-4">
                <v-window v-model="selectedTabIndex">
                    <v-window-item data-testid="dependentTab" class="pa-1">
                        <div v-if="isExpired">
                            <v-row>
                                <v-col class="d-flex justify-content-center">
                                    <h5 class="text-h6">
                                        Your access has expired
                                    </h5>
                                </v-col>
                            </v-row>
                            <v-row>
                                <v-col class="d-flex justify-content-center">
                                    <p>
                                        You no longer have access to this
                                        dependent as they have turned
                                        {{ webClientConfig.maxDependentAge }}
                                    </p>
                                </v-col>
                            </v-row>
                            <v-row>
                                <v-col class="d-flex justify-content-center">
                                    <HgButtonComponent
                                        variant="secondary"
                                        text="Remove Dependent"
                                        @click="deleteDependent()"
                                    />
                                </v-col>
                            </v-row>
                        </div>
                        <div v-else>
                            <v-row class="text-body-1">
                                <v-col xl="3" md="4" sm="6">
                                    <label
                                        :for="`dependent-phn-${dependent.ownerId}`"
                                        >PHN</label
                                    >
                                    <v-text-field
                                        :id="`dependent-phn-${dependent.ownerId}`"
                                        density="compact"
                                        :value="
                                            dependent.dependentInformation.PHN
                                        "
                                        data-testid="dependentPHN"
                                        readonly
                                        class="mt-2"
                                        hide-details
                                    />
                                </v-col>
                                <v-col xl="3" md="4" sm="6">
                                    <label
                                        :for="`dependent-dob-${dependent.ownerId}`"
                                        >Date of Birth</label
                                    >
                                    <v-text-field
                                        :id="`dependent-dob-${dependent.ownerId}`"
                                        density="compact"
                                        :value="
                                            formatDate(
                                                dependent.dependentInformation
                                                    .dateOfBirth
                                            )
                                        "
                                        data-testid="dependentDOB"
                                        readonly
                                        class="mt-2"
                                        hide-details
                                    />
                                </v-col>
                            </v-row>
                        </div>
                    </v-window-item>
                    <v-window-item data-testid="covid19Tab" class="pa-1">
                        <div class="d-flex justify-center">
                            <HgButtonComponent
                                :id="`download-proof-of-vaccination-btn-id-${dependent.ownerId}`"
                                :data-testid="`download-proof-of-vaccination-btn-${dependent.ownerId}`"
                                variant="secondary"
                                prepend-icon="check-circle"
                                text="Download Proof of Vaccination"
                                @click="
                                    showVaccineProofDownloadConfirmationModal
                                "
                            />
                        </div>
                        <p
                            :id="`covid19-table-header-${dependent.ownerId}`"
                            class="text-body-1 font-weight-bold my-4"
                        >
                            COVID-19 Test Results
                        </p>
                        <v-skeleton-loader
                            v-if="covid19TestsAreLoading"
                            type="table-thead, table-row@2"
                            data-testid="table-skeleton-loader"
                        />
                        <template v-else>
                            <p
                                v-if="covid19TestResultRows.length === 0"
                                data-testid="covid19NoRecords"
                                class="text-body-2"
                            >
                                No records found.
                            </p>
                            <v-table
                                v-else
                                class="w-100 mb-0"
                                :aria-labelledby="`covid19-table-header-${dependent.ownerId}`"
                                :data-testid="`covid19-table-${dependent.ownerId}`"
                            >
                                <thead>
                                    <tr>
                                        <th class="text-center" scope="col">
                                            Date
                                        </th>
                                        <th
                                            class="d-none d-md-table-cell text-center"
                                            scope="col"
                                        >
                                            Type
                                        </th>
                                        <th
                                            class="d-none d-md-table-cell text-center"
                                            scope="col"
                                        >
                                            Status
                                        </th>
                                        <th class="text-center" scope="col">
                                            Result
                                        </th>
                                        <th class="text-center" scope="col">
                                            Report
                                        </th>
                                    </tr>
                                </thead>
                                <tbody>
                                    <tr
                                        v-for="(
                                            row, index
                                        ) in covid19TestResultRows"
                                        :key="index"
                                    >
                                        <td
                                            data-testid="dependentCovidTestDate"
                                            class="text-center text-nowrap"
                                        >
                                            {{
                                                formatDateTime(
                                                    row.test.collectedDateTime
                                                )
                                            }}
                                        </td>
                                        <td
                                            data-testid="dependentCovidTestType"
                                            class="d-none d-md-table-cell text-center"
                                        >
                                            {{ row.test.testType }}
                                        </td>
                                        <td
                                            data-testid="dependentCovidTestStatus"
                                            class="d-none d-md-table-cell text-center"
                                        >
                                            {{ row.test.testStatus }}
                                        </td>
                                        <td
                                            data-testid="dependentCovidTestLabResult"
                                            class="text-center"
                                        >
                                            <span
                                                v-if="
                                                    row.test
                                                        .filteredLabResultOutcome
                                                "
                                                class="font-weight-bold"
                                                :class="
                                                    getOutcomeClasses(
                                                        row.test
                                                            .labResultOutcome
                                                    )
                                                "
                                            >
                                                {{
                                                    row.test
                                                        .filteredLabResultOutcome
                                                }}
                                            </span>
                                            <HgIconButtonComponent
                                                v-if="
                                                    row.test.resultReady &&
                                                    row.test.resultDescription
                                                "
                                                :id="
                                                    'dependent-covid-test-info-button-' +
                                                    index
                                                "
                                                data-testid="dependent-covid-test-info-button"
                                                aria-label="Result Description"
                                                class="ml-1 text-primary"
                                                size="x-small"
                                            >
                                                <v-icon
                                                    icon="info-circle"
                                                    size="large"
                                                />
                                                <v-overlay
                                                    activator="parent"
                                                    location-strategy="connected"
                                                    scroll-strategy="close"
                                                    open-on-click
                                                    :open-on-hover="false"
                                                >
                                                    <v-card
                                                        data-testid="dependent-covid-test-info-popover"
                                                        class="pa-2"
                                                        max-width="400px"
                                                    >
                                                        <Covid19TestResultDescriptionComponent
                                                            :description="
                                                                row.test
                                                                    .resultDescription
                                                            "
                                                            :link="
                                                                row.test
                                                                    .resultLink
                                                            "
                                                        />
                                                    </v-card>
                                                </v-overlay>
                                            </HgIconButtonComponent>
                                        </td>
                                        <td class="text-center">
                                            <HgIconButtonComponent
                                                v-if="
                                                    row.reportAvailable &&
                                                    row.test.resultReady
                                                "
                                                icon="download"
                                                data-testid="dependentCovidReportDownloadBtn"
                                                variant="secondary"
                                                class="pa-0"
                                                color="primary"
                                                size="small"
                                                @click="
                                                    showCovid19DownloadConfirmationModal(
                                                        row
                                                    )
                                                "
                                            />
                                        </td>
                                    </tr>
                                </tbody>
                            </v-table>
                        </template>
                    </v-window-item>
                    <v-window-item
                        :data-testid="`immunization-tab-${dependent.ownerId}`"
                        class="pa-1"
                    >
                        <HgAlertComponent
                            v-if="immunizationItems.length != 0"
                            class="mb-4"
                            type="info"
                            variant="outlined"
                            data-testid="dependent-immunization-disclaimer-alert"
                        >
                            <template #text>
                                <span class="text-body-1">
                                    If your dependent’s immunizations are
                                    missing or incorrect,
                                    <a
                                        href="https://www.immunizationrecord.gov.bc.ca/"
                                        target="_blank"
                                        rel="noopener"
                                        class="text-link"
                                        @click="
                                            trackingService.trackEvent({
                                                action: Action.ExternalLink,
                                                text: Text.ImmunizationUpdateForm,
                                                origin: Origin.Dependents,
                                                destination:
                                                    Destination.ImmunizationRecordBC,
                                                type: Type.Dependents,
                                                url: ExternalUrl.ImmunizationRecordBC,
                                            })
                                        "
                                        >fill in this online form</a
                                    >.
                                </span>
                            </template>
                        </HgAlertComponent>

                        <div
                            :data-testid="`immunization-tab-div-${dependent.ownerId}`"
                        >
                            <v-tabs
                                v-model="immunizationTabIndex"
                                color="primary"
                            >
                                <v-tab :key="1">History</v-tab>
                                <v-tab :key="2">Schedule</v-tab>
                            </v-tabs>
                            <v-skeleton-loader
                                v-if="immunizationsAreLoading"
                                type="table-thead, table-row@2"
                                data-testid="table-skeleton-loader"
                            />
                            <v-window
                                v-else
                                v-model="immunizationTabIndex"
                                class="pa-4"
                            >
                                <v-window-item class="pa-1">
                                    <p
                                        v-if="immunizationItems.length === 0"
                                        class="text-body-1"
                                        :data-testid="`immunization-history-no-rows-found-${dependent.ownerId}`"
                                    >
                                        No records found. If this is your first
                                        time checking for records, please try
                                        refreshing the page in a few minutes.
                                    </p>
                                    <div v-else>
                                        <v-row justify="end">
                                            <v-col cols="auto">
                                                <v-menu
                                                    v-if="
                                                        immunizationItems.length !=
                                                        0
                                                    "
                                                    id="download-immunization-history-report-btn"
                                                    variant="outline-dark"
                                                    :data-testid="`download-immunization-history-report-btn-${dependent.ownerId}`"
                                                >
                                                    <template
                                                        #activator="{
                                                            props: immunizationReportDownloadProps,
                                                        }"
                                                    >
                                                        <HgButtonComponent
                                                            :id="`download-immunization-history-report-btn-${dependent.ownerId}`"
                                                            :data-testid="`download-immunization-history-report-btn-${dependent.ownerId}`"
                                                            variant="secondary"
                                                            append-icon="caret-down"
                                                            text="Download"
                                                            v-bind="
                                                                immunizationReportDownloadProps
                                                            "
                                                            :disabled="
                                                                isDownloadImmunizationReportButtonDisabled
                                                            "
                                                        />
                                                    </template>
                                                    <v-list>
                                                        <v-list-item
                                                            :data-testid="`download-immunization-history-report-pdf-btn-${dependent.ownerId}`"
                                                            title="PDF"
                                                            @click="
                                                                showImmunizationDownloadConfirmationModal(
                                                                    pdfFormatType
                                                                )
                                                            "
                                                        />
                                                        <v-list-item
                                                            :data-testid="`download-immunization-history-report-csv-btn-${dependent.ownerId}`"
                                                            title="CSV"
                                                            @click="
                                                                showImmunizationDownloadConfirmationModal(
                                                                    csvFormatType
                                                                )
                                                            "
                                                        />
                                                        <v-list-item
                                                            :data-testid="`download-immunization-history-report-xlsx-btn-${dependent.ownerId}`"
                                                            title="XLSX"
                                                            @click="
                                                                showImmunizationDownloadConfirmationModal(
                                                                    xlsxFormatType
                                                                )
                                                            "
                                                        />
                                                    </v-list>
                                                </v-menu>
                                            </v-col>
                                        </v-row>
                                        <v-table
                                            class="w-100 mb-0"
                                            aria-label="Immunization History"
                                            :data-testid="`immunization-history-table-${dependent.ownerId}`"
                                        >
                                            <thead>
                                                <tr>
                                                    <th
                                                        class="text-center"
                                                        scope="col"
                                                    >
                                                        Date
                                                    </th>
                                                    <th
                                                        class="text-center"
                                                        scope="col"
                                                    >
                                                        Immunization
                                                    </th>
                                                    <th
                                                        class="d-none d-lg-table-cell text-center"
                                                        scope="col"
                                                    >
                                                        Agent
                                                    </th>
                                                    <th
                                                        class="d-none d-lg-table-cell text-center"
                                                        scope="col"
                                                    >
                                                        Product
                                                    </th>
                                                    <th
                                                        class="text-center"
                                                        scope="col"
                                                    >
                                                        Provider/Clinic
                                                    </th>
                                                    <th
                                                        class="d-none d-lg-table-cell text-center"
                                                        scope="col"
                                                    >
                                                        Lot Number
                                                    </th>
                                                </tr>
                                            </thead>
                                            <tbody>
                                                <tr
                                                    v-for="(
                                                        row, index
                                                    ) in immunizationItems"
                                                    :key="index"
                                                >
                                                    <td
                                                        :data-testid="`history-immunization-date-${dependent.ownerId}-${index}`"
                                                        class="text-center text-nowrap"
                                                    >
                                                        {{ row.date }}
                                                    </td>
                                                    <td
                                                        :data-testid="`history-product-${dependent.ownerId}-${index}`"
                                                        class="text-center"
                                                    >
                                                        {{ row.immunization }}
                                                    </td>
                                                    <td
                                                        :data-testid="`history-immunizing-agent-${dependent.ownerId}-${index}`"
                                                        class="d-none d-lg-table-cell text-center"
                                                    >
                                                        {{ row.agent }}
                                                    </td>
                                                    <td
                                                        :data-testid="`history-immunizing-product-${dependent.ownerId}-${index}`"
                                                        class="d-none d-lg-table-cell text-center"
                                                    >
                                                        {{ row.product }}
                                                    </td>
                                                    <td
                                                        :data-testid="`history-provider-clinic-${dependent.ownerId}-${index}`"
                                                        class="text-center"
                                                    >
                                                        {{
                                                            row.provider_clinic
                                                        }}
                                                    </td>
                                                    <td
                                                        :data-testid="`history-lot-number-${dependent.ownerId}-${index}`"
                                                        class="d-none d-lg-table-cell text-center"
                                                    >
                                                        {{ row.lotNumber }}
                                                    </td>
                                                </tr>
                                            </tbody>
                                        </v-table>
                                    </div>
                                </v-window-item>
                                <v-window-item class="pa-1">
                                    <v-row justify="end" no-gutters>
                                        <v-col cols="12" :md="true">
                                            <p class="mb-md-0 text-body-1">
                                                School-aged children are offered
                                                most immunizations in their
                                                school, particularly in grades 6
                                                and 9. The school can let you
                                                know which vaccines are offered.
                                                You need to book an appointment
                                                to get your child vaccinated
                                                against COVID‑19.
                                                <a
                                                    href="https://www2.gov.bc.ca/gov/content/covid-19/vaccine"
                                                    rel="noopener"
                                                    target="_blank"
                                                    class="text-link"
                                                    >Find out how.</a
                                                >
                                            </p>
                                        </v-col>
                                        <v-col
                                            v-if="
                                                recommendationItems.length > 0
                                            "
                                            cols="auto"
                                            class="pl-4 d-flex align-start"
                                        >
                                            <v-menu
                                                id="download-immunization-schedule-report-menu"
                                                :data-testid="`download-immunization-schedule-report-menu-${dependent.ownerId}`"
                                            >
                                                <template
                                                    #activator="{
                                                        props: immunizationForecastMenuProps,
                                                    }"
                                                >
                                                    <HgButtonComponent
                                                        text="Download"
                                                        variant="secondary"
                                                        append-icon="caret-down"
                                                        v-bind="
                                                            immunizationForecastMenuProps
                                                        "
                                                        :data-testid="`download-immunization-schedule-report-btn-${dependent.ownerId}`"
                                                        :disabled="
                                                            isDownloadImmunizationReportButtonDisabled
                                                        "
                                                    />
                                                </template>
                                                <v-list>
                                                    <v-list-item
                                                        :data-testid="`download-immunization-schedule-report-pdf-btn-${dependent.ownerId}`"
                                                        title="PDF"
                                                        @click="
                                                            showImmunizationDownloadConfirmationModal(
                                                                pdfFormatType
                                                            )
                                                        "
                                                    />
                                                    <v-list-item
                                                        :data-testid="`download-immunization-schedule-report-csv-btn-${dependent.ownerId}`"
                                                        title="CSV"
                                                        @click="
                                                            showImmunizationDownloadConfirmationModal(
                                                                csvFormatType
                                                            )
                                                        "
                                                    />
                                                    <v-list-item
                                                        :data-testid="`download-immunization-schedule-report-xlsx-btn-${dependent.ownerId}`"
                                                        title="XLSX"
                                                        @click="
                                                            showImmunizationDownloadConfirmationModal(
                                                                xlsxFormatType
                                                            )
                                                        "
                                                    />
                                                </v-list>
                                            </v-menu>
                                        </v-col>
                                    </v-row>
                                    <p
                                        v-if="recommendationItems.length === 0"
                                        :data-testid="`immunization-schedule-no-rows-found-${dependent.ownerId}`"
                                        class="text-body-1 my-4"
                                    >
                                        No records found.
                                    </p>
                                    <v-table
                                        v-else
                                        class="w-100 mb-0"
                                        aria-label="Immunization Schedule"
                                        :data-testid="`immunization-schedule-table-${dependent.ownerId}`"
                                    >
                                        <thead>
                                            <tr>
                                                <th
                                                    class="text-center"
                                                    scope="col"
                                                >
                                                    Immunization
                                                </th>
                                                <th
                                                    class="text-center"
                                                    scope="col"
                                                >
                                                    Due Date
                                                </th>
                                            </tr>
                                        </thead>
                                        <tbody>
                                            <tr
                                                v-for="(
                                                    row, index
                                                ) in recommendationItems"
                                                :key="index"
                                            >
                                                <td
                                                    :data-testid="`schedule-immunization-${dependent.ownerId}-${index}`"
                                                    class="text-center"
                                                >
                                                    {{ row.immunization }}
                                                </td>
                                                <td
                                                    :data-testid="`schedule-due-date-${dependent.ownerId}-${index}`"
                                                    class="text-center text-nowrap"
                                                >
                                                    {{ row.due_date }}
                                                </td>
                                            </tr>
                                        </tbody>
                                    </v-table>
                                </v-window-item>
                            </v-window>
                        </div>
                    </v-window-item>
                    <v-window-item
                        :data-testid="`lab-results-tab-${dependent.ownerId}`"
                        class="pa-1"
                    >
                        <v-skeleton-loader
                            v-if="labResultsAreLoading"
                            type="table-thead, table-row@2"
                            data-testid="table-skeleton-loader"
                        />
                        <div
                            v-else-if="labResults.length === 0"
                            :data-testid="`lab-results-no-records-${dependent.ownerId}`"
                            class="text-body-1"
                        >
                            No records found. If you just added your dependent,
                            it can take up to 24 hours to get their records.
                        </div>
                        <v-table
                            v-else
                            class="w-100 mb-0"
                            :aria-labelledby="`lab-results-tab-title-${dependent.ownerId}`"
                            :data-testid="`lab-results-table-${dependent.ownerId}`"
                        >
                            <thead>
                                <tr>
                                    <th class="text-center" scope="col">
                                        Date
                                    </th>
                                    <th class="text-center" scope="col">
                                        Title
                                    </th>
                                    <th
                                        class="d-none d-lg-table-cell text-center"
                                        scope="col"
                                    >
                                        Lab
                                    </th>
                                    <th
                                        class="d-none d-md-table-cell text-center"
                                        scope="col"
                                    >
                                        Status
                                    </th>
                                    <th class="text-center" scope="col">
                                        Detailed Report
                                    </th>
                                </tr>
                            </thead>
                            <tbody>
                                <tr
                                    v-for="(row, index) in labResults"
                                    :key="index"
                                >
                                    <td
                                        :data-testid="`lab-results-date-${dependent.ownerId}-${index}`"
                                        class="text-center text-nowrap"
                                    >
                                        {{
                                            formatDateTime(row.timelineDateTime)
                                        }}
                                    </td>
                                    <td
                                        :data-testid="`lab-results-title-${dependent.ownerId}-${index}`"
                                        class="text-center"
                                    >
                                        {{ row.commonName }}
                                    </td>
                                    <td
                                        :data-testid="`lab-results-lab-${dependent.ownerId}-${index}`"
                                        class="d-none d-lg-table-cell text-center"
                                    >
                                        {{ row.reportingSource }}
                                    </td>
                                    <td
                                        :data-testid="`lab-results-status-${dependent.ownerId}-${index}`"
                                        class="d-none d-md-table-cell text-center"
                                    >
                                        {{ row.orderStatus }}
                                    </td>
                                    <td class="text-center">
                                        <HgIconButtonComponent
                                            v-if="row.reportAvailable"
                                            :data-testid="`lab-results-report-download-button-${dependent.ownerId}-${index}`"
                                            variant="link"
                                            icon="download"
                                            color="primary"
                                            size="small"
                                            @click="
                                                showLaboratoryOrderDownloadConfirmationModal(
                                                    row
                                                )
                                            "
                                        />
                                    </td>
                                </tr>
                            </tbody>
                        </v-table>
                    </v-window-item>
                    <v-window-item
                        :data-testid="`clinical-document-tab-${dependent.ownerId}`"
                        class="pa-1"
                    >
                        <v-skeleton-loader
                            v-if="clinicalDocumentsAreLoading"
                            type="table-thead, table-row@2"
                            data-testid="table-skeleton-loader"
                        />
                        <div
                            v-else-if="clinicalDocuments.length === 0"
                            :data-testid="`clinical-document-no-records-${dependent.ownerId}`"
                            class="text-body-1"
                        >
                            No records found.
                        </div>
                        <v-table
                            v-else
                            class="w-100 mb-0"
                            :aria-labelledby="`clinical-document-tab-title-${dependent.ownerId}`"
                            :data-testid="`clinical-document-table-${dependent.ownerId}`"
                        >
                            <thead>
                                <tr>
                                    <th class="text-center" scope="col">
                                        Date
                                    </th>
                                    <th class="text-center" scope="col">
                                        Title
                                    </th>
                                    <th
                                        class="d-none d-md-table-cell text-center"
                                        scope="col"
                                    >
                                        Document Type
                                    </th>
                                    <th
                                        class="d-none d-md-table-cell text-center"
                                        scope="col"
                                    >
                                        Type
                                    </th>
                                    <th
                                        class="d-none d-md-table-cell text-center"
                                        scope="col"
                                    >
                                        Facility Name
                                    </th>
                                    <th class="text-center" scope="col">
                                        Report
                                    </th>
                                </tr>
                            </thead>
                            <tbody>
                                <tr
                                    v-for="(row, index) in clinicalDocuments"
                                    :key="index"
                                >
                                    <td
                                        :data-testid="`clinical-document-service-date-${dependent.ownerId}-${index}`"
                                        class="text-center text-nowrap"
                                    >
                                        {{ formatDate(row.serviceDate) }}
                                    </td>
                                    <td
                                        :data-testid="`clinical-document-name-${dependent.ownerId}-${index}`"
                                        class="text-center"
                                    >
                                        {{ row.name }}
                                    </td>
                                    <td
                                        :data-testid="`clinical-document-type-${dependent.ownerId}-${index}`"
                                        class="d-none d-md-table-cell text-center"
                                    >
                                        {{ row.type }}
                                    </td>
                                    <td
                                        :data-testid="`clinical-document-discipline-${dependent.ownerId}-${index}`"
                                        class="d-none d-md-table-cell text-center"
                                    >
                                        {{ row.discipline }}
                                    </td>
                                    <td
                                        :data-testid="`clinical-document-facility-name-${dependent.ownerId}-${index}`"
                                        class="d-none d-md-table-cell text-center"
                                    >
                                        {{ row.facilityName }}
                                    </td>
                                    <td class="text-center">
                                        <HgIconButtonComponent
                                            :data-testid="`clinical-document-report-download-button-${dependent.ownerId}-${index}`"
                                            variant="link"
                                            color="primary"
                                            icon="download"
                                            size="small"
                                            @click="
                                                showClinicalDocumentDownloadConfirmationModal(
                                                    row
                                                )
                                            "
                                        />
                                    </td>
                                </tr>
                            </tbody>
                        </v-table>
                    </v-window-item>
                </v-window>
            </v-card-text>
        </v-card>
        <MessageModalComponent
            ref="deleteModal"
            title="Remove Dependent"
            message="Are you sure you want to remove this dependent?"
            submit-text="Remove Dependent"
            @submit="deleteDependent"
        />
        <MessageModalComponent
            ref="reportDownloadModal"
            title="Sensitive Document"
            message="The file that you are downloading contains personal information. If you are on a public computer, please ensure that the file is deleted before you log off."
            @submit="downloadDocument"
        />
        <MessageModalComponent
            ref="vaccineRecordResultModal"
            ok-only
            title="Alert"
            :message="vaccineRecordState.resultMessage"
        />
    </div>
</template>
