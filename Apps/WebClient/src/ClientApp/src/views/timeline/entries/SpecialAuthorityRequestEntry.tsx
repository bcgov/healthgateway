import { View } from "react-native";

import { DisplayField } from "@/components/ui";
import { EntryType, getEntryTypeDetails } from "@/constants/entryType";
import type { SpecialAuthorityRequestTimelineEntry } from "@/models/timeline/specialAuthorityRequestTimelineEntry";
import { formatDate } from "@/utils/dateUtil";

import { TimelineEntryCard } from "../TimelineEntryCard";
import type { TimelineEntryProps } from "./entryProps";

function Field(props: React.ComponentProps<typeof DisplayField>) {
    return (
        <View className="py-3">
            <DisplayField nameClass="font-bold" {...props} />
        </View>
    );
}

/** DateTimeFormat.formatDateString */
const formatDateString = "yyyy-MM-dd";

/** Port of entry/SpecialAuthorityRequestTimelineComponent.vue */
export function SpecialAuthorityRequestEntry({
    entry: timelineEntry,
    commentsAreEnabled = false,
    isMobileDetails = false,
    testID,
}: TimelineEntryProps) {
    const entry = timelineEntry as SpecialAuthorityRequestTimelineEntry;
    return (
        <TimelineEntryCard
            testID={testID}
            entry={entry}
            entryIcon={
                getEntryTypeDetails(EntryType.SpecialAuthorityRequest)?.icon
            }
            iconClass="bg-primary"
            title={entry.drugName}
            subtitle={`Status: ${String(entry.requestStatus)}`}
            isMobileDetails={isMobileDetails}
            allowComment={commentsAreEnabled}
        >
            <View className="-my-3">
                <Field
                    testID="medicationPractitioner"
                    name="Prescriber Name"
                    value={entry.prescriberName}
                />
                <Field name="Reference Number" value={entry.referenceNumber} />
                <Field
                    name="Effective Date"
                    value={
                        entry.effectiveDate
                            ? formatDate(entry.effectiveDate, formatDateString)
                            : undefined
                    }
                />
                <Field
                    name="Expiry Date"
                    value={
                        entry.expiryDate
                            ? formatDate(entry.expiryDate, formatDateString)
                            : undefined
                    }
                />
            </View>
        </TimelineEntryCard>
    );
}
