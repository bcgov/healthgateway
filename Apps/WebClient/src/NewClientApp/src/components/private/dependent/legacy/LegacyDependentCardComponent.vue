<script setup lang="ts">
import { saveAs } from "file-saver";
import { computed, ref, watch } from "vue";

import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import HgIconButtonComponent from "@/components/common/HgIconButtonComponent.vue";
import LoadingComponent from "@/components/common/LoadingComponent.vue";
import MessageModalComponent from "@/components/common/MessageModalComponent.vue";
import { ActionType } from "@/constants/actionType";
import { EntryType } from "@/constants/entryType";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { ResultType } from "@/constants/resulttype";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { ClinicalDocument } from "@/models/clinicalDocument";
import { DateWrapper, StringISODate } from "@/models/dateWrapper";
import type { Dependent } from "@/models/dependent";
import EncodedMedia from "@/models/encodedMedia";
import { ResultError } from "@/models/errors";
import {
    ImmunizationAgent,
    ImmunizationEvent,
    Recommendation,
} from "@/models/immunizationModel";
import { Covid19LaboratoryTest, LaboratoryOrder } from "@/models/laboratory";
import Report from "@/models/report";
import ReportHeader from "@/models/reportHeader";
import { ReportFormatType, TemplateType } from "@/models/reportRequest";
import RequestResult from "@/models/requestResult";
import { LoadStatus } from "@/models/storeOperations";
import User from "@/models/user";
import {
    IClinicalDocumentService,
    IDependentService,
    IImmunizationService,
    ILaboratoryService,
    ILogger,
    IReportService,
} from "@/services/interfaces";
import { useConfigStore } from "@/stores/config";
import { useErrorStore } from "@/stores/error";
import { useUserStore } from "@/stores/user";
import { useVaccinationStatusAuthenticatedStore } from "@/stores/vaccinationStatusAuthenticated";
import ConfigUtil from "@/utility/configUtil";
import SnowPlow from "@/utility/snowPlow";

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

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const clinicalDocumentService = container.get<IClinicalDocumentService>(
    SERVICE_IDENTIFIER.ClinicalDocumentService
);
const immunizationService = container.get<IImmunizationService>(
    SERVICE_IDENTIFIER.ImmunizationService
);
const laboratoryService = container.get<ILaboratoryService>(
    SERVICE_IDENTIFIER.LaboratoryService
);
const dependentService = container.get<IDependentService>(
    SERVICE_IDENTIFIER.DependentService
);
const userStore = useUserStore();
const configStore = useConfigStore();
const vaccinationStatusStore = useVaccinationStatusAuthenticatedStore();
const errorStore = useErrorStore();

const reportFormatType = ref(ReportFormatType.PDF);
const csvFormatType = ref(ReportFormatType.CSV);
const pdfFormatType = ref(ReportFormatType.PDF);
const xlsxFormatType = ref(ReportFormatType.XLSX);
const isLoading = ref(false);
const laboratoryOrders = ref<LaboratoryOrder[]>([]);
const clinicalDocuments = ref<ClinicalDocument[]>([]);
const testRows = ref<Covid19LaboratoryTestRow[]>([]);
const immunizations = ref<ImmunizationEvent[]>([]);
const recommendations = ref<Recommendation[]>([]);
const isCovid19DataLoading = ref(false);
const isImmunizationDataLoaded = ref(false);
const isLaboratoryOrdersDataLoaded = ref(false);
const isClinicalDocumentsDataLoaded = ref(false);
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

const user = computed<User>(() => userStore.user);
const webClientConfig = computed(() => configStore.webConfig);
const vaccineRecordState = computed(() =>
    vaccinationStatusStore.vaccineRecordState(props.dependent.ownerId)
);
const headerData = computed<ReportHeader>(() => {
    return {
        phn: props.dependent.dependentInformation.PHN,
        dateOfBirth: formatDate(
            props.dependent.dependentInformation.dateOfBirth || ""
        ),
        name: props.dependent.dependentInformation
            ? props.dependent.dependentInformation.firstname +
              " " +
              props.dependent.dependentInformation.lastname
            : "",
        isRedacted: false,
        datePrinted: new DateWrapper().format(),
        filterText: "",
    };
});
const isVaccineRecordDownloading = computed(
    () => vaccineRecordState.value.status === LoadStatus.REQUESTED
);
const isDownloadImmunizationReportButtonDisabled = computed(
    () =>
        isReportDownloading.value ||
        selectedTabIndex.value !== tabIndicesMap.value[2] ||
        (immunizationItems.value.length == 0 &&
            recommendationItems.value.length == 0)
);
const isExpired = computed(() => {
    const birthDate = new DateWrapper(
        props.dependent.dependentInformation.dateOfBirth
    );
    const now = new DateWrapper();
    return (
        now.diff(birthDate, "year").years >
        webClientConfig.value.maxDependentAge
    );
});
const isCovid19TabShown = computed(() =>
    ConfigUtil.isDependentDatasetEnabled(EntryType.Covid19TestResult)
);
const isImmunizationTabShown = computed(() =>
    ConfigUtil.isDependentDatasetEnabled(EntryType.Immunization)
);
const isLaboratoryOrderTabShown = computed(() =>
    ConfigUtil.isDependentDatasetEnabled(EntryType.LabResult)
);
const isClinicalDocumentTabShown = computed(() =>
    ConfigUtil.isDependentDatasetEnabled(EntryType.ClinicalDocument)
);
const tabIndicesMap = computed(() => {
    const tabIndices: (number | undefined)[] = [0];
    const optionalTabs = [
        isCovid19TabShown.value,
        isImmunizationTabShown.value,
        isLaboratoryOrderTabShown.value,
        isClinicalDocumentTabShown.value,
    ];

    let index = 0;
    optionalTabs.forEach((shown) =>
        tabIndices.push(shown ? ++index : undefined)
    );

    return tabIndices;
});
const immunizationItems = computed(() =>
    immunizations.value.map<ImmunizationRow>((x) => ({
        date: DateWrapper.format(x.dateOfImmunization),
        immunization: x.immunization.name,
        agent: getAgentNames(x.immunization.immunizationAgents),
        product: getProductNames(x.immunization.immunizationAgents),
        provider_clinic: x.providerOrClinic,
        lotNumber: getAgentLotNumbers(x.immunization.immunizationAgents),
    }))
);
const recommendationItems = computed(() =>
    recommendations.value.map<RecommendationRow>((x) => ({
        immunization: x.recommendedVaccinations,
        due_date:
            x.agentDueDate === undefined || x.agentDueDate === null
                ? ""
                : DateWrapper.format(x.agentDueDate),
    }))
);

function deleteDependent(): void {
    isLoading.value = true;
    dependentService
        .removeDependent(user.value.hdid, props.dependent)
        .then(() => emit("needs-update"))
        .catch((err: ResultError) => {
            if (err.statusCode === 429) {
                errorStore.setTooManyRequestsError("page");
            } else {
                errorStore.addError(
                    ErrorType.Delete,
                    ErrorSourceType.Dependent,
                    err.traceId
                );
            }
        })
        .finally(() => {
            isLoading.value = false;
        });
}

function downloadCovid19Report(): void {
    if (selectedTestRow.value === undefined) {
        return;
    }

    isReportDownloading.value = true;
    const test = selectedTestRow.value.test;
    laboratoryService
        .getReportDocument(
            selectedTestRow.value.id,
            props.dependent.ownerId,
            true
        )
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
            logger.error(err.resultMessage);
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
    trackClickLink("download_report", "Dependent Lab PDF");

    const dateString = new DateWrapper(
        selectedLaboratoryOrderRow.value.timelineDateTime,
        { hasTime: true }
    ).format("yyyy_MM_dd-HH_mm");

    laboratoryService
        .getReportDocument(
            selectedLaboratoryOrderRow.value.reportId,
            props.dependent.ownerId,
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
            logger.error(err.resultMessage);
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

    const action = "download_report";
    const reportName = "Dependent Immunization";
    const formatTypeName = ReportFormatType[reportFormatType.value];
    const eventName = `${reportName} (${formatTypeName})`;
    trackClickLink(action, eventName);

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
            logger.error(err.resultMessage);
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

    isReportDownloading.value = true;
    trackClickLink("download_report", "Dependent Clinical Doc");

    clinicalDocumentService
        .getFile(
            selectedClinicalDocumentRow.value.fileId,
            props.dependent.ownerId
        )
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
            logger.error(err.resultMessage);
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

        if (selectedTabIndex.value === tabIndicesMap.value[1]) {
            downloadCovid19Report();
        } else if (selectedTabIndex.value === tabIndicesMap.value[2]) {
            downloadImmunizationReport();
        } else if (selectedTabIndex.value === tabIndicesMap.value[3]) {
            downloadLaboratoryOrderReport();
        } else if (selectedTabIndex.value === tabIndicesMap.value[4]) {
            downloadClinicalDocument();
        }
    } else {
        downloadVaccinePdf();
    }
}

function downloadVaccinePdf(): void {
    logger.debug(
        `Downloading vaccine PDF for hdid: ${props.dependent.ownerId}`
    );
    trackClickLink("Click Button", "Dependent Proof");
    vaccinationStatusStore.retrieveVaccineRecord(props.dependent.ownerId);
}

function formatDate(date: StringISODate): string {
    return new DateWrapper(date).format();
}

function fetchClinicalDocuments(): void {
    const hdid = props.dependent.ownerId;
    logger.debug(`Fetching Clinical Documents for Hdid: ${hdid}`);
    if (isClinicalDocumentsDataLoaded.value) {
        return;
    }
    isLoading.value = true;
    clinicalDocumentService
        .getRecords(hdid)
        .then((result) => {
            if (result.resultStatus == ResultType.Success) {
                const payload = result.resourcePayload;
                setClinicalDocuments(payload);
                isClinicalDocumentsDataLoaded.value = true;
            } else {
                logger.error(
                    `Error returned from the Clinical Documents call:
                        ${JSON.stringify(result.resultError)}`
                );
                errorStore.addError(
                    ErrorType.Retrieve,
                    ErrorSourceType.ClinicalDocument,
                    result.resultError?.traceId
                );
            }
        })
        .catch((err: ResultError) => {
            logger.error(err.resultMessage);
            if (err.statusCode === 429) {
                errorStore.setTooManyRequestsWarning("page");
            } else {
                errorStore.addError(
                    ErrorType.Retrieve,
                    ErrorSourceType.ClinicalDocument,
                    err.traceId
                );
            }
        })
        .finally(() => {
            isLoading.value = false;
        });
}

function fetchCovid19LaboratoryTests(): void {
    logger.debug(
        `Fetching COVID 19 Laboratory Tests for Hdid: ${props.dependent.ownerId}`
    );
    if (isCovid19DataLoading.value) {
        return;
    }
    isLoading.value = true;
    laboratoryService
        .getCovid19LaboratoryOrders(props.dependent.ownerId)
        .then((result) => {
            const payload = result.resourcePayload;
            if (result.resultStatus == ResultType.Success) {
                testRows.value =
                    payload.orders.flatMap<Covid19LaboratoryTestRow>((o) =>
                        o.labResults.map<Covid19LaboratoryTestRow>((r) => ({
                            id: o.id,
                            reportAvailable: o.reportAvailable,
                            test: r,
                        }))
                    );
                sortEntries();
                isCovid19DataLoading.value = true;
            } else if (
                result.resultError?.actionCode === ActionType.Refresh &&
                !payload.loaded &&
                payload.retryin > 0
            ) {
                logger.info("Re-querying for COVID-19 Laboratory Orders");
                setTimeout(
                    () => fetchCovid19LaboratoryTests(),
                    payload.retryin
                );
            } else {
                logger.error(
                    "Error returned from the COVID-19 Laboratory Orders call: " +
                        JSON.stringify(result.resultError)
                );
                errorStore.addError(
                    ErrorType.Retrieve,
                    ErrorSourceType.Covid19Laboratory,
                    result.resultError?.traceId
                );
            }
        })
        .catch((err: ResultError) => {
            logger.error(err.resultMessage);
            if (err.statusCode === 429) {
                errorStore.setTooManyRequestsWarning("page");
            } else {
                errorStore.addError(
                    ErrorType.Retrieve,
                    ErrorSourceType.Covid19Laboratory,
                    err.traceId
                );
            }
        })
        .finally(() => {
            isLoading.value = false;
        });
}

function fetchLaboratoryOrders(): void {
    logger.debug(`Fetching Lab Results for Hdid: ${props.dependent.ownerId}`);
    if (isLaboratoryOrdersDataLoaded.value) {
        return;
    }
    isLoading.value = true;
    laboratoryService
        .getLaboratoryOrders(props.dependent.ownerId)
        .then((result) => {
            const payload = result.resourcePayload;
            if (result.resultStatus == ResultType.Success) {
                setLaboratoryOrders(payload.orders);
                isLaboratoryOrdersDataLoaded.value = true;
                isLoading.value = false;
            } else if (
                result.resultError?.actionCode === ActionType.Refresh &&
                !payload.loaded &&
                payload.retryin > 0
            ) {
                logger.info("Re-querying for Laboratory Orders");
                setTimeout(() => fetchLaboratoryOrders(), payload.retryin);
            } else {
                logger.error(
                    "Error returned from the Laboratory Orders call: " +
                        JSON.stringify(result.resultError)
                );
                errorStore.addError(
                    ErrorType.Retrieve,
                    ErrorSourceType.Laboratory,
                    result.resultError?.traceId
                );
                isLoading.value = false;
            }
        })
        .catch((err: ResultError) => {
            logger.error(err.resultMessage);
            if (err.statusCode === 429) {
                errorStore.setTooManyRequestsWarning("page");
            } else {
                errorStore.addError(
                    ErrorType.Retrieve,
                    ErrorSourceType.Laboratory,
                    err.traceId
                );
            }
            isLoading.value = false;
        });
}

function fetchPatientImmunizations(): void {
    const hdid = props.dependent.ownerId;
    logger.debug(`Fetching Patient Immunizations for Hdid: ${hdid}`);
    if (isImmunizationDataLoaded.value) {
        return;
    }
    isLoading.value = true;
    immunizationService
        .getPatientImmunizations(hdid)
        .then((result) => {
            if (result.resultStatus == ResultType.Success) {
                const payload = result.resourcePayload;
                if (payload.loadState.refreshInProgress) {
                    logger.info("Re-querying Patient Immunizations");
                    setTimeout(() => fetchPatientImmunizations(), 10000);
                } else {
                    setImmunizations(payload.immunizations);
                    setRecommendations(payload.recommendations);
                    isImmunizationDataLoaded.value = true;
                    logger.debug(
                        `Patient Immunizations:
                            ${JSON.stringify(immunizations.value)}`
                    );
                    logger.debug(
                        `Patient Recommendations:
                            ${JSON.stringify(recommendations.value)}`
                    );
                }
            } else {
                logger.error(
                    `Error returned from the Patient Immunizations call:
                        ${JSON.stringify(result.resultError)}`
                );
                errorStore.addError(
                    ErrorType.Retrieve,
                    ErrorSourceType.Immunization,
                    result.resultError?.traceId
                );
            }
        })
        .catch((err: ResultError) => {
            logger.error(err.resultMessage);
            if (err.statusCode === 429) {
                errorStore.setTooManyRequestsWarning("page");
            } else {
                errorStore.addError(
                    ErrorType.Retrieve,
                    ErrorSourceType.Immunization,
                    err.traceId
                );
            }
        })
        .finally(() => {
            isLoading.value = false;
        });
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

function setLaboratoryOrders(records: LaboratoryOrder[]): void {
    laboratoryOrders.value = [...records].sort((a, b) => {
        const firstDate = new DateWrapper(a.timelineDateTime, {
            hasTime: true,
        });
        const secondDate = new DateWrapper(b.timelineDateTime, {
            hasTime: true,
        });

        if (firstDate.isBefore(secondDate)) {
            return 1;
        }

        if (firstDate.isAfter(secondDate)) {
            return -1;
        }

        return 0;
    });
}

function setClinicalDocuments(records: ClinicalDocument[]): void {
    clinicalDocuments.value = [...records].sort((a, b) => {
        const firstDate = new DateWrapper(a.serviceDate);
        const secondDate = new DateWrapper(b.serviceDate);

        if (firstDate.isBefore(secondDate)) {
            return 1;
        }

        if (firstDate.isAfter(secondDate)) {
            return -1;
        }

        return 0;
    });
}

function setImmunizations(records: ImmunizationEvent[]): void {
    immunizations.value = [...records].sort((a, b) => {
        const firstDate = new DateWrapper(a.dateOfImmunization);
        const secondDate = new DateWrapper(b.dateOfImmunization);

        if (firstDate.isBefore(secondDate)) {
            return 1;
        }

        if (firstDate.isAfter(secondDate)) {
            return -1;
        }

        return 0;
    });
}

function setRecommendations(records: Recommendation[]): void {
    recommendations.value = records
        .filter((x) => x.recommendedVaccinations)
        .sort((a, b) => {
            const firstDateEmpty =
                a.agentDueDate === null || a.agentDueDate === undefined;
            const secondDateEmpty =
                b.agentDueDate === null || b.agentDueDate === undefined;

            if (firstDateEmpty && secondDateEmpty) {
                return 0;
            }

            if (firstDateEmpty) {
                return -1;
            }

            if (secondDateEmpty) {
                return 1;
            }

            const firstDate = new DateWrapper(a.agentDueDate);
            const secondDate = new DateWrapper(b.agentDueDate);

            if (firstDate.isBefore(secondDate)) {
                return -1;
            }

            if (firstDate.isAfter(secondDate)) {
                return 1;
            }

            return 0;
        });
}

function sortEntries(): void {
    testRows.value.sort((a, b) => {
        const dateA = new DateWrapper(a.test.collectedDateTime);
        const dateB = new DateWrapper(b.test.collectedDateTime);
        if (dateA.isBefore(dateB)) {
            return 1;
        }
        if (dateA.isAfter(dateB)) {
            return -1;
        }
        return 0;
    });
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

function trackClickLink(action: string, linkType: string | undefined): void {
    if (linkType) {
        SnowPlow.trackEvent({
            action: `${action}`,
            text: `${linkType}`,
        });
    }
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
            trackClickLink("Download Card", "Dependent Proof");
            res.blob().then((blob) => saveAs(blob, "VaccineProof.pdf"));
        });
        vaccinationStatusStore.stopVaccineRecordDownload(
            props.dependent.ownerId
        );
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
                    <v-tab>Profile</v-tab>
                    <v-tab
                        v-if="isCovid19TabShown"
                        :disabled="isExpired"
                        data-testid="covid19TabTitle"
                        @click="fetchCovid19LaboratoryTests"
                    >
                        COVID-19
                    </v-tab>
                    <v-tab
                        v-if="isImmunizationTabShown"
                        :disabled="isExpired"
                        :data-testid="`immunization-tab-title-${dependent.ownerId}`"
                        @click="fetchPatientImmunizations"
                    >
                        Immunization
                    </v-tab>
                    <!-- TODO: are the id attributes necessary -->
                    <v-tab
                        v-if="isLaboratoryOrderTabShown"
                        :id="`lab-results-tab-title-${dependent.ownerId}`"
                        :disabled="isExpired"
                        :data-testid="`lab-results-tab-title-${dependent.ownerId}`"
                        @click="fetchLaboratoryOrders"
                    >
                        Lab Results
                    </v-tab>
                    <v-tab
                        v-if="isClinicalDocumentTabShown"
                        :id="`clinical-document-tab-title-${dependent.ownerId}`"
                        :disabled="isExpired"
                        :data-testid="`clinical-document-tab-title-${dependent.ownerId}`"
                        @click="fetchClinicalDocuments"
                    >
                        Clinical Docs
                    </v-tab>
                </v-tabs>
            </v-card-title>
            <v-card-text class="pa-4">
                <v-window v-model="selectedTabIndex">
                    <v-window-item data-testid="dependentTab">
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
                                    <label>PHN</label>
                                    <v-text-field
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
                                    <label>Date of Birth</label>
                                    <v-text-field
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
                    <v-window-item data-testid="covid19Tab">
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
                        <v-progress-circular v-if="isLoading" indeterminate />
                        <template v-else>
                            <p
                                v-if="testRows.length === 0"
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
                                        <th class="text-center">Date</th>
                                        <th
                                            class="d-none d-md-table-cell text-center"
                                        >
                                            Type
                                        </th>
                                        <th
                                            class="d-none d-md-table-cell text-center"
                                        >
                                            Status
                                        </th>
                                        <th class="text-center">Result</th>
                                        <th class="text-center">Report</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    <tr
                                        v-for="(row, index) in testRows"
                                        :key="index"
                                    >
                                        <td
                                            data-testid="dependentCovidTestDate"
                                            class="text-center text-nowrap"
                                        >
                                            {{
                                                formatDate(
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
                                            <span v-if="row.test.resultReady">
                                                <HgIconButtonComponent
                                                    :id="
                                                        'dependent-covid-test-info-button-' +
                                                        index
                                                    "
                                                    aria-label="Result Description"
                                                    data-testid="dependent-covid-test-info-button"
                                                >
                                                    <v-icon
                                                        icon="info-circle"
                                                        size="small"
                                                    />
                                                    <v-overlay
                                                        activator="parent"
                                                        location-strategy="connected"
                                                        scroll-strategy="block"
                                                        open-on-hover
                                                    >
                                                        <v-card
                                                            data-testid="dependent-covid-test-info-popover"
                                                            class="pa-2"
                                                        >
                                                            <h1>
                                                                Implement the
                                                                test description
                                                                component
                                                            </h1>
                                                            <!-- <Covid19LaboratoryTestDescriptionComponent
                                                            class="pa-2"
                                                            :description="
                                                                row.test
                                                                    .resultDescription
                                                            "
                                                            :link="row.test.resultLink"
                                                        /> -->
                                                        </v-card>
                                                    </v-overlay>
                                                </HgIconButtonComponent>
                                            </span>
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
                    >
                        <div
                            id="dependent-immunization-disclaimer"
                            class="mb-4"
                        >
                            <v-alert
                                v-if="immunizationItems.length != 0"
                                type="info"
                                variant="outlined"
                                border
                                data-testid="dependent-immunization-disclaimer-alert"
                            >
                                <template #text>
                                    <span class="text-body-1">
                                        If your dependent's immunizations are
                                        missing or incorrect,
                                        <a
                                            href="https://www.immunizationrecord.gov.bc.ca/"
                                            target="_blank"
                                            rel="noopener"
                                            >fill in this online form</a
                                        >.
                                    </span>
                                </template>
                            </v-alert>
                        </div>
                        <v-progress-circular
                            v-if="isLoading"
                            indeterminate
                            class="ma-4"
                        />
                        <div
                            v-else
                            :data-testid="`immunization-tab-div-${dependent.ownerId}`"
                        >
                            <v-tabs
                                v-model="immunizationTabIndex"
                                color="primary"
                            >
                                <v-tab :key="1"> History</v-tab>
                                <v-tab :key="2"> Recommendations </v-tab>
                            </v-tabs>
                            <v-window
                                v-model="immunizationTabIndex"
                                class="pa-4"
                            >
                                <v-window-item>
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
                                                    <th class="text-center">
                                                        Date
                                                    </th>
                                                    <th class="text-center">
                                                        Immunization
                                                    </th>
                                                    <th
                                                        class="d-none d-lg-table-cell text-center"
                                                    >
                                                        Agent
                                                    </th>
                                                    <th
                                                        class="d-none d-lg-table-cell text-center"
                                                    >
                                                        Product
                                                    </th>
                                                    <th class="text-center">
                                                        Provider/Clinic
                                                    </th>
                                                    <th
                                                        class="d-none d-lg-table-cell text-center"
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
                                <v-window-item title="Forecasts">
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
                                                id="download-immunization-forecast-report-btn"
                                                :data-testid="`download-immunization-forecast-report-btn-${dependent.ownerId}`"
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
                                                        :disabled="
                                                            isDownloadImmunizationReportButtonDisabled
                                                        "
                                                    />
                                                </template>
                                                <v-list>
                                                    <v-list-item
                                                        :data-testid="`download-immunization-forecast-report-pdf-btn-${dependent.ownerId}`"
                                                        title="PDF"
                                                        @click="
                                                            showImmunizationDownloadConfirmationModal(
                                                                pdfFormatType
                                                            )
                                                        "
                                                    />
                                                    <v-list-item
                                                        :data-testid="`download-immunization-forecast-report-csv-btn-${dependent.ownerId}`"
                                                        title="CSV"
                                                        @click="
                                                            showImmunizationDownloadConfirmationModal(
                                                                csvFormatType
                                                            )
                                                        "
                                                    />
                                                    <v-list-item
                                                        :data-testid="`download-immunization-forecast-report-xlsx-btn-${dependent.ownerId}`"
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
                                        :data-testid="`immunization-forecast-no-rows-found-${dependent.ownerId}`"
                                        class="text-body-1 my-4"
                                    >
                                        No records found.
                                    </p>
                                    <v-table
                                        v-else
                                        class="w-100 mb-0"
                                        aria-label="Immunization Forecasts"
                                        :data-testid="`immunization-forecast-table-${dependent.ownerId}`"
                                    >
                                        <thead>
                                            <tr>
                                                <th class="text-center">
                                                    Immunization
                                                </th>
                                                <th class="text-center">
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
                                                    :data-testid="`forecast-immunization-${dependent.ownerId}-${index}`"
                                                    class="text-center"
                                                >
                                                    {{ row.immunization }}
                                                </td>
                                                <td
                                                    :data-testid="`forecast-due-date-${dependent.ownerId}-${index}`"
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
                    >
                        <v-progress-circular
                            v-if="isLoading"
                            indeterminate
                            class="mt-3"
                        />
                        <div
                            v-else-if="laboratoryOrders.length === 0"
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
                                    <th class="text-center">Date</th>
                                    <th class="text-center">Title</th>
                                    <th
                                        class="d-none d-lg-table-cell text-center"
                                    >
                                        Lab
                                    </th>
                                    <th
                                        class="d-none d-md-table-cell text-center"
                                    >
                                        Status
                                    </th>
                                    <th class="text-center">Detailed Report</th>
                                </tr>
                            </thead>
                            <tbody>
                                <tr
                                    v-for="(row, index) in laboratoryOrders"
                                    :key="index"
                                >
                                    <td
                                        :data-testid="`lab-results-date-${dependent.ownerId}-${index}`"
                                        class="text-center text-nowrap"
                                    >
                                        {{ formatDate(row.timelineDateTime) }}
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
                    >
                        <v-progress-circular
                            v-if="isLoading"
                            indeterminate
                            class="mt-3"
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
                                    <th class="text-center">Date</th>
                                    <th class="text-center">Title</th>
                                    <th
                                        class="d-none d-md-table-cell text-center"
                                    >
                                        Document Type
                                    </th>
                                    <th
                                        class="d-none d-md-table-cell text-center"
                                    >
                                        Discipline
                                    </th>
                                    <th
                                        class="d-none d-md-table-cell text-center"
                                    >
                                        Facility Name
                                    </th>
                                    <th class="text-center">Report</th>
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
            title="Sensitive Document Download"
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
