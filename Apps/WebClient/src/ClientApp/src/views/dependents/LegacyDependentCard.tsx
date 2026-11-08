import { useMemo, useState } from "react";
import { Pressable, View } from "react-native";

import {
    AppText,
    ExternalLink,
    HgAlert,
    HgButton,
    HgDataTable,
    HgMenu,
    type HgTab,
    HgTabPanel,
    HgTabs,
    HgWindow,
    Icon,
    Loading,
    MessageDialog,
} from "@/components/ui";
import { elevation1 } from "@/components/ui/elevation";
import { TableSkeleton } from "@/components/ui/TableSkeleton";
import { EntryType } from "@/constants/entryType";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { SENSITIVE_DOCUMENT_MESSAGE } from "@/constants/sensitiveDocument";
import {
    Action,
    Actor,
    Dataset,
    Destination,
    Format,
    Origin,
    Text,
    TrackedExternalUrl,
    Type,
} from "@/constants/tracking";
import { useBreakpoint } from "@/hooks/useBreakpoint";
import { useThemeColors } from "@/hooks/useTheme";
import type { ClinicalDocument } from "@/models/clinicalDocument";
import type { Dependent } from "@/models/dependent";
import { isSessionEnded, ResultError } from "@/models/errors";
import type { ImmunizationAgent } from "@/models/immunization";
import type { LaboratoryOrder } from "@/models/laboratory";
import {
    type ReportField,
    ReportFormatType,
    reportMimeTypeMap,
    TemplateType,
} from "@/models/report";
import { services } from "@/services/container";
import { describeError, logger } from "@/services/logger";
import {
    selectClinicalDocuments,
    selectClinicalDocumentsAreLoading,
    useClinicalDocumentStore,
} from "@/stores/clinicalDocument.store";
import { selectWebConfig, useConfigStore } from "@/stores/config.store";
import {
    selectIsLive,
    useConnectivityStore,
} from "@/stores/connectivity.store";
import { useDependentStore } from "@/stores/dependent.store";
import { useErrorStore } from "@/stores/error.store";
import {
    selectImmunizations,
    selectImmunizationsAreLoading,
    selectRecommendations,
    useImmunizationStore,
} from "@/stores/immunization.store";
import {
    selectLabResults,
    selectLabResultsAreLoading,
    useLabResultStore,
} from "@/stores/labResult.store";
import { useUserStore } from "@/stores/user.store";
import { cn } from "@/utils/cn";
import { ConfigUtil } from "@/utils/configUtil";
import {
    descending,
    formatDate,
    fromIso,
    fromIsoDate,
    now,
} from "@/utils/dateUtil";
import { getFormat } from "@/utils/eventData";
import { saveBase64File } from "@/utils/saveFile";

import {
    DependentCardTitle,
    DependentExpired,
    isDependentExpired,
    ReadOnlyField,
} from "./DependentCard";

const immunizationTabs: HgTab[] = [
    { value: 0, title: "History" },
    { value: 1, title: "Schedule" },
];

const tabIndices = {
    profile: 0,
    immunization: 1,
    labResults: 2,
    clinicalDocs: 3,
};

const agentNames = (agents: ImmunizationAgent[]) =>
    agents.map((x) => x.name).join(", ");
const agentProducts = (agents: ImmunizationAgent[]) =>
    agents.map((x) => x.productName).join(", ");
const agentLots = (agents: ImmunizationAgent[]) =>
    agents.map((x) => x.lotNumber).join(", ");

function DownloadMenu({
    tab,
    id,
    disabled,
    onPick,
}: {
    tab: "history" | "schedule";
    id: string;
    disabled: boolean;
    onPick: (type: ReportFormatType) => void;
}) {
    return (
        <HgMenu
            testID={`download-immunization-${tab}-report-menu-${id}`}
            items={[
                ReportFormatType.PDF,
                ReportFormatType.CSV,
                ReportFormatType.XLSX,
            ].map((type) => ({
                title: type,
                testID: `download-immunization-${tab}-report-${type.toLowerCase()}-btn-${id}`,
                onPress: () => onPick(type),
            }))}
        >
            {(open) => (
                <HgButton
                    nativeID={`download-immunization-${tab}-report-btn-${id}`}
                    testID={`download-immunization-${tab}-report-btn-${id}`}
                    variant="secondary"
                    appendIcon="caret-down"
                    text="Download"
                    requiresConnection
                    disabled={disabled}
                    onPress={open}
                />
            )}
        </HgMenu>
    );
}

function DownloadIconButton({
    testID,
    onPress,
}: {
    testID: string;
    onPress: () => void;
}) {
    const colors = useThemeColors();
    const isLive = useConnectivityStore(selectIsLive);
    return (
        <Pressable
            testID={testID}
            accessibilityRole="button"
            accessibilityLabel="Download"
            accessibilityState={{ disabled: !isLive }}
            disabled={!isLive}
            hitSlop={8}
            onPress={onPress}
            className={cn("self-center p-2", !isLive && "opacity-40")}
        >
            <Icon name="download" size={16} color={colors.primary} />
        </Pressable>
    );
}

/**
 * Port of LegacyDependentCardComponent.vue, the card users get while `dependents.timelineEnabled` is off: profile,
 * immunization (history and schedule), lab results and clinical documents tabs.
 */
export function LegacyDependentCard({
    dependent,
    onNeedsUpdate,
}: {
    dependent: Dependent;
    onNeedsUpdate: () => void;
}) {
    const id = dependent.ownerId;
    const info = dependent.dependentInformation;
    const { mdAndUp, lgAndUp } = useBreakpoint();
    const hdid = useUserStore((s) => s.user.hdid);
    const maxDependentAge = useConfigStore(
        (s) => selectWebConfig(s)?.maxDependentAge ?? 0
    );
    const removeDependent = useDependentStore((s) => s.removeDependent);

    const immunizations = useImmunizationStore(selectImmunizations(id));
    const recommendations = useImmunizationStore(selectRecommendations(id));
    const immunizationsAreLoading = useImmunizationStore(
        selectImmunizationsAreLoading(id)
    );
    const retrieveImmunizations = useImmunizationStore(
        (s) => s.retrieveImmunizations
    );
    const labResultRecords = useLabResultStore(selectLabResults(id));
    const labResultsAreLoading = useLabResultStore(
        selectLabResultsAreLoading(id)
    );
    const retrieveLabResults = useLabResultStore((s) => s.retrieveLabResults);
    const clinicalDocumentRecords = useClinicalDocumentStore(
        selectClinicalDocuments(id)
    );
    const clinicalDocumentsAreLoading = useClinicalDocumentStore(
        selectClinicalDocumentsAreLoading(id)
    );
    const retrieveClinicalDocuments = useClinicalDocumentStore(
        (s) => s.retrieve
    );

    const [selectedTabIndex, setSelectedTabIndex] = useState(0);
    const [immunizationTabIndex, setImmunizationTabIndex] = useState(0);
    const [reportFormatType, setReportFormatType] = useState(
        ReportFormatType.PDF
    );
    const [isReportDownloading, setIsReportDownloading] = useState(false);
    const [deleteVisible, setDeleteVisible] = useState(false);
    const [downloadVisible, setDownloadVisible] = useState(false);
    const [selectedLaboratoryOrder, setSelectedLaboratoryOrder] =
        useState<LaboratoryOrder>();
    const [selectedClinicalDocument, setSelectedClinicalDocument] =
        useState<ClinicalDocument>();

    const isExpired = isDependentExpired(dependent, maxDependentAge);
    const isImmunizationTabShown = ConfigUtil.isDependentDatasetEnabled(
        EntryType.Immunization
    );
    const isLaboratoryOrderTabShown = ConfigUtil.isDependentDatasetEnabled(
        EntryType.LabResult
    );
    const isClinicalDocumentTabShown = ConfigUtil.isDependentDatasetEnabled(
        EntryType.ClinicalDocument
    );

    const tabs: HgTab[] = [
        {
            value: tabIndices.profile,
            title: "Profile",
            testID: `profile-tab-title-${id}`,
        },
        ...(isImmunizationTabShown
            ? [
                  {
                      value: tabIndices.immunization,
                      title: "Immunization",
                      disabled: isExpired,
                      testID: `immunization-tab-title-${id}`,
                  },
              ]
            : []),
        ...(isLaboratoryOrderTabShown
            ? [
                  {
                      value: tabIndices.labResults,
                      title: "Lab Results",
                      disabled: isExpired,
                      testID: `lab-results-tab-title-${id}`,
                  },
              ]
            : []),
        ...(isClinicalDocumentTabShown
            ? [
                  {
                      value: tabIndices.clinicalDocs,
                      title: "Clinical Docs",
                      disabled: isExpired,
                      testID: `clinical-document-tab-title-${id}`,
                  },
              ]
            : []),
    ];

    /**
     * The web client fetches when a window becomes active (`watch(selectedTabIndex)`), not on a header click, so
     * pressing the tab that is already open does not fetch again.
     */
    function selectTab(tabIndex: number) {
        if (tabIndex === selectedTabIndex) return;
        setSelectedTabIndex(tabIndex);
        const logError = (error: unknown) => logger.error(describeError(error));
        switch (tabIndex) {
            case tabIndices.immunization:
                retrieveImmunizations(id).catch(logError);
                break;
            case tabIndices.labResults:
                retrieveLabResults(id).catch(logError);
                break;
            case tabIndices.clinicalDocs:
                retrieveClinicalDocuments(id).catch(logError);
                break;
        }
    }

    const immunizationItems = useMemo(
        () =>
            [...immunizations]
                .sort((a, b) =>
                    descending(
                        fromIsoDate(a.dateOfImmunization),
                        fromIsoDate(b.dateOfImmunization)
                    )
                )
                .map((x) => ({
                    date: formatDate(fromIsoDate(x.dateOfImmunization)),
                    immunization: x.immunization.name,
                    agent: agentNames(x.immunization.immunizationAgents),
                    product: agentProducts(x.immunization.immunizationAgents),
                    provider_clinic: x.providerOrClinic,
                    lotNumber: agentLots(x.immunization.immunizationAgents),
                })),
        [immunizations]
    );
    const recommendationItems = useMemo(
        () =>
            recommendations.map((x) => ({
                immunization: x.recommendedVaccinations,
                due_date: formatDate(fromIsoDate(x.agentDueDate)),
            })),
        [recommendations]
    );
    const labResults = useMemo(
        () =>
            [...labResultRecords].sort((a, b) =>
                descending(
                    fromIso(a.timelineDateTime),
                    fromIso(b.timelineDateTime)
                )
            ),
        [labResultRecords]
    );
    const clinicalDocuments = useMemo(
        () =>
            [...clinicalDocumentRecords].sort((a, b) =>
                descending(
                    fromIsoDate(a.serviceDate),
                    fromIsoDate(b.serviceDate)
                )
            ),
        [clinicalDocumentRecords]
    );

    const isDownloadImmunizationReportButtonDisabled =
        isReportDownloading ||
        selectedTabIndex !== tabIndices.immunization ||
        (immunizationItems.length === 0 && recommendationItems.length === 0);

    function deleteDependent() {
        services.tracking.trackEvent({
            action: Action.ButtonClick,
            text: Text.RemoveDependent,
            type: Type.Dependents,
        });
        removeDependent(hdid, dependent)
            .catch((error: unknown) => logger.error(describeError(error)))
            .finally(onNeedsUpdate);
    }

    async function downloadReportFile(
        getDownloadInfo: () => Promise<{
            data: string;
            mimeType: string;
            fileName: string;
        }>,
        errorSource: ErrorSourceType
    ) {
        setIsReportDownloading(true);
        try {
            const { data, mimeType, fileName } = await getDownloadInfo();
            await saveBase64File(data, mimeType, fileName);
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
                        errorSource,
                        resultError?.traceId
                    );
        } finally {
            setIsReportDownloading(false);
        }
    }

    function downloadLaboratoryOrderReport() {
        const order = selectedLaboratoryOrder;
        if (!order) return;
        services.tracking.trackEvent({
            action: Action.Download,
            text: Text.Document,
            dataset: Dataset.LabResults,
            format: Format.Pdf,
            actor: Actor.Guardian,
        });
        const dateString = formatDate(
            fromIso(order.timelineDateTime),
            "yyyy_MM_dd-HH_mm"
        );
        void downloadReportFile(async () => {
            const result = await services.laboratory.getReportDocument(
                order.reportId,
                id,
                false
            );
            const report = result.resourcePayload;
            return {
                data: report.data,
                mimeType: report.mediaType,
                fileName: `Laboratory_Report_${info.firstname}_${info.lastname}_${dateString}.pdf`,
            };
        }, ErrorSourceType.LaboratoryReport);
    }

    function downloadImmunizationReport() {
        services.tracking.trackEvent({
            action: Action.Download,
            text:
                immunizationTabIndex === 0
                    ? Text.DownloadDependentHistoricImmunizations
                    : Text.DownloadDependentRecommendedImmunizations,
            dataset: Dataset.Immunizations,
            type: Type.Dependents,
            format: getFormat(reportFormatType),
        });
        void downloadReportFile(async () => {
            const result = await services.report.generateReport({
                data: {
                    header: {
                        phn: info.PHN,
                        dateOfBirth: formatDate(fromIsoDate(info.dateOfBirth)),
                        name: `${info.firstname} ${info.lastname}`,
                        isRedacted: false,
                        datePrinted: formatDate(now()),
                        filterText: "",
                    },
                    records: immunizationItems,
                    recommendations: recommendationItems,
                },
                template: TemplateType.DependentImmunization,
                type: reportFormatType,
            });
            return {
                data: result.resourcePayload.data,
                mimeType: reportMimeTypeMap.get(reportFormatType) ?? "",
                fileName: result.resourcePayload.fileName,
            };
        }, ErrorSourceType.DependentImmunizationReport);
    }

    function downloadClinicalDocument() {
        const document = selectedClinicalDocument;
        if (!document) return;
        services.tracking.trackEvent({
            action: Action.Download,
            text: Text.Document,
            dataset: Dataset.ClinicalDocuments,
            format: Format.Pdf,
            actor: Actor.Guardian,
        });
        void downloadReportFile(async () => {
            const result = await services.clinicalDocument.getFile(
                document.fileId,
                id
            );
            return {
                data: result.resourcePayload.data,
                mimeType: result.resourcePayload.mediaType,
                fileName: `Clinical_Document_${info.firstname}_${info.lastname}.pdf`,
            };
        }, ErrorSourceType.ClinicalDocument);
    }

    function downloadDocument() {
        if (selectedTabIndex === tabIndices.immunization)
            downloadImmunizationReport();
        else if (selectedTabIndex === tabIndices.labResults)
            downloadLaboratoryOrderReport();
        else if (selectedTabIndex === tabIndices.clinicalDocs)
            downloadClinicalDocument();
    }

    const showImmunizationDownload = (type: ReportFormatType) => {
        setReportFormatType(type);
        setDownloadVisible(true);
    };

    const historyFields: ReportField[] = [
        {
            key: "date",
            title: "Date",
            tdAlign: "center",
            tdNoWrap: true,
            tdTestID: (i) => `history-immunization-date-${id}-${i}`,
        },
        {
            key: "immunization",
            title: "Immunization",
            tdAlign: "center",
            tdTestID: (i) => `history-product-${id}-${i}`,
        },
        ...(lgAndUp
            ? [
                  {
                      key: "agent",
                      title: "Agent",
                      tdAlign: "center",
                      tdTestID: (i: number) =>
                          `history-immunizing-agent-${id}-${i}`,
                  } as ReportField,
                  {
                      key: "product",
                      title: "Product",
                      tdAlign: "center",
                      tdTestID: (i: number) =>
                          `history-immunizing-product-${id}-${i}`,
                  } as ReportField,
              ]
            : []),
        {
            key: "provider_clinic",
            title: "Provider/Clinic",
            tdAlign: "center",
            tdTestID: (i) => `history-provider-clinic-${id}-${i}`,
        },
        ...(lgAndUp
            ? [
                  {
                      key: "lotNumber",
                      title: "Lot Number",
                      tdAlign: "center",
                      tdTestID: (i: number) => `history-lot-number-${id}-${i}`,
                  } as ReportField,
              ]
            : []),
    ];
    const scheduleFields: ReportField[] = [
        {
            key: "immunization",
            title: "Immunization",
            tdAlign: "center",
            tdTestID: (i) => `schedule-immunization-${id}-${i}`,
        },
        {
            key: "due_date",
            title: "Due Date",
            tdAlign: "center",
            tdNoWrap: true,
            tdTestID: (i) => `schedule-due-date-${id}-${i}`,
        },
    ];
    const labFields: ReportField[] = [
        {
            key: "date",
            title: "Date",
            tdAlign: "center",
            tdNoWrap: true,
            tdTestID: (i) => `lab-results-date-${id}-${i}`,
        },
        {
            key: "commonName",
            title: "Title",
            tdAlign: "center",
            tdTestID: (i) => `lab-results-title-${id}-${i}`,
        },
        ...(lgAndUp
            ? [
                  {
                      key: "reportingSource",
                      title: "Lab",
                      tdAlign: "center",
                      tdTestID: (i: number) => `lab-results-lab-${id}-${i}`,
                  } as ReportField,
              ]
            : []),
        ...(mdAndUp
            ? [
                  {
                      key: "orderStatus",
                      title: "Status",
                      tdAlign: "center",
                      tdTestID: (i: number) => `lab-results-status-${id}-${i}`,
                  } as ReportField,
              ]
            : []),
        { key: "report", title: "Detailed Report" },
    ];
    const documentFields: ReportField[] = [
        {
            key: "date",
            title: "Date",
            tdAlign: "center",
            tdNoWrap: true,
            tdTestID: (i) => `clinical-document-service-date-${id}-${i}`,
        },
        {
            key: "name",
            title: "Title",
            tdAlign: "center",
            tdTestID: (i) => `clinical-document-name-${id}-${i}`,
        },
        ...(mdAndUp
            ? [
                  {
                      key: "type",
                      title: "Document Type",
                      tdAlign: "center",
                      tdTestID: (i: number) =>
                          `clinical-document-type-${id}-${i}`,
                  } as ReportField,
                  {
                      key: "discipline",
                      title: "Type",
                      tdAlign: "center",
                      tdTestID: (i: number) =>
                          `clinical-document-discipline-${id}-${i}`,
                  } as ReportField,
                  {
                      key: "facilityName",
                      title: "Facility Name",
                      tdAlign: "center",
                      tdTestID: (i: number) =>
                          `clinical-document-facility-name-${id}-${i}`,
                  } as ReportField,
              ]
            : []),
        { key: "report", title: "Report" },
    ];

    return (
        <View
            testID={`dependent-card-${info.PHN}`}
            style={elevation1}
            className="mt-4 rounded bg-surface"
        >
            <Loading isLoading={isReportDownloading} />
            <View className="bg-card-header rounded-t px-4 pt-4">
                <DependentCardTitle
                    name={`${info.firstname} ${info.lastname}`}
                    onDelete={() => setDeleteVisible(true)}
                />
                <HgTabs
                    selectedSurface
                    value={selectedTabIndex}
                    onChange={selectTab}
                    tabs={tabs}
                />
            </View>
            <HgWindow
                tabs={tabs}
                value={selectedTabIndex}
                onChange={selectTab}
                testID={`dependent-window-${id}`}
                className="p-4"
            >
                <HgTabPanel
                    active={selectedTabIndex === tabIndices.profile}
                    testID="dependentTab"
                >
                    {isExpired ? (
                        <DependentExpired
                            maxDependentAge={maxDependentAge}
                            onRemove={deleteDependent}
                        />
                    ) : (
                        <View className="flex-row flex-wrap -mx-2">
                            <ReadOnlyField
                                label="PHN"
                                nativeID={`dependent-phn-${id}`}
                                testID="dependentPHN"
                                value={info.PHN}
                            />
                            <ReadOnlyField
                                label="Date of Birth"
                                nativeID={`dependent-dob-${id}`}
                                testID="dependentDOB"
                                value={formatDate(
                                    fromIsoDate(info.dateOfBirth)
                                )}
                            />
                        </View>
                    )}
                </HgTabPanel>
                {isImmunizationTabShown && !isExpired ? (
                    <HgTabPanel
                        active={selectedTabIndex === tabIndices.immunization}
                        testID={`immunization-tab-${id}`}
                    >
                        {immunizationItems.length !== 0 ? (
                            <HgAlert
                                type="info"
                                testID="dependent-immunization-disclaimer-alert"
                            >
                                <AppText className="text-info-text">
                                    If your dependent’s immunizations are
                                    missing or incorrect,{" "}
                                    <ExternalLink
                                        href={
                                            TrackedExternalUrl.ImmunizationRecordBC
                                        }
                                        onPress={() =>
                                            services.tracking.trackEvent({
                                                action: Action.ExternalLink,
                                                text: Text.ImmunizationUpdateForm,
                                                origin: Origin.Dependents,
                                                destination:
                                                    Destination.ImmunizationRecordBC,
                                                type: Type.Dependents,
                                                url: TrackedExternalUrl.ImmunizationRecordBC,
                                            })
                                        }
                                    >
                                        fill in this online form
                                    </ExternalLink>
                                    .
                                </AppText>
                            </HgAlert>
                        ) : null}
                        <View testID={`immunization-tab-div-${id}`}>
                            <HgTabs
                                value={immunizationTabIndex}
                                onChange={setImmunizationTabIndex}
                                tabs={immunizationTabs}
                            />
                            {immunizationsAreLoading ? (
                                <TableSkeleton />
                            ) : (
                                <HgWindow
                                    tabs={immunizationTabs}
                                    value={immunizationTabIndex}
                                    onChange={setImmunizationTabIndex}
                                    testID={`immunization-window-${id}`}
                                    className="p-4"
                                >
                                    <HgTabPanel
                                        active={immunizationTabIndex === 0}
                                    >
                                        {immunizationItems.length === 0 ? (
                                            <AppText
                                                testID={`immunization-history-no-rows-found-${id}`}
                                            >
                                                No records found. If this is
                                                your first time checking for
                                                records, please try refreshing
                                                the page in a few minutes.
                                            </AppText>
                                        ) : (
                                            <View>
                                                <View className="items-end mb-4">
                                                    <DownloadMenu
                                                        tab="history"
                                                        id={id}
                                                        disabled={
                                                            isDownloadImmunizationReportButtonDisabled
                                                        }
                                                        onPick={
                                                            showImmunizationDownload
                                                        }
                                                    />
                                                </View>
                                                <HgDataTable
                                                    caption="Immunization history"
                                                    testID={`immunization-history-table-${id}`}
                                                    items={immunizationItems}
                                                    fields={historyFields}
                                                />
                                            </View>
                                        )}
                                    </HgTabPanel>
                                    <HgTabPanel
                                        active={immunizationTabIndex === 1}
                                    >
                                        <View
                                            className={
                                                mdAndUp
                                                    ? "flex-row items-start"
                                                    : ""
                                            }
                                        >
                                            <AppText
                                                className={
                                                    mdAndUp ? "flex-1" : "mb-4"
                                                }
                                            >
                                                School-aged children are offered
                                                most immunizations in their
                                                school, particularly in grades 6
                                                and 9. The school can let you
                                                know which vaccines are offered.
                                                You need to book an appointment
                                                to get your child vaccinated
                                                against COVID
                                                {"‑"}19.{" "}
                                                <ExternalLink href="https://www2.gov.bc.ca/gov/content/covid-19/vaccine">
                                                    Find out how.
                                                </ExternalLink>
                                            </AppText>
                                            {recommendationItems.length > 0 ? (
                                                <View
                                                    className={
                                                        mdAndUp
                                                            ? "pl-4"
                                                            : "items-end"
                                                    }
                                                >
                                                    <DownloadMenu
                                                        tab="schedule"
                                                        id={id}
                                                        disabled={
                                                            isDownloadImmunizationReportButtonDisabled
                                                        }
                                                        onPick={
                                                            showImmunizationDownload
                                                        }
                                                    />
                                                </View>
                                            ) : null}
                                        </View>
                                        {recommendationItems.length === 0 ? (
                                            <AppText
                                                testID={`immunization-schedule-no-rows-found-${id}`}
                                                className="my-4"
                                            >
                                                No records found.
                                            </AppText>
                                        ) : (
                                            <HgDataTable
                                                caption="Immunization schedule"
                                                testID={`immunization-schedule-table-${id}`}
                                                className="mt-4"
                                                items={recommendationItems}
                                                fields={scheduleFields}
                                            />
                                        )}
                                    </HgTabPanel>
                                </HgWindow>
                            )}
                        </View>
                    </HgTabPanel>
                ) : null}
                {isLaboratoryOrderTabShown && !isExpired ? (
                    <HgTabPanel
                        active={selectedTabIndex === tabIndices.labResults}
                        testID={`lab-results-tab-${id}`}
                    >
                        {labResultsAreLoading ? (
                            <TableSkeleton />
                        ) : labResults.length === 0 ? (
                            <AppText testID={`lab-results-no-records-${id}`}>
                                No records found. If you just added your
                                dependent, it can take up to 24 hours to get
                                their records.
                            </AppText>
                        ) : (
                            <HgDataTable
                                caption="Lab results"
                                testID={`lab-results-table-${id}`}
                                items={labResults.map((row) => ({
                                    ...row,
                                    date: formatDate(
                                        fromIso(row.timelineDateTime)
                                    ),
                                }))}
                                fields={labFields}
                                renderItem={{
                                    report: (row, index) =>
                                        row.reportAvailable ? (
                                            <DownloadIconButton
                                                testID={`lab-results-report-download-button-${id}-${index}`}
                                                onPress={() => {
                                                    setSelectedLaboratoryOrder(
                                                        row
                                                    );
                                                    setDownloadVisible(true);
                                                }}
                                            />
                                        ) : null,
                                }}
                            />
                        )}
                    </HgTabPanel>
                ) : null}
                {isClinicalDocumentTabShown && !isExpired ? (
                    <HgTabPanel
                        active={selectedTabIndex === tabIndices.clinicalDocs}
                        testID={`clinical-document-tab-${id}`}
                    >
                        {clinicalDocumentsAreLoading ? (
                            <TableSkeleton />
                        ) : clinicalDocuments.length === 0 ? (
                            <AppText
                                testID={`clinical-document-no-records-${id}`}
                            >
                                No records found.
                            </AppText>
                        ) : (
                            <HgDataTable
                                caption="Clinical documents"
                                testID={`clinical-document-table-${id}`}
                                items={clinicalDocuments.map((row) => ({
                                    ...row,
                                    date: formatDate(
                                        fromIsoDate(row.serviceDate)
                                    ),
                                }))}
                                fields={documentFields}
                                renderItem={{
                                    report: (row, index) => (
                                        <DownloadIconButton
                                            testID={`clinical-document-report-download-button-${id}-${index}`}
                                            onPress={() => {
                                                setSelectedClinicalDocument(
                                                    row
                                                );
                                                setDownloadVisible(true);
                                            }}
                                        />
                                    ),
                                }}
                            />
                        )}
                    </HgTabPanel>
                ) : null}
            </HgWindow>
            <MessageDialog
                visible={deleteVisible}
                title="Remove Dependent"
                message="Are you sure you want to remove this dependent?"
                submitText="Remove Dependent"
                onClose={() => setDeleteVisible(false)}
                onSubmit={deleteDependent}
            />
            <MessageDialog
                visible={downloadVisible}
                title="Sensitive Document"
                message={SENSITIVE_DOCUMENT_MESSAGE}
                onClose={() => setDownloadVisible(false)}
                onSubmit={downloadDocument}
            />
        </View>
    );
}
