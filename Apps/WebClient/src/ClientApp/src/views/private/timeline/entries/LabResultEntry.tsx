import { useState } from "react";
import { View } from "react-native";

import {
    AppText,
    BulletList,
    BulletListItem,
    DisplayField,
    ExternalLink,
    HgButton,
    HgDataTable,
    InfoTooltip,
    MessageDialog,
    Strong,
} from "@/components/common";
import { EntryType, getEntryTypeDetails } from "@/constants/entryType";
import { ErrorSourceType } from "@/constants/errorType";
import { SENSITIVE_DOCUMENT_MESSAGE } from "@/constants/sensitiveDocument";
import {
    Action,
    Dataset,
    Format,
    Origin,
    Text,
    Type,
} from "@/constants/tracking";
import { isSessionEnded } from "@/models/errors";
import type { ReportField } from "@/models/report";
import type { LabResultTimelineEntry } from "@/models/timeline/labResultTimelineEntry";
import { services } from "@/services/container";
import { cn } from "@/utils/cn";
import { formatDate } from "@/utils/dateUtil";
import {
    downloadEncodedMedia,
    handleReportDownloadError,
} from "@/utils/encodedMedia";

import { TimelineEntryCard } from "../TimelineEntryCard";
import type { TimelineEntryProps } from "./entryProps";

const fields: ReportField[] = [
    { key: "testName", title: "Test Name" },
    { key: "result", title: "Result" },
    { key: "status", title: "Status" },
];

function Field(props: React.ComponentProps<typeof DisplayField>) {
    return (
        <View className="py-3">
            <DisplayField nameClass="font-bold" {...props} />
        </View>
    );
}

const tooltipTextClass = "text-tooltip-text text-body-medium leading-[1.6]";

function ResultInfo() {
    return (
        <>
            <AppText className={cn(tooltipTextClass, "mb-4")}>
                Follow the instructions from your health care provider. When
                needed, they can explain what your results mean.
            </AppText>
            <AppText className={cn(tooltipTextClass, "mb-2")}>
                Remember:
            </AppText>
            <BulletList textClassName={tooltipTextClass}>
                <BulletListItem>
                    <Strong>Ranges</Strong> are different between laboratories
                </BulletListItem>
                <BulletListItem>
                    <Strong>“Out of range”</Strong> results may be{" "}
                    <Strong>normal</Strong> for you
                </BulletListItem>
            </BulletList>
        </>
    );
}

/** Port of entry/LabResultTimelineComponent.vue */
export function LabResultEntry({
    entry: timelineEntry,
    commentsAreEnabled = false,
    hdid,
    isMobileDetails = false,
    testID,
}: TimelineEntryProps) {
    const entry = timelineEntry as LabResultTimelineEntry;
    const [isLoadingDocument, setIsLoadingDocument] = useState(false);
    const [confirmVisible, setConfirmVisible] = useState(false);

    const statusInfoId = (index: number) =>
        `laboratory-test-status-info-${entry.labPdfId}-${index}-${isMobileDetails ? "1" : "0"}`;

    async function getReport() {
        services.tracking.trackEvent({
            action: Action.Download,
            text: Text.DownloadLabResult,
            origin: Origin.Timeline,
            dataset: Dataset.LabResults,
            type: Type.LabResults,
            format: Format.Pdf,
        });
        setIsLoadingDocument(true);
        try {
            const result = await services.laboratory.getReportDocument(
                entry.id,
                hdid,
                false
            );
            const dateString = formatDate(
                entry.timelineDateTime,
                "yyyy_MM_dd-HH_mm"
            );
            await downloadEncodedMedia(
                result.resourcePayload,
                `Laboratory_Report_${dateString}.pdf`
            );
        } catch (error) {
            if (isSessionEnded(error)) return;
            handleReportDownloadError(error, ErrorSourceType.LaboratoryReport);
        } finally {
            setIsLoadingDocument(false);
        }
    }

    return (
        <>
            <TimelineEntryCard
                testID={testID}
                entry={entry}
                entryIcon={getEntryTypeDetails(EntryType.LabResult)?.icon}
                iconClass="bg-primary"
                title={entry.commonName}
                subtitle={`Order Status: ${entry.orderStatus}`}
                isMobileDetails={isMobileDetails}
                allowComment={commentsAreEnabled}
                hasAttachment={entry.reportAvailable}
                onAttachmentPress={() => setConfirmVisible(true)}
            >
                <View className="-my-3">
                    <Field
                        testID="laboratory-collection-date"
                        name="Collection Date"
                        value={
                            entry.collectionDateTime
                                ? formatDate(
                                      entry.collectionDateTime,
                                      "yyyy-MMM-dd, t"
                                  )
                                : undefined
                        }
                    />
                    <Field
                        testID="laboratory-ordering-provider"
                        name="Ordering Provider"
                        value={entry.orderingProvider}
                    />
                </View>
                <View className="py-3">
                    <DisplayField
                        testID="laboratory-reporting-lab"
                        nameClass="font-bold"
                        name="Reporting Lab"
                        value={entry.reportingLab}
                    />
                </View>
                <View
                    className="py-3 mb-2"
                    testID="reporting-lab-information-text"
                >
                    <AppText className="font-bold">
                        Find resources about your lab tests:
                    </AppText>
                </View>
                <BulletList className="mb-4">
                    <BulletListItem>
                        <ExternalLink href="https://www.healthlinkbc.ca/tests-treatments-medications/medical-tests">
                            HealthLink BC
                        </ExternalLink>
                    </BulletListItem>
                    <BulletListItem>
                        <ExternalLink href="https://www.mypathologyreport.ca/">
                            For pathology tests (like a biopsy)
                        </ExternalLink>
                    </BulletListItem>
                    <BulletListItem>
                        <ExternalLink href="https://smartsexresource.com/resources/">
                            BC CDC - Smart Sex Resource
                        </ExternalLink>
                    </BulletListItem>
                </BulletList>
                {entry.reportAvailable ? (
                    <HgButton
                        testID="laboratory-report-download-btn"
                        className="mt-2 mb-6"
                        text="Download report"
                        requiresConnection
                        icon="eye"
                        loading={isLoadingDocument}
                        onPress={() => setConfirmVisible(true)}
                    />
                ) : null}
                <HgDataTable
                    caption="Laboratory test results"
                    testID="laboratoryResultTable"
                    items={entry.tests.map((t) => ({ ...t }))}
                    fields={fields}
                    density="compact"
                    maxHeight={isMobileDetails ? undefined : 300}
                    renderHeader={{
                        result: () => (
                            <View className="flex-row items-center justify-center">
                                <AppText variant="body-medium">Result</AppText>
                                <View className="ml-2">
                                    <InfoTooltip
                                        accessibilityLabel="More information about the result"
                                        testID="result-info-button"
                                        tooltipTestID="result-info-popover"
                                        size={14}
                                    >
                                        <ResultInfo />
                                    </InfoTooltip>
                                </View>
                            </View>
                        ),
                    }}
                    renderItem={{
                        status: (item, index) => (
                            <View className="flex-row items-center justify-center">
                                <AppText className="text-body-small">
                                    {item.status}
                                </AppText>
                                <View className="ml-2">
                                    <InfoTooltip
                                        accessibilityLabel="More information about the test status"
                                        testID="laboratory-test-status-info-button"
                                        nativeID={statusInfoId(index)}
                                        tooltipTestID={`${statusInfoId(index)}-popover`}
                                    >
                                        {item.statusInfo.map((paragraph, i) => (
                                            <AppText
                                                key={i}
                                                className={tooltipTextClass}
                                            >
                                                {paragraph}
                                            </AppText>
                                        ))}
                                    </InfoTooltip>
                                </View>
                            </View>
                        ),
                    }}
                />
            </TimelineEntryCard>
            <MessageDialog
                visible={confirmVisible}
                title="Sensitive Document"
                message={SENSITIVE_DOCUMENT_MESSAGE}
                onClose={() => setConfirmVisible(false)}
                onSubmit={() => void getReport()}
            />
        </>
    );
}
