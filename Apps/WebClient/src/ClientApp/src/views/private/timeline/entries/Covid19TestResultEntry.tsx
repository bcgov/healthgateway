import { useState } from "react";
import { View } from "react-native";

import {
    AppText,
    DisplayField,
    ExternalLink,
    HgButton,
    MessageDialog,
} from "@/components/common";
import { EntryType, getEntryTypeDetails } from "@/constants/entryType";
import { ErrorSourceType } from "@/constants/errorType";
import { SENSITIVE_DOCUMENT_MESSAGE } from "@/constants/sensitiveDocument";
import { Action, Actor, Dataset, Format, Text } from "@/constants/tracking";
import { isSessionEnded } from "@/models/errors";
import type { Covid19TestResultTimelineEntry } from "@/models/timeline/covid19TestResultTimelineEntry";
import { services } from "@/services/container";
import { cn } from "@/utils/cn";
import { formatDate } from "@/utils/dateUtil";
import {
    downloadEncodedMedia,
    handleReportDownloadError,
} from "@/utils/encodedMedia";

import { TimelineEntryCard } from "../TimelineEntryCard";
import type { TimelineEntryProps } from "./entryProps";

function outcomeClasses(outcome: string): string {
    switch (outcome?.toUpperCase()) {
        case "NEGATIVE":
            return "text-success font-bold";
        case "POSITIVE":
            return "text-error font-bold";
        default:
            return "font-bold";
    }
}

function Field(props: React.ComponentProps<typeof DisplayField>) {
    return (
        <View className="py-3">
            <DisplayField nameClass="font-bold" {...props} />
        </View>
    );
}

/** Port of Covid19TestResultDescriptionComponent.vue: paragraphs, the last one ending with "this page." */
function ResultDescription({
    description,
    link,
}: {
    description: string[];
    link: string;
}) {
    return (
        <View>
            {description.map((paragraph, index) => (
                <AppText
                    key={index}
                    className={index + 1 < description.length ? "mb-4" : ""}
                >
                    {paragraph}
                    {index + 1 === description.length && link ? (
                        <>
                            {" "}
                            <ExternalLink testID="result-link" href={link}>
                                this page
                            </ExternalLink>
                            .
                        </>
                    ) : null}
                </AppText>
            ))}
        </View>
    );
}

/** Port of entry/Covid19TestResultTimelineComponent.vue */
export function Covid19TestResultEntry({
    entry: timelineEntry,
    commentsAreEnabled = false,
    hdid,
    isMobileDetails = false,
    testID,
}: TimelineEntryProps) {
    const entry = timelineEntry as Covid19TestResultTimelineEntry;
    const [isLoadingDocument, setIsLoadingDocument] = useState(false);
    const [confirmVisible, setConfirmVisible] = useState(false);
    const canDownload = entry.reportAvailable && entry.resultReady;

    async function getReport() {
        services.tracking.trackEvent({
            action: Action.Download,
            text: Text.Document,
            dataset: Dataset.Covid19Tests,
            format: Format.Pdf,
            actor: Actor.User,
        });
        setIsLoadingDocument(true);
        try {
            const result = await services.laboratory.getReportDocument(
                entry.id,
                hdid,
                true
            );
            const dateString = formatDate(
                entry.displayDate,
                "yyyy_MM_dd-HH_mm"
            );
            await downloadEncodedMedia(
                result.resourcePayload,
                `COVID_Result_${dateString}.pdf`
            );
        } catch (error) {
            if (isSessionEnded(error)) return;
            handleReportDownloadError(
                error,
                ErrorSourceType.Covid19LaboratoryReport
            );
        } finally {
            setIsLoadingDocument(false);
        }
    }

    return (
        <>
            <TimelineEntryCard
                testID={testID}
                entry={entry}
                entryIcon={
                    getEntryTypeDetails(EntryType.Covid19TestResult)?.icon
                }
                iconClass="bg-primary"
                title={entry.summaryTitle}
                isMobileDetails={isMobileDetails}
                allowComment={commentsAreEnabled}
                hasAttachment={canDownload}
                onAttachmentPress={() => setConfirmVisible(true)}
                headerDescription={
                    entry.resultReady && entry.tests.length === 1 ? (
                        <AppText testID="laboratoryHeaderDescription">
                            <AppText className="font-bold">Result: </AppText>
                            <AppText
                                className={outcomeClasses(
                                    entry.labResultOutcome
                                )}
                            >
                                {entry.labResultOutcome}
                            </AppText>
                        </AppText>
                    ) : undefined
                }
            >
                {canDownload ? (
                    <HgButton
                        testID="covid-result-download-btn"
                        className="mt-2 mb-6"
                        variant="secondary"
                        text="Download Full Report"
                        requiresConnection
                        icon="download"
                        loading={isLoadingDocument}
                        onPress={() => setConfirmVisible(true)}
                    />
                ) : null}
                <View className="py-3">
                    <DisplayField
                        testID="laboratoryReportingLab"
                        nameClass="font-bold"
                        name="Reporting Lab"
                        value={entry.reportingLab}
                    />
                </View>
                {entry.tests.map((test, testIndex) => (
                    <View key={test.id}>
                        {entry.tests.length > 1 ? (
                            <View className="h-px bg-border-divider my-4" />
                        ) : null}
                        {test.resultReady && entry.tests.length > 1 ? (
                            <View className="py-3">
                                <DisplayField
                                    testID="laboratoryTestResult"
                                    nameClass="font-bold"
                                    name="Result"
                                    value={test.labResultOutcome}
                                    valueClass={outcomeClasses(
                                        test.labResultOutcome
                                    )}
                                />
                            </View>
                        ) : null}
                        <View className="-my-3 py-3">
                            <Field
                                testID="laboratoryTestType"
                                name="Test Type"
                                value={test.testType}
                            />
                            <Field
                                testID={`laboratoryTestStatus-${testIndex}`}
                                name="Test Status"
                                value={test.testStatus}
                            />
                            <Field
                                name="Collection Date"
                                value={formatDate(
                                    test.collectedDateTime,
                                    "yyyy-MMM-dd, t"
                                )}
                            />
                            <Field
                                name="Result Date"
                                value={formatDate(
                                    test.resultDateTime,
                                    "yyyy-MMM-dd, t"
                                )}
                            />
                        </View>
                        {test.resultDescription.length > 0 ? (
                            <View
                                className="py-3"
                                testID={`laboratoryResultDescription-${testIndex}`}
                            >
                                <AppText className={cn("font-bold")}>
                                    Result Description:{" "}
                                </AppText>
                                <ResultDescription
                                    description={test.resultDescription}
                                    link={test.resultLink}
                                />
                            </View>
                        ) : null}
                    </View>
                ))}
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
