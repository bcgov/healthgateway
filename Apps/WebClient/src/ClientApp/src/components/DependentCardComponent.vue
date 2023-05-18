<script setup lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import {
    faDownload,
    faEllipsisV,
    faInfoCircle,
} from "@fortawesome/free-solid-svg-icons";
import { saveAs } from "file-saver";
import { computed, ref, watch } from "vue";
import { useStore } from "vue-composition-wrapper";

import Covid19LaboratoryTestDescriptionComponent from "@/components/laboratory/Covid19LaboratoryTestDescriptionComponent.vue";
import LoadingComponent from "@/components/LoadingComponent.vue";
import DeleteModalComponent from "@/components/modal/DeleteModalComponent.vue";
import MessageModalComponent from "@/components/modal/MessageModalComponent.vue";
import { ActionType } from "@/constants/actionType";
import { EntryType } from "@/constants/entryType";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { ResultType } from "@/constants/resulttype";
import { ClinicalDocument } from "@/models/clinicalDocument";
import type { WebClientConfiguration } from "@/models/configData";
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
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import {
    IClinicalDocumentService,
    IDependentService,
    IImmunizationService,
    ILaboratoryService,
    ILogger,
    IReportService,
} from "@/services/interfaces";
import ConfigUtil from "@/utility/configUtil";
import SnowPlow from "@/utility/snowPlow";

library.add(faEllipsisV, faDownload, faInfoCircle);

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
const store = useStore();

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
const selectedTestRow = ref<Covid19LaboratoryTestRow>();
const selectedLaboratoryOrderRow = ref<LaboratoryOrder>();
const selectedClinicalDocumentRow = ref<ClinicalDocument>();

const reportDownloadModal = ref<MessageModalComponent>();
const vaccineRecordResultModal = ref<MessageModalComponent>();
const deleteModal = ref<DeleteModalComponent>();

const user = computed<User>(() => store.getters["user/user"]);
const webClientConfig = computed<WebClientConfiguration>(
    () => store.getters["config/webClient"]
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
const isVaccineRecordDownloading = computed<boolean>(
    () => vaccineRecordState.value.status === LoadStatus.REQUESTED
);
const isDownloadImmunizationReportButtonDisabled = computed<boolean>(
    () =>
        isReportDownloading.value ||
        selectedTabIndex.value !== tabIndicesMap.value[2] ||
        (immunizationItems.value.length == 0 &&
            recommendationItems.value.length == 0)
);
const isExpired = computed<boolean>(() => {
    const birthDate = new DateWrapper(
        props.dependent.dependentInformation.dateOfBirth
    );
    const now = new DateWrapper();
    return (
        now.diff(birthDate, "year").years >
        webClientConfig.value.maxDependentAge
    );
});
const isCovid19TabShown = computed<boolean>(() =>
    ConfigUtil.isDependentDatasetEnabled(EntryType.Covid19TestResult)
);
const isImmunizationTabShown = computed<boolean>(() =>
    ConfigUtil.isDependentDatasetEnabled(EntryType.Immunization)
);
const isLaboratoryOrderTabShown = computed<boolean>(() =>
    ConfigUtil.isDependentDatasetEnabled(EntryType.LabResult)
);
const isClinicalDocumentTabShown = computed<boolean>(() =>
    ConfigUtil.isDependentDatasetEnabled(EntryType.ClinicalDocument)
);
const tabIndicesMap = computed<(number | undefined)[]>(() => {
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
const immunizationItems = computed<ImmunizationRow[]>(() =>
    immunizations.value.map<ImmunizationRow>((x) => ({
        date: DateWrapper.format(x.dateOfImmunization),
        immunization: x.immunization.name,
        agent: getAgentNames(x.immunization.immunizationAgents),
        product: getProductNames(x.immunization.immunizationAgents),
        provider_clinic: x.providerOrClinic,
        lotNumber: getAgentLotNumbers(x.immunization.immunizationAgents),
    }))
);
const recommendationItems = computed<RecommendationRow[]>(() =>
    recommendations.value.map<RecommendationRow>((x) => ({
        immunization: x.recommendedVaccinations,
        due_date:
            x.agentDueDate === undefined || x.agentDueDate === null
                ? ""
                : DateWrapper.format(x.agentDueDate),
    }))
);
const vaccineRecordState = computed<VaccinationRecord>(() =>
    store.getters["vaccinationStatus/authenticatedVaccineRecordState"](
        props.dependent.ownerId
    )
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

function setTooManyRequestsWarning(key: string): void {
    store.dispatch("errorBanner/setTooManyRequestsWarning", { key });
}

function deleteDependent(): void {
    isLoading.value = true;
    dependentService
        .removeDependent(user.value.hdid, props.dependent)
        .then(() => emit("needs-update"))
        .catch((err: ResultError) => {
            if (err.statusCode === 429) {
                setTooManyRequestsError("page");
            } else {
                addError(
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
                setTooManyRequestsError("page");
            } else {
                addError(
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

    laboratoryService
        .getReportDocument(
            selectedLaboratoryOrderRow.value.reportId,
            props.dependent.ownerId,
            false
        )
        .then((result) => {
            const report = result.resourcePayload;
            const dateString = new DateWrapper(
                selectedLaboratoryOrderRow.value.timelineDateTime,
                { hasTime: true }
            ).format("yyyy_MM_dd-HH_mm");
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
                setTooManyRequestsError("page");
            } else {
                addError(
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
                setTooManyRequestsError("page");
            } else {
                addError(
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
                setTooManyRequestsError("page");
            } else {
                addError(
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
    store.dispatch("vaccinationStatus/retrieveAuthenticatedVaccineRecord", {
        hdid: props.dependent.ownerId,
    });
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
                addError(
                    ErrorType.Retrieve,
                    ErrorSourceType.ClinicalDocument,
                    result.resultError?.traceId
                );
            }
        })
        .catch((err: ResultError) => {
            logger.error(err.resultMessage);
            if (err.statusCode === 429) {
                setTooManyRequestsWarning("page");
            } else {
                addError(
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
                addError(
                    ErrorType.Retrieve,
                    ErrorSourceType.Covid19Laboratory,
                    result.resultError?.traceId
                );
            }
        })
        .catch((err: ResultError) => {
            logger.error(err.resultMessage);
            if (err.statusCode === 429) {
                setTooManyRequestsWarning("page");
            } else {
                addError(
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
                addError(
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
                setTooManyRequestsWarning("page");
            } else {
                addError(
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
                addError(
                    ErrorType.Retrieve,
                    ErrorSourceType.Immunization,
                    result.resultError?.traceId
                );
            }
        })
        .catch((err: ResultError) => {
            logger.error(err.resultMessage);
            if (err.statusCode === 429) {
                setTooManyRequestsWarning("page");
            } else {
                addError(
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
        store.dispatch(
            "vaccinationStatus/stopAuthenticatedVaccineRecordDownload",
            { hdid: props.dependent.ownerId }
        );
    }
});
</script>

<template>
    <div>
        <b-card
            no-body
            :data-testid="`dependent-card-${dependent.dependentInformation.PHN}`"
        >
            <b-tabs v-model="selectedTabIndex" card>
                <b-tab no-body active data-testid="dependentTab">
                    <template #title>
                        <div>Profile</div>
                    </template>
                    <div v-if="isExpired" class="p-3">
                        <b-row>
                            <b-col class="d-flex justify-content-center">
                                <h5>Your access has expired</h5>
                            </b-col>
                        </b-row>
                        <b-row>
                            <b-col class="d-flex justify-content-center">
                                <p>
                                    You no longer have access to this dependent
                                    as they have turned
                                    {{ webClientConfig.maxDependentAge }}
                                </p>
                            </b-col>
                        </b-row>
                        <b-row>
                            <b-col class="d-flex justify-content-center">
                                <hg-button
                                    variant="secondary"
                                    @click="deleteDependent()"
                                >
                                    Remove Dependent
                                </hg-button>
                            </b-col>
                        </b-row>
                    </div>
                    <div v-else class="p-3">
                        <b-row>
                            <b-col class="col-12 col-md-6 col-lg-4">
                                <b-row>
                                    <b-col class="col-12">PHN</b-col>
                                    <b-col class="col-12">
                                        <b-form-input
                                            :value="
                                                dependent.dependentInformation
                                                    .PHN
                                            "
                                            data-testid="dependentPHN"
                                            readonly
                                        />
                                    </b-col>
                                </b-row>
                            </b-col>
                            <b-col class="col-12 col-md-6 col-lg-4">
                                <b-row>
                                    <b-col class="col-12">Date of Birth</b-col>
                                    <b-col class="col-12">
                                        <b-form-input
                                            :value="
                                                formatDate(
                                                    dependent
                                                        .dependentInformation
                                                        .dateOfBirth
                                                )
                                            "
                                            data-testid="dependentDOB"
                                            readonly
                                        />
                                    </b-col>
                                </b-row>
                            </b-col>
                        </b-row>
                    </div>
                </b-tab>
                <b-tab
                    v-if="isCovid19TabShown"
                    :disabled="isExpired"
                    data-testid="covid19Tab"
                    no-body
                    @click="fetchCovid19LaboratoryTests"
                >
                    <template #title>
                        <div data-testid="covid19TabTitle">COVID‑19</div>
                    </template>
                    <div class="p-3">
                        <div class="d-flex justify-content-center">
                            <hg-button
                                :id="`download-proof-of-vaccination-btn-id-${dependent.ownerId}`"
                                :data-testid="`download-proof-of-vaccination-btn-${dependent.ownerId}`"
                                variant="secondary"
                                @click="
                                    showVaccineProofDownloadConfirmationModal
                                "
                            >
                                <hg-icon
                                    icon="check-circle"
                                    size="medium"
                                    square
                                    aria-hidden="true"
                                    class="mr-2"
                                />
                                Download Proof of Vaccination
                            </hg-button>
                        </div>
                        <b-spinner v-if="isLoading" class="mt-3" />
                        <h4
                            v-if="!isLoading"
                            :id="`covid19-table-header-${dependent.ownerId}`"
                            class="pt-3"
                        >
                            COVID-19 Test Results
                        </h4>
                        <div
                            v-if="!isLoading && testRows.length === 0"
                            data-testid="covid19NoRecords"
                        >
                            No records found.
                        </div>
                    </div>
                    <b-table-simple
                        v-if="!isLoading && testRows.length > 0"
                        small
                        striped
                        borderless
                        :items="testRows"
                        class="w-100 mb-0"
                        :aria-labelledby="`covid19-table-header-${dependent.ownerId}`"
                        :data-testid="`covid19-table-${dependent.ownerId}`"
                    >
                        <b-thead>
                            <b-tr>
                                <b-th class="align-middle">Date</b-th>
                                <b-th
                                    class="d-none d-md-table-cell align-middle"
                                >
                                    Type
                                </b-th>
                                <b-th
                                    class="d-none d-md-table-cell align-middle"
                                >
                                    Status
                                </b-th>
                                <b-th class="align-middle">Result</b-th>
                                <b-th class="align-middle">Report</b-th>
                            </b-tr>
                        </b-thead>
                        <b-tbody>
                            <b-tr v-for="(row, index) in testRows" :key="index">
                                <b-td
                                    data-testid="dependentCovidTestDate"
                                    class="align-middle text-nowrap"
                                >
                                    {{ formatDate(row.test.collectedDateTime) }}
                                </b-td>
                                <b-td
                                    data-testid="dependentCovidTestType"
                                    class="d-none d-md-table-cell align-middle"
                                >
                                    {{ row.test.testType }}
                                </b-td>
                                <b-td
                                    data-testid="dependentCovidTestStatus"
                                    class="d-none d-md-table-cell align-middle"
                                >
                                    {{ row.test.testStatus }}
                                </b-td>
                                <b-td
                                    data-testid="dependentCovidTestLabResult"
                                    class="align-middle"
                                >
                                    <span
                                        v-if="row.test.filteredLabResultOutcome"
                                        class="font-weight-bold"
                                        :class="
                                            getOutcomeClasses(
                                                row.test.labResultOutcome
                                            )
                                        "
                                    >
                                        {{ row.test.filteredLabResultOutcome }}
                                    </span>
                                    <span v-if="row.test.resultReady">
                                        <hg-button
                                            :id="
                                                'dependent-covid-test-info-button-' +
                                                index
                                            "
                                            aria-label="Result Description"
                                            href="#"
                                            variant="link"
                                            data-testid="dependent-covid-test-info-button"
                                            class="shadow-none p-0 ml-1"
                                        >
                                            <hg-icon
                                                icon="info-circle"
                                                size="small"
                                            />
                                        </hg-button>
                                        <b-popover
                                            :target="
                                                'dependent-covid-test-info-button-' +
                                                index
                                            "
                                            triggers="hover focus"
                                            placement="bottomleft"
                                            boundary="viewport"
                                            data-testid="dependent-covid-test-info-popover"
                                        >
                                            <Covid19LaboratoryTestDescriptionComponent
                                                class="p-2"
                                                :description="
                                                    row.test.resultDescription
                                                "
                                                :link="row.test.resultLink"
                                            />
                                        </b-popover>
                                    </span>
                                </b-td>
                                <b-td class="align-middle">
                                    <hg-button
                                        v-if="
                                            row.reportAvailable &&
                                            row.test.resultReady
                                        "
                                        data-testid="dependentCovidReportDownloadBtn"
                                        variant="link"
                                        class="p-0"
                                        @click="
                                            showCovid19DownloadConfirmationModal(
                                                row
                                            )
                                        "
                                    >
                                        <hg-icon
                                            icon="download"
                                            size="medium"
                                            square
                                            aria-hidden="true"
                                        />
                                    </hg-button>
                                </b-td>
                            </b-tr>
                        </b-tbody>
                    </b-table-simple>
                </b-tab>
                <b-tab
                    v-if="isImmunizationTabShown"
                    :disabled="isExpired"
                    no-body
                    :data-testid="`immunization-tab-${dependent.ownerId}`"
                    @click="fetchPatientImmunizations"
                >
                    <template #title>
                        <div
                            :data-testid="`immunization-tab-title-${dependent.ownerId}`"
                        >
                            Immunization
                        </div>
                    </template>
                    <div id="dependent-immunization-disclaimer">
                        <b-alert
                            :show="immunizationItems.length != 0"
                            variant="info"
                            class="mt-3 mb-1 mx-3"
                            data-testid="dependent-immunization-disclaimer-alert"
                        >
                            <span>
                                If your dependent's immunizations are missing or
                                incorrect,
                                <a
                                    href="https://www.immunizationrecord.gov.bc.ca/"
                                    target="_blank"
                                    rel="noopener"
                                    >fill in this online form</a
                                >.
                            </span>
                        </b-alert>
                    </div>
                    <b-spinner v-if="isLoading" class="m-3" />
                    <div
                        v-else
                        :data-testid="`immunization-tab-div-${dependent.ownerId}`"
                    >
                        <b-card no-body class="border-0">
                            <b-tabs card nav-wrapper-class="bg-white">
                                <b-tab title="History" no-body active>
                                    <div
                                        v-if="immunizationItems.length === 0"
                                        class="p-3"
                                        :data-testid="`immunization-history-no-rows-found-${dependent.ownerId}`"
                                    >
                                        No records found. If this is your first
                                        time checking for records, please try
                                        refreshing the page in a few minutes.
                                    </div>
                                    <div v-else>
                                        <b-row
                                            align-h="end"
                                            class="p-3"
                                            no-gutters
                                        >
                                            <b-col
                                                cols="auto"
                                                align-self="center"
                                            >
                                                <b-dropdown
                                                    v-if="
                                                        immunizationItems.length !=
                                                        0
                                                    "
                                                    id="download-immunization-history-report-btn"
                                                    text="Download"
                                                    variant="outline-dark"
                                                    :data-testid="`download-immunization-history-report-btn-${dependent.ownerId}`"
                                                    :disabled="
                                                        isDownloadImmunizationReportButtonDisabled
                                                    "
                                                >
                                                    <b-dropdown-item
                                                        :data-testid="`download-immunization-history-report-pdf-btn-${dependent.ownerId}`"
                                                        @click="
                                                            showImmunizationDownloadConfirmationModal(
                                                                pdfFormatType
                                                            )
                                                        "
                                                        >PDF</b-dropdown-item
                                                    >
                                                    <b-dropdown-item
                                                        :data-testid="`download-immunization-history-report-csv-btn-${dependent.ownerId}`"
                                                        @click="
                                                            showImmunizationDownloadConfirmationModal(
                                                                csvFormatType
                                                            )
                                                        "
                                                        >CSV</b-dropdown-item
                                                    >
                                                    <b-dropdown-item
                                                        :data-testid="`download-immunization-history-report-xlsx-btn-${dependent.ownerId}`"
                                                        @click="
                                                            showImmunizationDownloadConfirmationModal(
                                                                xlsxFormatType
                                                            )
                                                        "
                                                        >XLSX</b-dropdown-item
                                                    >
                                                </b-dropdown>
                                            </b-col>
                                        </b-row>
                                        <b-table-simple
                                            small
                                            striped
                                            borderless
                                            :items="immunizationItems"
                                            class="w-100 mb-0"
                                            aria-label="Immunization History"
                                            :data-testid="`immunization-history-table-${dependent.ownerId}`"
                                        >
                                            <b-thead>
                                                <b-tr>
                                                    <b-th class="align-middle">
                                                        Date
                                                    </b-th>
                                                    <b-th class="align-middle">
                                                        Immunization
                                                    </b-th>
                                                    <b-th
                                                        class="d-none d-lg-table-cell align-middle"
                                                    >
                                                        Agent
                                                    </b-th>
                                                    <b-th
                                                        class="d-none d-lg-table-cell align-middle"
                                                    >
                                                        Product
                                                    </b-th>
                                                    <b-th class="align-middle">
                                                        Provider/Clinic
                                                    </b-th>
                                                    <th
                                                        class="d-none d-lg-table-cell align-middle"
                                                    >
                                                        Lot Number
                                                    </th>
                                                </b-tr>
                                            </b-thead>
                                            <b-tbody>
                                                <b-tr
                                                    v-for="(
                                                        row, index
                                                    ) in immunizationItems"
                                                    :key="index"
                                                >
                                                    <b-td
                                                        :data-testid="`history-immunization-date-${dependent.ownerId}-${index}`"
                                                        class="align-middle text-nowrap"
                                                    >
                                                        {{ row.date }}
                                                    </b-td>
                                                    <b-td
                                                        :data-testid="`history-product-${dependent.ownerId}-${index}`"
                                                        class="align-middle"
                                                    >
                                                        {{ row.immunization }}
                                                    </b-td>
                                                    <b-td
                                                        :data-testid="`history-immunizing-agent-${dependent.ownerId}-${index}`"
                                                        class="d-none d-lg-table-cell align-middle"
                                                    >
                                                        {{ row.agent }}
                                                    </b-td>
                                                    <b-td
                                                        :data-testid="`history-immunizing-product-${dependent.ownerId}-${index}`"
                                                        class="d-none d-lg-table-cell align-middle"
                                                    >
                                                        {{ row.product }}
                                                    </b-td>
                                                    <b-td
                                                        :data-testid="`history-provider-clinic-${dependent.ownerId}-${index}`"
                                                        class="align-middle"
                                                    >
                                                        {{
                                                            row.provider_clinic
                                                        }}
                                                    </b-td>
                                                    <b-td
                                                        :data-testid="`history-lot-number-${dependent.ownerId}-${index}`"
                                                        class="d-none d-lg-table-cell align-middle"
                                                    >
                                                        {{ row.lotNumber }}
                                                    </b-td>
                                                </b-tr>
                                            </b-tbody>
                                        </b-table-simple>
                                    </div>
                                </b-tab>
                                <b-tab title="Forecasts" no-body>
                                    <div class="p-3">
                                        <b-row align-h="end" no-gutters>
                                            <b-col cols="12" :md="true">
                                                <p class="mb-md-0">
                                                    School-aged children are
                                                    offered most immunizations
                                                    in their school,
                                                    particularly in grades 6 and
                                                    9. The school can let you
                                                    know which vaccines are
                                                    offered. You need to book an
                                                    appointment to get your
                                                    child vaccinated against
                                                    COVID‑19.
                                                    <a
                                                        href="https://www2.gov.bc.ca/gov/content/covid-19/vaccine"
                                                        rel="noopener"
                                                        target="_blank"
                                                        >Find out how.</a
                                                    >
                                                </p>
                                            </b-col>
                                            <b-col
                                                v-if="
                                                    recommendationItems.length >
                                                    0
                                                "
                                                cols="auto"
                                                align-self="center"
                                                class="pl-3"
                                            >
                                                <b-dropdown
                                                    id="download-immunization-forecast-report-btn"
                                                    text="Download"
                                                    variant="outline-dark"
                                                    :data-testid="`download-immunization-forecast-report-btn-${dependent.ownerId}`"
                                                    :disabled="
                                                        isDownloadImmunizationReportButtonDisabled
                                                    "
                                                >
                                                    <b-dropdown-item
                                                        :data-testid="`download-immunization-forecast-report-pdf-btn-${dependent.ownerId}`"
                                                        @click="
                                                            showImmunizationDownloadConfirmationModal(
                                                                pdfFormatType
                                                            )
                                                        "
                                                        >PDF</b-dropdown-item
                                                    >
                                                    <b-dropdown-item
                                                        :data-testid="`download-immunization-forecast-report-csv-btn-${dependent.ownerId}`"
                                                        @click="
                                                            showImmunizationDownloadConfirmationModal(
                                                                csvFormatType
                                                            )
                                                        "
                                                        >CSV</b-dropdown-item
                                                    >
                                                    <b-dropdown-item
                                                        :data-testid="`download-immunization-forecast-report-xlsx-btn-${dependent.ownerId}`"
                                                        @click="
                                                            showImmunizationDownloadConfirmationModal(
                                                                xlsxFormatType
                                                            )
                                                        "
                                                        >XLSX</b-dropdown-item
                                                    >
                                                </b-dropdown>
                                            </b-col>
                                        </b-row>
                                        <div
                                            v-if="
                                                recommendationItems.length === 0
                                            "
                                            :data-testid="`immunization-forecast-no-rows-found-${dependent.ownerId}`"
                                        >
                                            No records found.
                                        </div>
                                    </div>
                                    <b-table-simple
                                        v-if="recommendationItems.length > 0"
                                        small
                                        striped
                                        borderless
                                        class="w-100 mb-0"
                                        aria-label="Immunization Forecasts"
                                        :data-testid="`immunization-forecast-table-${dependent.ownerId}`"
                                    >
                                        <b-thead>
                                            <b-tr>
                                                <b-th class="align-middle">
                                                    Immunization
                                                </b-th>
                                                <b-th class="align-middle">
                                                    Due Date
                                                </b-th>
                                            </b-tr>
                                        </b-thead>
                                        <b-tbody>
                                            <b-tr
                                                v-for="(
                                                    row, index
                                                ) in recommendationItems"
                                                :key="index"
                                            >
                                                <b-td
                                                    :data-testid="`forecast-immunization-${dependent.ownerId}-${index}`"
                                                    class="align-middle"
                                                >
                                                    {{ row.immunization }}
                                                </b-td>
                                                <b-td
                                                    :data-testid="`forecast-due-date-${dependent.ownerId}-${index}`"
                                                    class="align-middle text-nowrap"
                                                >
                                                    {{ row.due_date }}
                                                </b-td>
                                            </b-tr>
                                        </b-tbody>
                                    </b-table-simple>
                                </b-tab>
                            </b-tabs>
                        </b-card>
                    </div>
                </b-tab>
                <b-tab
                    v-if="isLaboratoryOrderTabShown"
                    :disabled="isExpired"
                    no-body
                    :data-testid="`lab-results-tab-${dependent.ownerId}`"
                    @click="fetchLaboratoryOrders"
                >
                    <template #title>
                        <div
                            :id="`lab-results-tab-title-${dependent.ownerId}`"
                            :data-testid="`lab-results-tab-title-${dependent.ownerId}`"
                        >
                            Lab Results
                        </div>
                    </template>
                    <div
                        v-if="isLoading || laboratoryOrders.length === 0"
                        class="p-3"
                    >
                        <b-spinner v-if="isLoading" class="mt-3" />
                        <div
                            v-if="!isLoading && laboratoryOrders.length === 0"
                            :data-testid="`lab-results-no-records-${dependent.ownerId}`"
                        >
                            No records found. If you just added your dependent,
                            it can take up to 24 hours to get their records.
                        </div>
                    </div>
                    <b-table-simple
                        v-if="!isLoading && laboratoryOrders.length > 0"
                        small
                        striped
                        borderless
                        :items="laboratoryOrders"
                        class="w-100 mb-0"
                        :aria-labelledby="`lab-results-tab-title-${dependent.ownerId}`"
                        :data-testid="`lab-results-table-${dependent.ownerId}`"
                    >
                        <b-thead>
                            <b-tr>
                                <b-th class="align-middle">Date</b-th>
                                <b-th class="align-middle">Title</b-th>
                                <b-th
                                    class="d-none d-lg-table-cell align-middle"
                                >
                                    Lab
                                </b-th>
                                <b-th
                                    class="d-none d-md-table-cell align-middle"
                                >
                                    Status
                                </b-th>
                                <b-th class="align-middle"
                                    >Detailed Report</b-th
                                >
                            </b-tr>
                        </b-thead>
                        <b-tbody>
                            <b-tr
                                v-for="(row, index) in laboratoryOrders"
                                :key="index"
                            >
                                <b-td
                                    :data-testid="`lab-results-date-${dependent.ownerId}-${index}`"
                                    class="align-middle text-nowrap"
                                >
                                    {{ formatDate(row.timelineDateTime) }}
                                </b-td>
                                <b-td
                                    :data-testid="`lab-results-title-${dependent.ownerId}-${index}`"
                                    class="align-middle"
                                >
                                    {{ row.commonName }}
                                </b-td>
                                <b-td
                                    :data-testid="`lab-results-lab-${dependent.ownerId}-${index}`"
                                    class="d-none d-lg-table-cell align-middle"
                                >
                                    {{ row.reportingSource }}
                                </b-td>
                                <b-td
                                    :data-testid="`lab-results-status-${dependent.ownerId}-${index}`"
                                    class="d-none d-md-table-cell align-middle"
                                >
                                    {{ row.orderStatus }}
                                </b-td>
                                <b-td class="align-middle">
                                    <hg-button
                                        v-if="row.reportAvailable"
                                        :data-testid="`lab-results-report-download-button-${dependent.ownerId}-${index}`"
                                        variant="link"
                                        class="p-0"
                                        @click="
                                            showLaboratoryOrderDownloadConfirmationModal(
                                                row
                                            )
                                        "
                                    >
                                        <hg-icon
                                            icon="download"
                                            size="medium"
                                            square
                                            aria-hidden="true"
                                        />
                                    </hg-button>
                                </b-td>
                            </b-tr>
                        </b-tbody>
                    </b-table-simple>
                </b-tab>
                <b-tab
                    v-if="isClinicalDocumentTabShown"
                    :disabled="isExpired"
                    no-body
                    :data-testid="`clinical-document-tab-${dependent.ownerId}`"
                    @click="fetchClinicalDocuments"
                >
                    <template #title>
                        <div
                            :id="`clinical-document-tab-title-${dependent.ownerId}`"
                            :data-testid="`clinical-document-tab-title-${dependent.ownerId}`"
                        >
                            Clinical Docs
                        </div>
                    </template>
                    <div
                        v-if="isLoading || clinicalDocuments.length === 0"
                        class="p-3"
                    >
                        <b-spinner v-if="isLoading" class="mt-3" />
                        <div
                            v-if="!isLoading && clinicalDocuments.length === 0"
                            :data-testid="`clinical-document-no-records-${dependent.ownerId}`"
                        >
                            No records found.
                        </div>
                    </div>
                    <b-table-simple
                        v-if="!isLoading && clinicalDocuments.length > 0"
                        small
                        striped
                        borderless
                        :items="clinicalDocuments"
                        class="w-100 mb-0"
                        :aria-labelledby="`clinical-document-tab-title-${dependent.ownerId}`"
                        :data-testid="`clinical-document-table-${dependent.ownerId}`"
                    >
                        <b-thead>
                            <b-tr>
                                <b-th class="align-middle">Date</b-th>
                                <b-th class="align-middle">Title</b-th>
                                <b-th
                                    class="d-none d-md-table-cell align-middle"
                                >
                                    Document Type
                                </b-th>
                                <b-th
                                    class="d-none d-md-table-cell align-middle"
                                >
                                    Discipline
                                </b-th>
                                <b-th
                                    class="d-none d-md-table-cell align-middle"
                                >
                                    Facility Name
                                </b-th>
                                <b-th class="align-middle">Report</b-th>
                            </b-tr>
                        </b-thead>
                        <b-tbody>
                            <b-tr
                                v-for="(row, index) in clinicalDocuments"
                                :key="index"
                            >
                                <b-td
                                    :data-testid="`clinical-document-service-date-${dependent.ownerId}-${index}`"
                                    class="align-middle text-nowrap"
                                >
                                    {{ formatDate(row.serviceDate) }}
                                </b-td>
                                <b-td
                                    :data-testid="`clinical-document-name-${dependent.ownerId}-${index}`"
                                    class="align-middle"
                                >
                                    {{ row.name }}
                                </b-td>
                                <b-td
                                    :data-testid="`clinical-document-type-${dependent.ownerId}-${index}`"
                                    class="d-none d-md-table-cell align-middle"
                                >
                                    {{ row.type }}
                                </b-td>
                                <b-td
                                    :data-testid="`clinical-document-discipline-${dependent.ownerId}-${index}`"
                                    class="d-none d-md-table-cell align-middle"
                                >
                                    {{ row.discipline }}
                                </b-td>
                                <b-td
                                    :data-testid="`clinical-document-facility-name-${dependent.ownerId}-${index}`"
                                    class="d-none d-md-table-cell align-middle"
                                >
                                    {{ row.facilityName }}
                                </b-td>
                                <b-td class="align-middle">
                                    <hg-button
                                        :data-testid="`clinical-document-report-download-button-${dependent.ownerId}-${index}`"
                                        variant="link"
                                        class="p-0"
                                        @click="
                                            showClinicalDocumentDownloadConfirmationModal(
                                                row
                                            )
                                        "
                                    >
                                        <hg-icon
                                            icon="download"
                                            size="medium"
                                            square
                                            aria-hidden="true"
                                        />
                                    </hg-button>
                                </b-td>
                            </b-tr>
                        </b-tbody>
                    </b-table-simple>
                </b-tab>
                <template #tabs-start>
                    <div class="w-100">
                        <b-row>
                            <b-col>
                                <span
                                    class="card-title"
                                    data-testid="dependentName"
                                >
                                    {{
                                        dependent.dependentInformation.firstname
                                    }}
                                    {{
                                        dependent.dependentInformation.lastname
                                    }}
                                </span>
                            </b-col>
                            <ul class="list-unstyled">
                                <li
                                    role="presentation"
                                    class="ml-auto mr-1 nav-item align-self-center"
                                >
                                    <b-nav-item-dropdown
                                        right
                                        text=""
                                        :no-caret="true"
                                    >
                                        <template slot="button-content">
                                            <hg-icon
                                                icon="ellipsis-v"
                                                size="medium"
                                                data-testid="dependentMenuBtn"
                                                class="dependentMenu"
                                            />
                                        </template>
                                        <b-dropdown-item
                                            data-testid="deleteDependentMenuBtn"
                                            class="menuItem"
                                            @click="
                                                showDeleteConfirmationModal()
                                            "
                                        >
                                            Delete
                                        </b-dropdown-item>
                                    </b-nav-item-dropdown>
                                </li>
                            </ul>
                        </b-row>
                    </div>
                </template>
            </b-tabs>
        </b-card>
        <LoadingComponent
            :is-loading="isReportDownloading"
            :full-screen="false"
        />
        <LoadingComponent
            :is-loading="isVaccineRecordDownloading"
            :text="vaccineRecordState.statusMessage"
            :full-screen="false"
        />
        <delete-modal-component
            ref="deleteModal"
            title="Remove Dependent"
            confirm="Remove Dependent"
            message="Are you sure you want to remove this dependent?"
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

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

th {
    font-weight: bold;
}

td,
th {
    text-align: center;
}

.dependentMenu {
    color: $soft_text;
}

.card-title {
    padding-left: 14px;
    font-size: 1.2em;
}
</style>
