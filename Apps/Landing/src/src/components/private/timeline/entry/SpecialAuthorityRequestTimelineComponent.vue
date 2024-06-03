<script setup lang="ts">
import { computed } from "vue";

import DisplayFieldComponent from "@/components/common/DisplayFieldComponent.vue";
import TimelineEntryComponent from "@/components/private/timeline/TimelineEntryComponent.vue";
import DateTimeFormat from "@/constants/dateTimeFormat";
import { EntryType, entryTypeMap } from "@/constants/entryType";
import { DateWrapper } from "@/models/dateWrapper";
import SpecialAuthorityRequestTimelineEntry from "@/models/timeline/specialAuthorityRequestTimelineEntry";
import { useTimelineStore } from "@/stores/timeline";

interface Props {
    entry: SpecialAuthorityRequestTimelineEntry;
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
const entryIcon = computed(
    () => entryTypeMap.get(EntryType.SpecialAuthorityRequest)?.icon
);

function formatDate(date: DateWrapper): string {
    return date.format(DateTimeFormat.formatDateString);
}
</script>

<template>
    <TimelineEntryComponent
        :card-id="index + '-' + datekey"
        :entry-icon="entryIcon"
        icon-class="bg-primary"
        :title="entry.drugName"
        :subtitle="'Status: ' + entry.requestStatus"
        :entry="entry"
        :is-mobile-details="isMobileDetails"
        :allow-comment="commentsAreEnabled"
    >
        <v-row>
            <v-col :cols="cols">
                <DisplayFieldComponent
                    data-testid="medicationPractitioner"
                    name="Prescriber Name"
                    name-class="font-weight-bold"
                    :value="entry.prescriberName"
                />
            </v-col>
            <v-col :cols="cols">
                <DisplayFieldComponent
                    name="Reference Number"
                    name-class="font-weight-bold"
                    :value="entry.referenceNumber"
                />
            </v-col>
            <v-col :cols="cols">
                <DisplayFieldComponent
                    name="Effective Date"
                    name-class="font-weight-bold"
                    :value="
                        entry.effectiveDate
                            ? formatDate(entry.effectiveDate)
                            : undefined
                    "
                />
            </v-col>
            <v-col :cols="cols">
                <DisplayFieldComponent
                    name="Expiry Date"
                    name-class="font-weight-bold"
                    :value="
                        entry.expiryDate
                            ? formatDate(entry.expiryDate)
                            : undefined
                    "
                />
            </v-col>
        </v-row>
    </TimelineEntryComponent>
</template>
