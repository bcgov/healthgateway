<script lang="ts">
import { IconDefinition, library } from "@fortawesome/fontawesome-svg-core";
import { faIdCard, faSyringe } from "@fortawesome/free-solid-svg-icons";
import Vue from "vue";
import { Component, Prop } from "vue-property-decorator";

import EventBus, { EventMessageName } from "@/eventbus";
import ImmunizationTimelineEntry from "@/models/immunizationTimelineEntry";

library.add(faIdCard);

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

    private eventBus = EventBus;
    private forecastVisible = false;

    private get entryIcon(): IconDefinition {
        return faSyringe;
    }

    private get isCovidImmunization(): boolean {
        return this.entry.immunization.targetedDisease
            ?.toLowerCase()
            .includes("covid");
    }

    private toggleDetails(): void {
        this.forecastVisible = !this.forecastVisible;
    }

    private showCard(): void {
        this.eventBus.$emit(EventMessageName.TimelineCovidCard);
    }
}
</script>

<template>
    <EntryCard
        :card-id="index + '-' + datekey"
        :entry-icon="entryIcon"
        :title="entry.immunization.name"
        :entry="entry"
        :allow-comment="false"
    >
        <b-row slot="details-body" class="justify-content-between">
            <b-col>
                <b-row
                    v-for="agent in entry.immunization.immunizationAgents"
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
                            ><font-awesome-icon
                                icon="id-card"
                                class="card-button"
                                fixed-width
                                size="1x"
                            ></font-awesome-icon
                            ><span>View Card</span></b-btn
                        ></b-col
                    >
                </b-row>
                <b-row v-if="entry.immunization.forecast">
                    <b-col>
                        <div class="d-flex flex-row-reverse">
                            <b-btn
                                data-testid="detailsBtn"
                                variant="link"
                                class="detailsButton"
                                @click="toggleDetails()"
                            >
                                <span v-if="forecastVisible">
                                    <font-awesome-icon
                                        icon="chevron-up"
                                        aria-hidden="true"
                                    ></font-awesome-icon
                                ></span>
                                <span v-else>
                                    <font-awesome-icon
                                        icon="chevron-down"
                                        aria-hidden="true"
                                    ></font-awesome-icon
                                ></span>
                                <span v-if="forecastVisible"
                                    >Hide Forecast</span
                                >
                                <span v-else>Forecast</span>
                            </b-btn>
                        </div>
                        <b-collapse
                            :id="'entryForecast-' + index + '-' + datekey"
                            v-model="forecastVisible"
                        >
                            <b-row>
                                <b-col>
                                    <div data-testid="forecastDisplayName">
                                        <strong>Immunization: </strong>
                                        {{
                                            entry.immunization.forecast
                                                .displayName
                                        }}
                                    </div>
                                    <div data-testid="forecastDueDate">
                                        <strong>Due Date: </strong>
                                        {{
                                            entry.immunization.forecast.dueDate
                                        }}
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
                                    Please follow directions from your COVID
                                    vaccine provider for information on COVID-19
                                    2nd dose. For information on recommended
                                    immunizations, please visit
                                    <a
                                        href="https://immunizebc.ca/"
                                        target="blank_"
                                        >https://immunizebc.ca/</a
                                    >
                                    or contact your local Public Health Unit.
                                </p>
                            </div>
                        </b-collapse>
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

.card-button {
    display: block;
    color: $medium_background;
    font-size: 3.5em;
}
</style>
