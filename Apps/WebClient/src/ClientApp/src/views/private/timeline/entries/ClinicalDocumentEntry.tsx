import { useState } from "react";
import { View } from "react-native";

import { DisplayField, HgButton, MessageDialog } from "@/components/common";
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
import type { ClinicalDocumentTimelineEntry } from "@/models/timeline/clinicalDocumentTimelineEntry";
import { services } from "@/services/container";
import { describeError, logger } from "@/services/logger";
import { useClinicalDocumentFileStore } from "@/stores/clinicalDocument.store";
import { formatDate } from "@/utils/dateUtil";
import { downloadEncodedMedia } from "@/utils/encodedMedia";

import { TimelineEntryCard } from "../TimelineEntryCard";
import type { TimelineEntryProps } from "./entryProps";

function Field(props: React.ComponentProps<typeof DisplayField>) {
    return (
        <View className="py-3">
            <DisplayField nameClass="font-bold" {...props} />
        </View>
    );
}

/** Port of entry/ClinicalDocumentTimelineComponent.vue */
export function ClinicalDocumentEntry({
    entry: timelineEntry,
    commentsAreEnabled = false,
    hdid,
    isMobileDetails = false,
    testID,
}: TimelineEntryProps) {
    const entry = timelineEntry as ClinicalDocumentTimelineEntry;
    const getFile = useClinicalDocumentFileStore((s) => s.getFile);
    const [confirmVisible, setConfirmVisible] = useState(false);
    // The web client's isLoadingFile checks `fileId in files` on a Map, which is always false, so the button never
    // shows loading there; kept for parity.
    const isLoadingFile = false;

    async function downloadFile() {
        services.tracking.trackEvent({
            action: Action.Download,
            text: Text.DownloadClinicalDocument,
            origin: Origin.Timeline,
            dataset: Dataset.ClinicalDocuments,
            type: Type.ClinicalDocuments,
            format: Format.Pdf,
        });
        try {
            const result = await getFile(entry.fileId, hdid);
            const dateString = formatDate(entry.date, "yyyy_MM_dd-HH_mm");
            await downloadEncodedMedia(
                result,
                `Clinical_Document_${dateString}.pdf`
            );
        } catch (error) {
            logger.error(describeError(error));
        }
    }

    return (
        <>
            <TimelineEntryCard
                testID={testID}
                entry={entry}
                entryIcon={
                    getEntryTypeDetails(EntryType.ClinicalDocument)?.icon
                }
                iconClass="bg-primary"
                title={entry.name}
                subtitle={entry.documentType}
                isMobileDetails={isMobileDetails}
                allowComment={commentsAreEnabled}
                hasAttachment
                onAttachmentPress={() => setConfirmVisible(true)}
            >
                <View className="-my-3">
                    <Field
                        testID="clinical-document-discipline"
                        name="Type"
                        value={entry.discipline}
                    />
                    <Field
                        testID="clinical-document-facility"
                        name="Facility"
                        value={entry.facilityName}
                    />
                </View>
                <View className="flex-row py-3 mt-3">
                    <HgButton
                        testID="clinical-document-download-button"
                        variant="secondary"
                        icon="download"
                        text="Download Full Report"
                        requiresConnection
                        loading={isLoadingFile}
                        onPress={() => setConfirmVisible(true)}
                    />
                </View>
            </TimelineEntryCard>
            <MessageDialog
                visible={confirmVisible}
                title="Sensitive Document"
                message={SENSITIVE_DOCUMENT_MESSAGE}
                onClose={() => setConfirmVisible(false)}
                onSubmit={() => void downloadFile()}
            />
        </>
    );
}
