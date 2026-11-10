import { useState } from "react";
import { Pressable, View } from "react-native";

import {
    AppText,
    DisplayField,
    ExternalLink,
    HgAlert,
    Icon,
} from "@/components/common";
import { EntryType, getEntryTypeDetails } from "@/constants/entryType";
import { useThemeColors } from "@/hooks/useTheme";
import type { HealthVisitTimelineEntry } from "@/models/timeline/healthVisitTimelineEntry";

import { TimelineEntryCard } from "../TimelineEntryCard";
import type { TimelineEntryProps } from "./entryProps";

export const HEALTH_VISIT_GUIDE_URL =
    "https://www2.gov.bc.ca/gov/content/health/managing-your-health/health-gateway/guide/healthrecords#healthandhospital";

/** The billing-claim note shared by the entry's info panel and the timeline's health-visit-only alert */
export function HealthVisitBillingInfo() {
    return (
        <View>
            <AppText className="mb-4">
                Information is from the billing claim and may show a different
                practitioner or clinic than the one you visited. For more
                information, see our{" "}
                <ExternalLink href={HEALTH_VISIT_GUIDE_URL}>
                    Support Guide
                </ExternalLink>
                .
            </AppText>
            <AppText>
                Health Gateway shows your health visits billed to the BC Medical
                Services Plan (MSP) for the past seven years.
            </AppText>
        </View>
    );
}

/** Port of entry/HealthVisitTimelineComponent.vue */
export function HealthVisitEntry({
    entry: timelineEntry,
    commentsAreEnabled = false,
    isMobileDetails = false,
    testID,
}: TimelineEntryProps) {
    const entry = timelineEntry as HealthVisitTimelineEntry;
    const colors = useThemeColors();
    const [showInfoDetails, setShowInfoDetails] = useState(false);
    return (
        <TimelineEntryCard
            testID={testID}
            entry={entry}
            entryIcon={getEntryTypeDetails(EntryType.HealthVisit)?.icon}
            iconClass="bg-primary"
            title={entry.specialtyDescription}
            subtitle={entry.practitionerName}
            isMobileDetails={isMobileDetails}
            allowComment={commentsAreEnabled}
        >
            <View className="py-3 mb-1">
                <DisplayField
                    testID="encounterClinicName"
                    nameClass="font-bold"
                    name="Clinic/Practitioner"
                    value={entry.clinic.name}
                    append={
                        <Pressable
                            testID="health-visit-clinic-info-button"
                            accessibilityRole="button"
                            accessibilityLabel="More information about the clinic"
                            aria-expanded={showInfoDetails}
                            onPress={() => setShowInfoDetails((v) => !v)}
                            className="ml-1 p-1"
                        >
                            <Icon
                                name="circle-info"
                                size={20}
                                color={colors.primary}
                            />
                        </Pressable>
                    }
                />
            </View>
            {/* v-show: kept mounted, hidden */}
            <View style={showInfoDetails ? undefined : { display: "none" }}>
                <HgAlert
                    testID="health-visit-clinic-name-info-popover"
                    type="info"
                    className="mb-6"
                >
                    <HealthVisitBillingInfo />
                </HgAlert>
            </View>
            {entry.showRollOffWarning ? (
                <HgAlert
                    testID="encounterRolloffAlert"
                    type="warning"
                    centerContent
                    text="Health visits are shown for the past 6 years only. You may wish to export and save older records so you still have them when the calendar year changes."
                />
            ) : null}
        </TimelineEntryCard>
    );
}
