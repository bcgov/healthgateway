<script setup lang="ts">
import { computed, ref } from "vue";

import DisplayFieldComponent from "@/components/common/DisplayFieldComponent.vue";
import TimelineEntryComponent from "@/components/private/timeline/TimelineEntryComponent.vue";
import { EntryType, entryTypeMap } from "@/constants/entryType";
import HealthVisitTimelineEntry from "@/models/timeline/healthVisitTimelineEntry";
import { useTimelineStore } from "@/stores/timeline";

interface Props {
    entry: HealthVisitTimelineEntry;
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

const showInfoDetails = ref(false);

const cols = computed(() => timelineStore.columnCount);
const entryIcon = computed(() => entryTypeMap.get(EntryType.HealthVisit)?.icon);
</script>

<template>
    <TimelineEntryComponent
        :card-id="index + '-' + datekey"
        :entry-icon="entryIcon"
        icon-class="bg-primary"
        :title="entry.specialtyDescription"
        :subtitle="entry.practitionerName"
        :entry="entry"
        :is-mobile-details="isMobileDetails"
        :allow-comment="commentsAreEnabled"
    >
        <v-row class="mb-3">
            <v-col :cols="cols">
                <DisplayFieldComponent
                    data-testid="encounterClinicName"
                    name="Clinic/Practitioner"
                    name-class="font-weight-bold"
                    :value="entry.clinic.name"
                >
                    <template #append>
                        <v-icon
                            aria-label="More Information"
                            class="ml-2"
                            icon="info-circle"
                            color="primary"
                            size="small"
                            @click="showInfoDetails = !showInfoDetails"
                        />
                    </template>
                </DisplayFieldComponent>
            </v-col>
        </v-row>
        <v-slide-y-transition>
            <v-alert
                v-show="showInfoDetails"
                data-testid="health-visit-clinic-name-info-popover"
                class="d-print-none mb-6"
                type="info"
                variant="outlined"
                border
            >
                <p class="text-body-1">
                    Information is from the billing claim and may show a
                    different practitioner or clinic from the one you visited.
                    For more information, visit the
                    <a
                        href="https://www2.gov.bc.ca/gov/content?id=FE8BA7F9F1F0416CB2D24CF71C4BAF80#healthandhospital"
                        target="_blank"
                        rel="noopener"
                        >FAQ</a
                    >
                    page.
                </p>
            </v-alert>
        </v-slide-y-transition>
        <v-alert
            v-if="entry.showRollOffWarning"
            data-testid="encounterRolloffAlert"
            class="d-print-none"
            type="warning"
            icon="circle-exclamation"
            variant="outlined"
            border
            text="Health visits are shown for the past 6 years only. You may wish
                to export and save older records so you still have them when the
                calendar year changes."
        />
    </TimelineEntryComponent>
</template>
