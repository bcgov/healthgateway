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
import { SENSITIVE_DOCUMENT_MESSAGE } from "@/constants/sensitiveDocument";
import {
    Action,
    Dataset,
    Format,
    Origin,
    Text,
    Type,
} from "@/constants/tracking";
import type { DiagnosticImagingTimelineEntry } from "@/models/timeline/diagnosticImagingTimelineEntry";
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

function Field(props: React.ComponentProps<typeof DisplayField>) {
    return (
        <View className="py-3">
            <DisplayField nameClass="font-bold" {...props} />
        </View>
    );
}

const Bullet = ({ children }: { children: React.ReactNode }) => (
    <View className="flex-row">
        <AppText className="w-6">{"•"}</AppText>
        <AppText className="flex-1">{children}</AppText>
    </View>
);

/** Port of entry/DiagnosticImagingTimelineComponent.vue */
export function DiagnosticImagingEntry({
    entry: timelineEntry,
    commentsAreEnabled = false,
    hdid,
    isMobileDetails = false,
    testID,
}: TimelineEntryProps) {
    const entry = timelineEntry as DiagnosticImagingTimelineEntry;
    const retrieveFile = usePatientDataStore((s) => s.retrievePatientDataFile);
    const isLoadingFile = usePatientDataStore(
        selectPatientDataFileLoading(entry.fileId)
    );
    const [confirmVisible, setConfirmVisible] = useState(false);
    const hasFile = Boolean(entry.fileId);

    function downloadFile() {
        if (!entry.fileId) return;
        services.tracking.trackEvent({
            action: Action.Download,
            text: Text.DownloadImagingReport,
            origin: Origin.Timeline,
            dataset: Dataset.ImagingReports,
            type: Type.ImagingReports,
            format: Format.Pdf,
        });
        const dateString = formatDate(entry.date, "yyyy_MM_dd-HH_mm");
        retrieveFile(hdid, entry.fileId)
            .then((file) =>
                savePatientDataFile(file, `diagnostic_image_${dateString}.pdf`)
            )
            .catch((error: unknown) => logger.error(describeError(error)));
    }

    return (
        <>
            <TimelineEntryCard
                testID={testID}
                entry={entry}
                entryIcon={
                    getEntryTypeDetails(EntryType.DiagnosticImaging)?.icon
                }
                iconClass="bg-primary"
                title={entry.title}
                subtitle={`Status: ${entry.examStatus}`}
                isMobileDetails={isMobileDetails}
                allowComment={commentsAreEnabled}
                hasAttachment={hasFile}
                onAttachmentPress={() => setConfirmVisible(true)}
            >
                <AppText className="italic mb-4">
                    Imaging reports may contain sensitive information. Consult
                    your provider if you are unsure about viewing.
                </AppText>
                <View className="-my-3 mb-3">
                    <Field
                        testID="diagnostic-imaging-procedure-description"
                        name="Description"
                        value={entry.procedureDescription}
                    />
                    <Field
                        testID="diagnostic-imaging-health-authority"
                        name="Health Authority"
                        value={entry.healthAuthority}
                    />
                </View>
                {hasFile ? (
                    <View className="flex-row mb-6">
                        <HgButton
                            testID="diagnostic-imaging-download-button"
                            variant="secondary"
                            text="Download Full Report"
                            requiresConnection
                            icon="download"
                            loading={isLoadingFile}
                            onPress={() => setConfirmVisible(true)}
                        />
                    </View>
                ) : null}
                <AppText className="mb-4">
                    If you have questions about your imaging report: Contact
                    your primary care provider or call HealthlinkBC at 811.
                </AppText>
                <AppText className="mb-4">
                    You can learn more about medical imaging tests on the
                    following websites:
                </AppText>
                <Bullet>
                    <ExternalLink href="https://www.healthlinkbc.ca/tests-treatments-medications/medical-tests">
                        HealthLink BC
                    </ExternalLink>
                </Bullet>
                <Bullet>
                    <ExternalLink href="https://bcradiology.ca/patient-resources/">
                        BC Radiological Society – Patient Resources
                    </ExternalLink>
                </Bullet>
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
