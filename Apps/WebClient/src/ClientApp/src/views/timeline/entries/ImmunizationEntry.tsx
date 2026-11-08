import { View } from "react-native";

import { AppText, DisplayField } from "@/components/ui";
import { EntryType, getEntryTypeDetails } from "@/constants/entryType";
import type { ImmunizationTimelineEntry } from "@/models/timeline/immunizationTimelineEntry";

import { TimelineEntryCard } from "../TimelineEntryCard";
import type { TimelineEntryProps } from "./entryProps";

/** One v-col in the details grid (timelineStore.columnCount is 12: every field takes a full row). */
function Field(props: React.ComponentProps<typeof DisplayField>) {
    return (
        <View className="py-3">
            <DisplayField nameClass="font-bold" {...props} />
        </View>
    );
}

/** Port of entry/ImmunizationTimelineComponent.vue */
export function ImmunizationEntry({
    entry: timelineEntry,
    isMobileDetails = false,
    testID,
}: TimelineEntryProps) {
    const entry = timelineEntry as ImmunizationTimelineEntry;
    const { immunization } = entry;
    return (
        <TimelineEntryCard
            testID={testID}
            entry={entry}
            entryIcon={getEntryTypeDetails(EntryType.Immunization)?.icon}
            iconClass="bg-primary"
            title={immunization.name}
            isMobileDetails={isMobileDetails}
        >
            {immunization.immunizationAgents.map((agent, agentIndex) => (
                <View key={`${agent.code}-${agentIndex}`}>
                    {agentIndex > 0 ? (
                        <View className="h-px bg-border-divider my-4" />
                    ) : null}
                    {/* v-row: 12px gutters around each full-width column */}
                    <View className="-my-3">
                        {agent.productName ? (
                            <Field
                                testID="immunizationProductTitle"
                                name="Product"
                                value={agent.productName}
                            />
                        ) : null}
                        <Field
                            testID="immunizationAgentNameTitle"
                            name="Immunizing Agent"
                            value={agent.name}
                        />
                        {immunization.providerOrClinic ? (
                            <Field
                                testID="immunizationProviderTitle"
                                name="Provider / Clinic"
                                value={immunization.providerOrClinic}
                            />
                        ) : null}
                        {agent.lotNumber ? (
                            <Field
                                testID="immunizationLotTitle"
                                name="Lot Number"
                                value={agent.lotNumber}
                            />
                        ) : null}
                    </View>
                </View>
            ))}
            {immunization.forecast ? (
                <View>
                    <View className="h-px bg-border-divider my-4" />
                    <AppText
                        variant="title-large"
                        className="font-bold mb-4"
                        accessibilityRole="header"
                        aria-level={3}
                    >
                        Forecast
                    </AppText>
                    <View className="-my-3">
                        <Field
                            testID="forecastDisplayName"
                            name="Immunization"
                            value={immunization.forecast.displayName}
                        />
                        <Field
                            testID="forecastDueDate"
                            name="Due Date"
                            value={immunization.forecast.dueDate}
                        />
                    </View>
                </View>
            ) : null}
        </TimelineEntryCard>
    );
}
