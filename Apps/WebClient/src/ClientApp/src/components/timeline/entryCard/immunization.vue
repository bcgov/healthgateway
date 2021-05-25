<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faIdCard, faSyringe } from "@fortawesome/free-solid-svg-icons";
import Vue from "vue";
import { Component, Prop } from "vue-property-decorator";

import EventBus, { EventMessageName } from "@/eventbus";
import ImmunizationTimelineEntry from "@/models/immunizationTimelineEntry";

library.add(faIdCard, faSyringe);

import EntrycardTimelineComponent from "./entrycard.vue";

@Component({
    components: {
        EntryCard: EntrycardTimelineComponent,
    },
})
export default class ImmunizationTimelineComponent extends Vue {
    @Prop() entry!: ImmunizationTimelineEntry;
    @Prop() index!: number;
    @Prop() datekey!: string;
    @Prop() isMobileDetails!: boolean;

    private eventBus = EventBus;

    private get isCovidImmunization(): boolean {
        return this.entry.immunization.targetedDisease
            ?.toLowerCase()
            .includes("covid");
    }

    private showCard(): void {
        this.eventBus.$emit(EventMessageName.TimelineCovidCard);
    }
}
</script>

<template>
    <EntryCard
        :card-id="index + '-' + datekey"
        entry-icon="syringe"
        :title="entry.immunization.name"
        :entry="entry"
        :is-mobile-details="isMobileDetails"
        :allow-comment="false"
        :has-attachment="isCovidImmunization"
    >
        <b-row slot="details-body" class="justify-content-between">
            <b-col>
                <b-row>
                    <b-col>
                        <b-row
                            v-for="agent in entry.immunization
                                .immunizationAgents"
                            :key="agent.code"
                            class="my-2"
                        >
                            <b-col>
                                <div data-testid="immunizationProductTitle">
                                    <strong> Product: </strong>
                                    {{ agent.productName }}
                                </div>
                                <div data-testid="immunizationAgentNameTitle">
                                    <strong> Immunizing Agent: </strong>
                                    {{ agent.name }}
                                </div>
                                <div data-testid="immunizationProviderTitle">
                                    <strong> Provider / Clinic: </strong>
                                    {{ entry.immunization.providerOrClinic }}
                                </div>
                                <div data-testid="immunizationLotTitle">
                                    <strong> Lot Number: </strong>
                                    {{ agent.lotNumber }}
                                </div>
                            </b-col>
                        </b-row>
                    </b-col>
                    <b-col
                        v-if="isCovidImmunization"
                        cols="auto"
                        class="text-center pr-0"
                    >
                        <b-btn
                            data-testid="cardBtn"
                            class="detailsButton"
                            variant="link"
                            @click="showCard()"
                        >
                            <hg-icon
                                icon="id-card"
                                size="large"
                                fixed-width
                                class="card-button"
                            />
                            <span>View Card</span>
                        </b-btn>
                    </b-col>
                </b-row>
                <b-row v-if="entry.immunization.forecast" class="mt-3">
                    <b-col>
                        <strong>Forecast:</strong>
                        <b-row class="my-1">
                            <b-col>
                                <div data-testid="forecastDisplayName">
                                    <strong>Immunization: </strong>
                                    {{
                                        entry.immunization.forecast.displayName
                                    }}
                                </div>
                                <div data-testid="forecastDueDate">
                                    <strong>Due Date: </strong>
                                    {{ entry.immunization.forecast.dueDate }}
                                </div>
                                <div data-testid="forecastStatus">
                                    <strong>Status: </strong>
                                    {{ entry.immunization.forecast.status }}
                                </div>
                            </b-col>
                        </b-row>

                        <div v-if="isCovidImmunization">
                            <br />
                            <p data-testid="forecastFollowDirections">
                                If your COVID-19 Dose 2 forecast date has been 
                                moved up, you will receive an invitation to book 
                                an appointment in the next few days. Do not be 
                                concerned if your Dose 2 forecast date has already 
                                passed. You will still be able to book your second dose. 
                                For
                                information on recommended immunizations, please
                                visit
                                <a href="https://immunizebc.ca/" target="blank_"
                                    >https://immunizebc.ca/</a
                                >.
                            </p>
                        </div>
                    </b-col>
                </b-row>
            </b-col>
        </b-row>
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

.hg-icon.card-button {
    display: block;
    color: $medium_background;
    font-size: 3.5rem;
}
</style>
