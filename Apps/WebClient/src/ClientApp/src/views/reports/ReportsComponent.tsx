import { useLocalSearchParams } from "expo-router";
import { useMemo, useRef, useState } from "react";
import { Image, Platform, View } from "react-native";

import {
    AppText,
    HgAlert,
    HgButton,
    HgChip,
    HgDatePicker,
    HgMenu,
    HgSelect,
    Loading,
    MessageDialog,
} from "@/components/ui";
import { EntryType, getEntryTypeDetails } from "@/constants/entryType";
import { ErrorSourceType, ErrorType, ServiceCode } from "@/constants/errorType";
import { SENSITIVE_DOCUMENT_MESSAGE } from "@/constants/sensitiveDocument";
import { Action, Actor, Text } from "@/constants/tracking";
import { useBreakpoint } from "@/hooks/useBreakpoint";
import { isSessionEnded, ResultError } from "@/models/errors";
import { ReportFormatType, reportMimeTypeMap } from "@/models/report";
import { type ReportFilter, ReportFilterBuilder } from "@/models/reportFilter";
import { services } from "@/services/container";
import { describeError, logger } from "@/services/logger";
import { useErrorStore } from "@/stores/error.store";
import {
    selectLabResultsAreQueued,
    useLabResultStore,
} from "@/stores/labResult.store";
import {
    selectMedications,
    useMedicationStore,
} from "@/stores/medication.store";
import { getReportHeaderData } from "@/stores/report";
import { useUserStore } from "@/stores/user.store";
import { ConfigUtil } from "@/utils/configUtil";
import { formatDate, fromIsoDate } from "@/utils/dateUtil";
import { getDataset, getFormat } from "@/utils/eventData";
import { saveBase64File } from "@/utils/saveFile";

import {
    Covid19TestResultReport,
    type DatasetReportProps,
    HealthVisitReport,
    HospitalVisitReport,
    LabResultReport,
    MedicationReport,
    NoteReport,
    type ReportHandle,
    SpecialAuthorityRequestReport,
} from "./DatasetReports";
import { ImmunizationReport } from "./ImmunizationReport";

const ReportsImage = require("@/assets/images/reports/reports.png") as number;

/** Vuetify's slide transitions take 0.3s. */
const PANEL_TRANSITION_MS = 300;

const reportEntryTypes = [
    EntryType.Covid19TestResult,
    EntryType.HealthVisit,
    EntryType.HospitalVisit,
    EntryType.Immunization,
    EntryType.LabResult,
    EntryType.Medication,
    EntryType.Note,
    EntryType.SpecialAuthorityRequest,
];

/** reportComponentMap: the report for a record type */
function renderReport(entryType: EntryType, props: DatasetReportProps) {
    switch (entryType) {
        case EntryType.Covid19TestResult:
            return <Covid19TestResultReport {...props} />;
        case EntryType.HealthVisit:
            return <HealthVisitReport {...props} />;
        case EntryType.HospitalVisit:
            return <HospitalVisitReport {...props} />;
        case EntryType.Immunization:
            return <ImmunizationReport {...props} />;
        case EntryType.LabResult:
            return <LabResultReport {...props} />;
        case EntryType.Medication:
            return <MedicationReport {...props} />;
        case EntryType.Note:
            return <NoteReport {...props} />;
        case EntryType.SpecialAuthorityRequest:
            return <SpecialAuthorityRequestReport {...props} />;
        default:
            return null;
    }
}

const reportErrorSources = new Map<EntryType, ErrorSourceType>([
    [EntryType.Covid19TestResult, ErrorSourceType.Covid19LaboratoryReport],
    [EntryType.HealthVisit, ErrorSourceType.HealthVisitReport],
    [EntryType.HospitalVisit, ErrorSourceType.HospitalVisitReport],
    [EntryType.Immunization, ErrorSourceType.ImmunizationReport],
    [EntryType.LabResult, ErrorSourceType.LaboratoryReport],
    [EntryType.Medication, ErrorSourceType.MedicationReport],
    [EntryType.Note, ErrorSourceType.NoteReport],
    [
        EntryType.SpecialAuthorityRequest,
        ErrorSourceType.SpecialAuthorityRequestReport,
    ],
]);

/** estimateBase64SizeInBytes: the decoded size, to refuse a huge PDF before converting it */
export function estimateBase64SizeInBytes(base64: string): number {
    let padding = 0;
    if (base64.endsWith("==")) padding = 2;
    else if (base64.endsWith("=")) padding = 1;
    return Math.floor((base64.length * 3) / 4) - padding;
}

const replaceSpaceWithDash = (source: string) => source.replaceAll(" ", "-");

interface Props {
    hdid: string;
    isDependent?: boolean;
}

/** Port of ReportsComponent.vue */
export function ReportsComponent({ hdid, isDependent = false }: Props) {
    const { defaultEntryType } = useLocalSearchParams<{
        defaultEntryType?: string;
    }>();
    const { xs } = useBreakpoint();
    const report = useRef<ReportHandle>(null);
    const [isLoading, setIsLoading] = useState(false);
    const [isGeneratingReport, setIsGeneratingReport] = useState(false);
    const [reportFormatType, setReportFormatType] = useState(
        ReportFormatType.PDF
    );
    const [selectedEntryType, setSelectedEntryType] = useState<
        EntryType | undefined
    >(defaultEntryType as EntryType | undefined);
    const [selectedStartDate, setSelectedStartDate] = useState("");
    const [selectedEndDate, setSelectedEndDate] = useState("");
    const [selectedMedications, setSelectedMedications] = useState<string[]>(
        []
    );
    const [hasRecords, setHasRecords] = useState(false);
    const [reportFilter, setReportFilter] = useState<ReportFilter>(() =>
        ReportFilterBuilder.create().build()
    );
    const [startDateIsValid, setStartDateIsValid] = useState(true);
    const [endDateIsValid, setEndDateIsValid] = useState(true);
    const [isAdvancedOpen, setIsAdvancedOpen] = useState(false);
    // v-slide-y-transition: on web the panel stays shown while it fades out
    const [isAdvancedLeaving, setIsAdvancedLeaving] = useState(false);
    const [confirmVisible, setConfirmVisible] = useState(false);
    const [queuedAlertClosed, setQueuedAlertClosed] = useState(false);

    const labResultsAreQueued = useLabResultStore(
        selectLabResultsAreQueued(hdid)
    );
    const medications = useMedicationStore(selectMedications(hdid));
    const patientHdid = useUserStore((s) => s.patient?.hdid);

    const reportIsEnabled = isDependent
        ? ConfigUtil.isDependentDatasetEnabled
        : ConfigUtil.isDatasetEnabled;
    const reportTypeOptions = reportEntryTypes
        .filter((entryType) => reportIsEnabled(entryType))
        .map((entryType) => ({
            value: entryType,
            title: getEntryTypeDetails(entryType)?.name ?? "",
        }));

    const medicationOptions = useMemo(() => {
        const names = new Set(
            medications.map((m) => m.medicationSummary.brandName)
        );
        return [...names]
            .sort((a, b) => a.localeCompare(b))
            .map((name) => ({ title: name, value: name }));
    }, [medications]);

    const isMedicationReport = selectedEntryType === EntryType.Medication;
    const showLabResultsQueuedMessage =
        selectedEntryType === EntryType.LabResult &&
        labResultsAreQueued &&
        !queuedAlertClosed;
    const isDownloadDisabled =
        isLoading || !selectedEntryType || !patientHdid || !hasRecords;
    const start = reportFilter.startDate
        ? `From ${formatDate(fromIsoDate(reportFilter.startDate))}`
        : "";
    const end = reportFilter.endDate
        ? `Up To ${formatDate(fromIsoDate(reportFilter.endDate))}`
        : "";
    const reportFilterDateString = `${start} ${end}`;

    function buildFilter(
        startDate: string,
        endDate: string,
        excluded: string[]
    ) {
        setReportFilter(
            ReportFilterBuilder.create()
                .withStartDate(startDate || null)
                .withEndDate(endDate || null)
                .withMedications(excluded)
                .build()
        );
    }

    function closeAdvanced() {
        setIsAdvancedOpen(false);
        if (Platform.OS !== "web") return;
        setIsAdvancedLeaving(true);
        setTimeout(() => setIsAdvancedLeaving(false), PANEL_TRANSITION_MS);
    }

    function clearFilterDates() {
        setSelectedStartDate("");
        setSelectedEndDate("");
        buildFilter("", "", selectedMedications);
    }

    function clearFilterMedication(medicationName: string) {
        const remaining = selectedMedications.filter(
            (m) => m !== medicationName
        );
        setSelectedMedications(remaining);
        buildFilter(selectedStartDate, selectedEndDate, remaining);
    }

    function cancelFilterChanges() {
        setSelectedStartDate(reportFilter.startDate ?? "");
        setSelectedEndDate(reportFilter.endDate ?? "");
        setSelectedMedications(reportFilter.medications);
        closeAdvanced();
    }

    function applyFilterChanges() {
        buildFilter(selectedStartDate, selectedEndDate, selectedMedications);
        closeAdvanced();
    }

    function showConfirmationModal(type: ReportFormatType) {
        setReportFormatType(type);
        setConfirmVisible(true);
    }

    async function downloadReport() {
        if (!selectedEntryType || !report.current) return;
        const reportName =
            getEntryTypeDetails(selectedEntryType)?.name ?? "selected";
        setIsGeneratingReport(true);
        services.tracking.trackEvent({
            action: Action.Download,
            text: Text.Export,
            dataset: getDataset(selectedEntryType),
            format: getFormat(reportFormatType),
            actor: isDependent ? Actor.Guardian : Actor.User,
        });
        try {
            const result = await report.current.generateReport(
                reportFormatType,
                getReportHeaderData(hdid, reportFilter)
            );
            // Prefer the backend error: it holds the report service's own failure message.
            if (result.resultError)
                throw ResultError.fromModel(result.resultError);
            if (!result.resourcePayload?.data)
                throw new ResultError(
                    ServiceCode.Report,
                    `Unable to download the ${reportName} report.`,
                    500
                );
            const base64Data = result.resourcePayload.data;
            // Large base64 payloads can exhaust memory during conversion: block PDFs over ~25 MB.
            if (
                reportFormatType === ReportFormatType.PDF &&
                estimateBase64SizeInBytes(base64Data) > 25 * 1024 * 1024
            )
                throw new ResultError(
                    ServiceCode.Report,
                    `The ${reportName} PDF report is too large to generate. Please reduce the date range or apply additional filters and try again.`,
                    413
                );
            await saveBase64File(
                base64Data,
                reportMimeTypeMap.get(reportFormatType) ?? "",
                result.resourcePayload.fileName
            );
        } catch (error) {
            if (isSessionEnded(error)) return;
            const resultError =
                error instanceof ResultError ? error : undefined;
            logger.error(describeError(error));
            if (resultError?.statusCode === 429)
                useErrorStore.getState().setTooManyRequestsError("page");
            else
                useErrorStore
                    .getState()
                    .addError(
                        ErrorType.Download,
                        reportErrorSources.get(selectedEntryType) ??
                            ErrorSourceType.ExportRecords,
                        resultError?.traceId ?? ""
                    );
        } finally {
            setIsGeneratingReport(false);
        }
    }

    return (
        <View>
            {showLabResultsQueuedMessage ? (
                <HgAlert
                    type="info"
                    closable
                    onClose={() => setQueuedAlertClosed(true)}
                    testID="laboratory-orders-queued-alert-message"
                    text="We are getting your lab results. It may take up to 48 hours until you can see them."
                />
            ) : null}
            <View className="mb-4">
                <View
                    className={
                        xs ? "gap-3" : "flex-row items-center justify-between"
                    }
                >
                    <HgSelect
                        nativeID="reportType"
                        testID="report-type"
                        className={xs ? "" : "flex-1 mr-4"}
                        label="Record Type"
                        hint="Select a record type"
                        items={reportTypeOptions}
                        value={selectedEntryType}
                        disabled={isLoading}
                        onChange={(value) =>
                            setSelectedEntryType(value as EntryType)
                        }
                    />
                    <View className="flex-row items-center">
                        <HgButton
                            nativeID="advanceFilterBtn"
                            testID="advanced-btn"
                            variant="link"
                            className="mr-2 px-4"
                            text="Advanced"
                            onPress={() =>
                                isAdvancedOpen
                                    ? closeAdvanced()
                                    : setIsAdvancedOpen(true)
                            }
                        />
                        <HgMenu
                            testID="export-record-menu"
                            items={[
                                ReportFormatType.PDF,
                                ReportFormatType.CSV,
                                ReportFormatType.XLSX,
                            ].map((type) => ({
                                title: type,
                                onPress: () => showConfirmationModal(type),
                            }))}
                        >
                            {(open) => (
                                <HgButton
                                    nativeID="export-record-btn"
                                    testID="export-record-btn"
                                    text="Download"
                                    disabled={isDownloadDisabled}
                                    loading={isGeneratingReport}
                                    onPress={open}
                                />
                            )}
                        </HgMenu>
                    </View>
                </View>
                {reportFilter.hasActiveFilter() ? (
                    <View className="flex-row flex-wrap gap-6 mt-4">
                        {reportFilter.hasDateFilter() ? (
                            <View>
                                <AppText className="font-bold mb-1">
                                    Date Range
                                </AppText>
                                <HgChip
                                    testID="clear-filter"
                                    onClose={clearFilterDates}
                                >
                                    <AppText
                                        variant="body-medium"
                                        testID="selected-dates-filter"
                                    >
                                        {reportFilterDateString}
                                    </AppText>
                                </HgChip>
                            </View>
                        ) : null}
                        {reportFilter.hasMedicationsFilter() &&
                        isMedicationReport ? (
                            <View className="shrink">
                                <AppText className="font-bold mb-1">
                                    Exclude
                                </AppText>
                                <View className="flex-row flex-wrap gap-1">
                                    {reportFilter.medications.map((item) => (
                                        <HgChip
                                            key={item}
                                            testID={`${replaceSpaceWithDash(item)}-clear-filter`}
                                            onClose={() =>
                                                clearFilterMedication(item)
                                            }
                                        >
                                            <AppText
                                                variant="body-medium"
                                                testID={`${replaceSpaceWithDash(item)}-excluded`}
                                            >
                                                {item}
                                            </AppText>
                                        </HgChip>
                                    ))}
                                </View>
                            </View>
                        ) : null}
                    </View>
                ) : null}
                {/* v-show: the fields stay mounted and keep what was typed */}
                <View
                    className="pt-4"
                    style={[
                        { opacity: isAdvancedOpen ? 1 : 0 },
                        Platform.OS === "web"
                            ? {
                                  transitionProperty: "opacity",
                                  transitionDuration: `${PANEL_TRANSITION_MS}ms`,
                              }
                            : null,
                        isAdvancedOpen || isAdvancedLeaving
                            ? null
                            : { display: "none" },
                    ]}
                >
                    <HgDatePicker
                        nativeID="start-date"
                        testID="start-date-input"
                        label="From"
                        hideDetails
                        value={selectedStartDate}
                        onChange={setSelectedStartDate}
                        onValidityChange={setStartDateIsValid}
                    />
                    <HgDatePicker
                        nativeID="end-date"
                        testID="end-date-input"
                        label="To"
                        hideDetails
                        value={selectedEndDate}
                        onChange={setSelectedEndDate}
                        onValidityChange={setEndDateIsValid}
                    />
                    {isMedicationReport ? (
                        <View className="mt-2">
                            <AppText
                                variant="title-large"
                                className="font-bold mb-2"
                            >
                                Exclude These Records
                            </AppText>
                            <AppText className="mb-2">Medications:</AppText>
                            <HgSelect
                                multiple
                                testID="medication-exclusion-filter"
                                hint="Choose a medication"
                                items={medicationOptions}
                                value={selectedMedications}
                                disabled={isLoading}
                                onChange={setSelectedMedications}
                            />
                        </View>
                    ) : null}
                    <View className="flex-row justify-end mt-4">
                        <HgButton
                            testID="clear-btn"
                            variant="secondary"
                            className="mr-2"
                            text="Cancel"
                            disabled={isLoading}
                            onPress={cancelFilterChanges}
                        />
                        <HgButton
                            testID="apply-filter-btn"
                            text="Apply"
                            disabled={
                                isLoading ||
                                !startDateIsValid ||
                                !endDateIsValid
                            }
                            onPress={applyFilterChanges}
                        />
                    </View>
                </View>
            </View>
            <Loading isLoading={isGeneratingReport} />
            {selectedEntryType ? (
                <View testID="report-sample">
                    {renderReport(selectedEntryType, {
                        ref: report,
                        hdid,
                        filter: reportFilter,
                        isDependent,
                        onIsLoadingChanged: setIsLoading,
                        onIsEmptyChanged: (isEmpty: boolean) =>
                            setHasRecords(!isEmpty),
                    })}
                </View>
            ) : (
                <View className="mb-4 items-center">
                    <Image
                        testID="info-image"
                        source={ReportsImage}
                        // decorative: the text under it says it all (the web client's alt is "...")
                        accessible={false}
                        importantForAccessibility="no"
                        style={{ width: 200, height: 212 }}
                        resizeMode="contain"
                    />
                    <AppText
                        testID="info-text"
                        className="text-headline-small tracking-normal text-center"
                    >
                        Select a record type above to create a report
                    </AppText>
                </View>
            )}
            <MessageDialog
                visible={confirmVisible}
                title="Sensitive Document"
                message={SENSITIVE_DOCUMENT_MESSAGE}
                onClose={() => setConfirmVisible(false)}
                onSubmit={() => void downloadReport()}
            />
        </View>
    );
}
