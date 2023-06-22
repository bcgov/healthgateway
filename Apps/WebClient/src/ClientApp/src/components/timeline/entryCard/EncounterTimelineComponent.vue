<script setup lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faInfoCircle } from "@fortawesome/free-solid-svg-icons";
import { computed } from "vue";

import EntryCardTimelineComponent from "@/components/timeline/entryCard/EntrycardTimelineComponent.vue";
import { EntryType, entryTypeMap } from "@/constants/entryType";
import EncounterTimelineEntry from "@/models/encounterTimelineEntry";

library.add(faInfoCircle);

interface Props {
    entry: EncounterTimelineEntry;
    index: number;
    datekey: string;
    isMobileDetails?: boolean;
    commentsAreEnabled?: boolean;
}
const props = withDefaults(defineProps<Props>(), {
    isMobileDetails: false,
    commentsAreEnabled: false,
});

const entryIcon = computed(() => entryTypeMap.get(EntryType.HealthVisit)?.icon);
const showEncounterRolloffAlert = computed(() =>
    props.entry.showRollOffWarning()
);
</script>

<template>
    <EntryCardTimelineComponent
        :card-id="index + '-' + datekey"
        :entry-icon="entryIcon"
        :title="entry.specialtyDescription"
        :subtitle="entry.practitionerName"
        :entry="entry"
        :is-mobile-details="isMobileDetails"
        :allow-comment="commentsAreEnabled"
    >
        <b-row slot="details-body">
            <b-col>
                <div class="d-inline-flex" data-testid="encounterClinicLabel">
                    <strong>Clinic/Practitioner:</strong>
                    <hg-button
                        v-if="entry.clinic.name"
                        :id="`tooltip-info${index}-${datekey}`"
                        aria-label="Info"
                        href="#"
                        variant="link"
                        data-testid="health-visit-clinic-name-info-button"
                        class="shadow-none align-baseline p-0 ml-1"
                    >
                        <hg-icon icon="info-circle" size="small" />
                    </hg-button>
                    <b-popover
                        v-if="entry.clinic.name"
                        :target="`tooltip-info${index}-${datekey}`"
                        triggers="hover focus"
                        placement="topright"
                        boundary="viewport"
                        data-testid="health-visit-clinic-name-info-popover"
                    >
                        <span>
                            Information is from the billing claim and may show a
                            different practitioner or clinic from the one you
                            visited. For more information, visit the
                            <a
                                href="https://www2.gov.bc.ca/gov/content?id=FE8BA7F9F1F0416CB2D24CF71C4BAF80#healthandhospital"
                                target="_blank"
                                rel="noopener"
                                >FAQ</a
                            >
                            page.
                        </span>
                    </b-popover>
                </div>
                <div data-testid="encounterClinicName">
                    {{ entry.clinic.name }}
                </div>
                <div>
                    <b-alert
                        :show="showEncounterRolloffAlert"
                        variant="warning"
                        class="no-print mt-3 mb-0"
                        data-testid="encounterRolloffAlert"
                    >
                        <span>
                            Health visits are shown for the past 6 years only.
                            You may wish to export and save older records so you
                            still have them when the calendar year changes.
                        </span>
                    </b-alert>
                </div>
            </b-col>
        </b-row>
    </EntryCardTimelineComponent>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

.col {
    padding: 0px;
    margin: 0px;
}

.row {
    padding: 0;
    margin: 0px;
}

.infoIcon {
    color: $aquaBlue;
}
</style>
