<script setup lang="ts">
import { computed, ref } from "vue";

import DisplayFieldComponent from "@/components/common/DisplayFieldComponent.vue";
import HgAlertComponent from "@/components/common/HgAlertComponent.vue";
import HgIconButtonComponent from "@/components/common/HgIconButtonComponent.vue";
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
        <v-row class="mb-1">
            <v-col :cols="cols">
                <DisplayFieldComponent
                    data-testid="encounterClinicName"
                    name="Clinic/Practitioner"
                    name-class="font-weight-bold"
                    :value="entry.clinic.name"
                >
                    <template #append>
                        <HgIconButtonComponent
                            data-testid="health-visit-clinic-info-button"
                            aria-label="More Information"
                            class="ml-1 text-primary"
                            size="x-small"
                            @click="showInfoDetails = !showInfoDetails"
                        >
                            <v-icon icon="info-circle" size="large" />
                        </HgIconButtonComponent>
                    </template>
                </DisplayFieldComponent>
            </v-col>
        </v-row>
        <v-slide-y-transition>
            <HgAlertComponent
                v-show="showInfoDetails"
                data-testid="health-visit-clinic-name-info-popover"
                class="d-print-none mb-6"
                type="info"
                variant="outlined"
            >
                <p class="text-body-1">
                    Information is from the billing claim and may show a
                    different practitioner or clinic from the one you visited.
                    For more information, visit the
                    <a
                        href="https://www2.gov.bc.ca/gov/content?id=FE8BA7F9F1F0416CB2D24CF71C4BAF80#healthandhospital"
                        target="_blank"
                        rel="noopener"
                        class="text-link"
                        >FAQ</a
                    >
                    page.
                </p>
                <p class="text-body-1">
                    Health Gateway shows your health visits billed to the BC
                    Medical Services Plan (MSP) for the past seven years. You
                    can also get your hospital visits since 2021 for everywhere
                    except Interior Health. We are working on adding Interior
                    visits in the future.
                </p>
            </HgAlertComponent>
        </v-slide-y-transition>
        <HgAlertComponent
            v-if="entry.showRollOffWarning"
            data-testid="encounterRolloffAlert"
            class="d-print-none"
            type="warning"
            variant="outlined"
            text="Health visits are shown for the past 6 years only. You may wish
                to export and save older records so you still have them when the
                calendar year changes."
            :center-content="true"
        />
    </TimelineEntryComponent>
</template>
