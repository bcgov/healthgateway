<script lang="ts">
import Vue from "vue";
import { Component, Prop } from "vue-property-decorator";

import { EntryType, entryTypeMap } from "@/constants/entryType";
import ImmunizationTimelineEntry from "@/models/immunizationTimelineEntry";

import EntrycardTimelineComponent from "./EntrycardTimelineComponent.vue";

// eslint-disable-next-line @typescript-eslint/no-explicit-any
const options: any = {
    components: {
        EntryCard: EntrycardTimelineComponent,
    },
};

@Component(options)
export default class ImmunizationTimelineComponent extends Vue {
    @Prop() entry!: ImmunizationTimelineEntry;
    @Prop() index!: number;
    @Prop() datekey!: string;
    @Prop() isMobileDetails!: boolean;

    private get isCovidImmunization(): boolean {
        return (
            this.entry.immunization.valid &&
            this.entry.immunization.targetedDisease
                ?.toLowerCase()
                .includes("covid")
        );
    }

    private get entryIcon(): string | undefined {
        return entryTypeMap.get(EntryType.Immunization)?.icon;
    }
}
</script>

<template>
    <EntryCard
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
                    v-for="(agent, index) in entry.immunization
                        .immunizationAgents"
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
            <p class="my-4" data-testid="timeline-immunization-disclaimer">
                You can add or update immunizations by visiting
                <a
                    href="https://www.immunizationrecord.gov.bc.ca"
                    target="_blank"
                    rel="noopener"
                    >immunizationrecord.gov.bc.ca</a
                >.
            </p>
            <div v-if="entry.immunization.forecast" class="mt-4">
                <strong>Forecast</strong>
                <div class="my-2" data-testid="forecastDisplayName">
                    <strong>Immunization: </strong>
                    {{ entry.immunization.forecast.displayName }}
                </div>
                <div class="my-2" data-testid="forecastDueDate">
                    <strong>Due Date: </strong>
                    {{ entry.immunization.forecast.dueDate }}
                </div>
                <div class="my-2" data-testid="forecastStatus">
                    <strong>Status: </strong>
                    {{ entry.immunization.forecast.status }}
                </div>
            </div>
        </div>
    </EntryCard>
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
