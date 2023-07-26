<script setup lang="ts">
import { computed } from "vue";

import DisplayFieldComponent from "@/components/common/DisplayFieldComponent.vue";
import TimelineEntryComponent from "@/components/private/timeline/TimelineEntryComponent.vue";
import { EntryType, entryTypeMap } from "@/constants/entryType";
import ImmunizationTimelineEntry from "@/models/timeline/immunizationTimelineEntry";
import { useTimelineStore } from "@/stores/timeline";

interface Props {
    entry: ImmunizationTimelineEntry;
    index: number;
    datekey: string;
    isMobileDetails?: boolean;
    commentsAreEnabled?: boolean;
}
withDefaults(defineProps<Props>(), {
    isMobileDetails: false,
    commentsAreEnabled: false,
});

const timelineStore = useTimelineStore();

const cols = computed(() => timelineStore.columnCount);
const entryIcon = computed(() => {
    return entryTypeMap.get(EntryType.Immunization)?.icon;
});
</script>

<template>
    <TimelineEntryComponent
        :card-id="index + '-' + datekey"
        :entry-icon="entryIcon"
        icon-class="bg-primary"
        :title="entry.immunization.name"
        :entry="entry"
        :is-mobile-details="isMobileDetails"
        :allow-comment="false"
    >
        <template
            v-for="(agent, agentIndex) in entry.immunization.immunizationAgents"
            :key="agent.code"
        >
            <v-divider v-if="agentIndex > 0" class="my-4" />
            <v-row>
                <v-col :cols="cols">
                    <DisplayFieldComponent
                        data-testid="immunizationProductTitle"
                        name="Product"
                        name-class="font-weight-bold"
                        :value="agent.productName"
                    />
                </v-col>
                <v-col :cols="cols">
                    <DisplayFieldComponent
                        data-testid="immunizationAgentNameTitle"
                        name="Immunizing Agent"
                        name-class="font-weight-bold"
                        :value="agent.name"
                    />
                </v-col>
                <v-col :cols="cols">
                    <DisplayFieldComponent
                        data-testid="immunizationProviderTitle"
                        name="Provider / Clinic"
                        name-class="font-weight-bold"
                        :value="entry.immunization.providerOrClinic"
                    />
                </v-col>
                <v-col :cols="cols">
                    <DisplayFieldComponent
                        data-testid="immunizationLotTitle"
                        name="Lot Number"
                        name-class="font-weight-bold"
                        :value="agent.lotNumber"
                    />
                </v-col>
            </v-row>
        </template>
        <template v-if="entry.immunization.forecast">
            <v-divider class="my-4" />
            <h3 class="text-h6 font-weight-bold mb-4">Forecast</h3>
            <v-row>
                <v-col :cols="cols">
                    <DisplayFieldComponent
                        data-testid="forecastDisplayName"
                        name="Immunization"
                        name-class="font-weight-bold"
                        :value="entry.immunization.forecast.displayName"
                    />
                </v-col>
                <v-col :cols="cols">
                    <DisplayFieldComponent
                        data-testid="forecastDueDate"
                        name="Due Date"
                        name-class="font-weight-bold"
                        :value="entry.immunization.forecast.dueDate"
                    />
                </v-col>
            </v-row>
        </template>
    </TimelineEntryComponent>
</template>
