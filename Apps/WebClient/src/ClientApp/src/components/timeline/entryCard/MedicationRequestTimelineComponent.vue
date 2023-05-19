<script setup lang="ts">
import { computed } from "vue";

import EntryCardTimelineComponent from "@/components/timeline/entryCard/EntrycardTimelineComponent.vue";
import DateTimeFormat from "@/constants/dateTimeFormat";
import { EntryType, entryTypeMap } from "@/constants/entryType";
import { DateWrapper } from "@/models/dateWrapper";
import MedicationRequestTimelineEntry from "@/models/medicationRequestTimelineEntry";

interface Props {
    entry: MedicationRequestTimelineEntry;
    index: number;
    datekey: string;
    isMobileDetails?: boolean;
    commentsAreEnabled?: boolean;
}
withDefaults(defineProps<Props>(), {
    isMobileDetails: false,
    commentsAreEnabled: false,
});

const entryIcon = computed(
    () => entryTypeMap.get(EntryType.SpecialAuthorityRequest)?.icon
);

function formatDate(date: DateWrapper): string {
    return date.format(DateTimeFormat.formatDateString);
}
</script>

<template>
    <EntryCardTimelineComponent
        :card-id="index + '-' + datekey"
        :entry-icon="entryIcon"
        :title="entry.drugName"
        :subtitle="'Status: ' + entry.requestStatus"
        :entry="entry"
        :is-mobile-details="isMobileDetails"
        :allow-comment="commentsAreEnabled"
    >
        <b-row slot="details-body" class="justify-content-between">
            <b-col>
                <div>
                    <div class="detailSection">
                        <div data-testid="medicationPractitioner">
                            <strong>Prescriber Name:</strong>
                            {{
                                entry.prescriberFirstName +
                                " " +
                                entry.prescriberLastName
                            }}
                        </div>
                    </div>
                    <div class="detailSection">
                        <div>
                            <strong>Effective Date:</strong>
                            <span class="text-nowrap">
                                {{ formatDate(entry.effectiveDate) }}
                            </span>
                        </div>
                        <div>
                            <strong>Expiry Date:</strong>
                            <span class="text-nowrap">
                                {{ formatDate(entry.expiryDate) }}
                            </span>
                        </div>
                        <div>
                            <strong>Reference Number:</strong>
                            {{ entry.referenceNumber }}
                        </div>
                    </div>
                </div>
            </b-col>
        </b-row>
    </EntryCardTimelineComponent>
</template>

<style lang="scss" scoped>
.col {
    padding: 0px;
    margin: 0px;
}

.row {
    padding: 0px;
    margin: 0px;
}

.detailSection {
    margin-top: 15px;
}
</style>
