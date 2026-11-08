import type { DateTime } from "luxon";
import { View } from "react-native";

import { DisplayField, InfoTooltip } from "@/components/ui";
import { EntryType, getEntryTypeDetails } from "@/constants/entryType";
import type { HospitalVisitTimelineEntry } from "@/models/timeline/hospitalVisitTimelineEntry";
import { formatDate } from "@/utils/dateUtil";

import { TimelineEntryCard } from "../TimelineEntryCard";
import type { TimelineEntryProps } from "./entryProps";

const format = (date?: DateTime) =>
    date ? formatDate(date, "yyyy-MMM-dd, t") : undefined;

function Field(props: React.ComponentProps<typeof DisplayField>) {
    return (
        <View className="py-3">
            <DisplayField nameClass="font-bold" {...props} />
        </View>
    );
}

const Tip = ({
    id,
    topic,
    text,
}: {
    id: string;
    /** what the tip explains, for the trigger's name */
    topic: string;
    text: string;
}) => (
    <View className="ml-2">
        <InfoTooltip
            accessibilityLabel={`More information about the ${topic}`}
            testID={`${id}-info-button`}
            tooltipTestID={`${id}-info-popover`}
            text={text}
        />
    </View>
);

/** Port of entry/HospitalVisitTimelineComponent.vue */
export function HospitalVisitEntry({
    entry: timelineEntry,
    commentsAreEnabled = false,
    isMobileDetails = false,
    testID,
}: TimelineEntryProps) {
    const entry = timelineEntry as HospitalVisitTimelineEntry;
    return (
        <TimelineEntryCard
            testID={testID}
            entry={entry}
            entryIcon={getEntryTypeDetails(EntryType.HospitalVisit)?.icon}
            iconClass="bg-primary"
            title={entry.facility}
            subtitle={entry.visitType}
            isMobileDetails={isMobileDetails}
            allowComment={commentsAreEnabled}
        >
            <Field
                testID="hospital-visit-location"
                name="Location"
                value={entry.facility}
                append={
                    entry.facility ? (
                        <Tip
                            id="hospital-visit-location"
                            topic="location"
                            text="Virtual visits show your provider's location."
                        />
                    ) : undefined
                }
            />
            <View className="-my-3 py-3">
                <Field
                    testID="hospital-visit-provider"
                    name="Provider"
                    value={entry.provider}
                    append={
                        entry.provider && entry.facility ? (
                            <Tip
                                id="hospital-visit-provider"
                                topic="provider"
                                text="Inpatient visits only show the first attending physician."
                            />
                        ) : undefined
                    }
                />
                <Field
                    testID="hospital-visit-service"
                    name="Service"
                    value={entry.healthService}
                />
                <Field
                    testID="hospital-visit-date"
                    name="Visit Date"
                    value={format(entry.admitDateTime)}
                    append={
                        entry.outpatient ? (
                            <Tip
                                id="hospital-visit-date"
                                topic="visit date"
                                text="Outpatient visits may only show the first in a series of dates."
                            />
                        ) : undefined
                    }
                />
                <Field
                    testID="hospital-visit-discharge-date"
                    name="Discharge Date"
                    value={format(entry.endDateTime)}
                />
            </View>
        </TimelineEntryCard>
    );
}
