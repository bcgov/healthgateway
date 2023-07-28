<script setup lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faInfoCircle } from "@fortawesome/free-solid-svg-icons";
import { computed } from "vue";

import DisplayFieldComponent from "@/components/common/DisplayFieldComponent.vue";
import InfoTooltipComponent from "@/components/common/InfoTooltipComponent.vue";
import TimelineEntryComponent from "@/components/private/timeline/TimelineEntryComponent.vue";
import { EntryType, entryTypeMap } from "@/constants/entryType";
import { DateWrapper } from "@/models/dateWrapper";
import HospitalVisitTimelineEntry from "@/models/timeline/hospitalVisitTimelineEntry";
import { useTimelineStore } from "@/stores/timeline";

library.add(faInfoCircle);

interface Props {
    entry: HospitalVisitTimelineEntry;
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
    return entryTypeMap.get(EntryType.HospitalVisit)?.icon;
});

function formatDate(date?: DateWrapper): string | undefined {
    return date?.format("yyyy-MMM-dd, t");
}
</script>

<template>
    <TimelineEntryComponent
        :card-id="index + '-' + datekey"
        :entry-icon="entryIcon"
        icon-class="bg-primary"
        :title="entry.facility"
        :subtitle="entry.visitType"
        :entry="entry"
        :is-mobile-details="isMobileDetails"
        :allow-comment="commentsAreEnabled"
    >
        <v-row>
            <v-col>
                <DisplayFieldComponent
                    data-testid="hospital-visit-location"
                    name="Location"
                    name-class="font-weight-bold"
                    :value="entry.facility"
                >
                    <template v-if="entry.facility" #append-value>
                        <InfoTooltipComponent
                            data-testid="hospital-visit-location-info-button"
                            tooltip-testid="hospital-visit-location-info-popover"
                            text="Virtual visits show your provider's location."
                            class="ml-2"
                        />
                    </template>
                </DisplayFieldComponent>
            </v-col>
        </v-row>
        <v-row>
            <v-col :cols="cols">
                <DisplayFieldComponent
                    data-testid="hospital-visit-provider"
                    name="Provider"
                    name-class="font-weight-bold"
                    :value="entry.provider"
                >
                    <template
                        v-if="entry.provider && entry.facility"
                        #append-value
                    >
                        <InfoTooltipComponent
                            data-testid="hospital-visit-provider-info-button"
                            tooltip-testid="hospital-visit-provider-info-popover"
                            text="Inpatient visits only show the first attending physician."
                            class="ml-2"
                        />
                    </template>
                </DisplayFieldComponent>
            </v-col>
            <v-col :cols="cols">
                <DisplayFieldComponent
                    data-testid="hospital-visit-service"
                    name="Service"
                    name-class="font-weight-bold"
                    :value="entry.healthService"
                />
            </v-col>
            <v-col :cols="cols">
                <DisplayFieldComponent
                    data-testid="hospital-visit-date"
                    name="Visit Date"
                    name-class="font-weight-bold"
                    :value="formatDate(entry.admitDateTime)"
                >
                    <template v-if="entry.outpatient" #append-value>
                        <InfoTooltipComponent
                            data-testid="hospital-visit-date-info-button"
                            tooltip-testid="hospital-visit-date-info-popover"
                            text="Outpatient visits may only show the first in a series of dates."
                            class="ml-2"
                        />
                    </template>
                </DisplayFieldComponent>
            </v-col>
            <v-col :cols="cols">
                <DisplayFieldComponent
                    data-testid="hospital-visit-discharge-date"
                    name="Discharge Date"
                    name-class="font-weight-bold"
                    :value="formatDate(entry.endDateTime)"
                />
            </v-col>
        </v-row>
    </TimelineEntryComponent>
</template>
