<script setup lang="ts">
import { computed } from "vue";

import EntryCardTimelineComponent from "@/components/timeline/entryCard/EntrycardTimelineComponent.vue";
import { EntryType, entryTypeMap } from "@/constants/entryType";
import ImmunizationTimelineEntry from "@/models/immunizationTimelineEntry";

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

const entryIcon = computed(() => {
    return entryTypeMap.get(EntryType.Immunization)?.icon;
});
</script>

<template>
    <EntryCardTimelineComponent
        :card-id="index + '-' + datekey"
        :entry-icon="entryIcon"
        :title="entry.immunization.name"
        :entry="entry"
        :is-mobile-details="isMobileDetails"
        :allow-comment="false"
    >
        <div slot="details-body">
            <div>
                <div
                    v-for="agent in entry.immunization.immunizationAgents"
                    :key="agent.code"
                    class="my-2"
                >
                    <hr v-if="index > 0" />
                    <div class="my-2" data-testid="immunizationProductTitle">
                        <strong> Product: </strong>
                        {{ agent.productName }}
                    </div>
                    <div class="my-2" data-testid="immunizationAgentNameTitle">
                        <strong> Immunizing Agent: </strong>
                        {{ agent.name }}
                    </div>
                    <div class="my-2" data-testid="immunizationProviderTitle">
                        <strong> Provider / Clinic: </strong>
                        {{ entry.immunization.providerOrClinic }}
                    </div>
                    <div class="my-2" data-testid="immunizationLotTitle">
                        <strong> Lot Number: </strong>
                        {{ agent.lotNumber }}
                    </div>
                </div>
            </div>
            <div v-if="entry.immunization.forecast" class="mt-4">
                <strong>Forecast</strong>
                <div class="my-2 text-muted" data-testid="forecastDisplayName">
                    <strong>Immunization: </strong>
                    {{ entry.immunization.forecast.displayName }}
                </div>
                <div class="my-2 text-muted" data-testid="forecastDueDate">
                    <strong>Due Date: </strong>
                    {{ entry.immunization.forecast.dueDate }}
                </div>
            </div>
        </div>
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
</style>
