import { useState } from "react";
import { View } from "react-native";

import {
    AppText,
    ExternalLink,
    HgButton,
    MessageDialog,
} from "@/components/common";
import { EntryType, getEntryTypeDetails } from "@/constants/entryType";
import { SENSITIVE_DOCUMENT_MESSAGE } from "@/constants/sensitiveDocument";
import { Action, Dataset, Format, Origin, Type } from "@/constants/tracking";
import type { BcCancerScreeningTimelineEntry } from "@/models/timeline/bcCancerScreeningTimelineEntry";
import { services } from "@/services/container";
import { describeError, logger } from "@/services/logger";
import {
    selectPatientDataFileLoading,
    usePatientDataStore,
} from "@/stores/patientData.store";
import { formatDate } from "@/utils/dateUtil";

import { TimelineEntryCard } from "../TimelineEntryCard";
import type { TimelineEntryProps } from "./entryProps";
import { savePatientDataFile } from "./patientDataFile";

function programLinkFor(programName: string | undefined): string {
    const name = programName?.toLowerCase().trim() ?? "unknown";
    if (name.includes("breast"))
        return "http://www.bccancer.bc.ca/screening/breast";
    if (name.includes("colon"))
        return "http://www.bccancer.bc.ca/screening/colon";
    if (name.includes("lung"))
        return "http://www.bccancer.bc.ca/screening/lung";
    if (name.includes("cervical"))
        return "http://www.bccancer.bc.ca/screening/cervix";
    return "http://www.bccancer.bc.ca/screening";
}

function programDisplayNameFor(programName: string | undefined): string {
    const name = programName?.trim().toLowerCase() || "unknown";
    const overrides: Record<string, string> = {
        "cervical cancer": "cervix cancer",
    };
    return overrides[name] ?? name;
}

/** Port of entry/BcCancerScreeningTimelineComponent.vue */
export function BcCancerScreeningEntry({
    entry: timelineEntry,
    commentsAreEnabled = false,
    hdid,
    isMobileDetails = false,
    testID,
}: TimelineEntryProps) {
    const entry = timelineEntry as BcCancerScreeningTimelineEntry;
    const retrieveFile = usePatientDataStore((s) => s.retrievePatientDataFile);
    const isLoadingFile = usePatientDataStore(
        selectPatientDataFileLoading(entry.fileId)
    );
    const [confirmVisible, setConfirmVisible] = useState(false);
    const hasFile = Boolean(entry.fileId);
    const programLink = programLinkFor(entry.programName);
    const programDisplayName = programDisplayNameFor(entry.programName);

    function trackProgramLinkClick() {
        services.tracking.trackEvent({
            action: Action.ExternalLink,
            text: `${entry.title} Help`,
            origin: Origin.Timeline,
            type: Type.BcCancerScreening,
            destination: entry.bcCancerDestination,
            url: programLink,
        });
    }

    function downloadFile() {
        if (!entry.fileId) return;
        services.tracking.trackEvent({
            action: Action.Download,
            text: `Download ${entry.title}`,
            dataset: Dataset.BcCancerScreening,
            type: Type.BcCancerScreening,
            format: Format.Pdf,
        });
        const dateString = formatDate(entry.date, "yyyy_MM_dd-HH_mm");
        retrieveFile(hdid, entry.fileId)
            .then((file) =>
                savePatientDataFile(file, `${entry.fileName}_${dateString}.pdf`)
            )
            .catch((error: unknown) => logger.error(describeError(error)));
    }

    return (
        <>
            <TimelineEntryCard
                testID={testID}
                entry={entry}
                entryIcon={
                    getEntryTypeDetails(EntryType.BcCancerScreening)?.icon
                }
                iconClass="bg-primary"
                title={entry.title}
                subtitle={entry.subtitle}
                isMobileDetails={isMobileDetails}
                allowComment={commentsAreEnabled}
                hasAttachment={hasFile}
                onAttachmentPress={() => setConfirmVisible(true)}
            >
                <View className="mb-3">
                    {entry.isResult ? (
                        <AppText testID="bc-cancer-result-body">
                            Download your {programDisplayName} screening result
                            letter. It may include important information about
                            next steps. If you have questions,{" "}
                            <ExternalLink
                                href={programLink}
                                onPress={trackProgramLinkClick}
                            >
                                check the BC Cancer website
                            </ExternalLink>{" "}
                            or talk to your care provider.
                        </AppText>
                    ) : (
                        <AppText testID="bc-cancer-screening-body">
                            Find out about your {programDisplayName} screening
                            next steps. You will also get this letter in the
                            mail.{" "}
                            <ExternalLink
                                href={programLink}
                                onPress={trackProgramLinkClick}
                            >
                                Learn more about {programDisplayName} screening
                            </ExternalLink>
                            .
                        </AppText>
                    )}
                </View>
                {hasFile ? (
                    <View className="flex-row">
                        <HgButton
                            testID="bc-cancer-screening-download-button"
                            variant="secondary"
                            text={entry.callToActionText}
                            requiresConnection
                            icon="eye"
                            loading={isLoadingFile}
                            onPress={() => setConfirmVisible(true)}
                        />
                    </View>
                ) : null}
            </TimelineEntryCard>
            <MessageDialog
                visible={confirmVisible}
                title="Sensitive Document"
                message={SENSITIVE_DOCUMENT_MESSAGE}
                onClose={() => setConfirmVisible(false)}
                onSubmit={downloadFile}
            />
        </>
    );
}
