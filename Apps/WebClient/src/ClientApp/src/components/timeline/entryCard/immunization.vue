<script lang="ts">
import { faSyringe, IconDefinition } from "@fortawesome/free-solid-svg-icons";
import Vue from "vue";
import { Component, Prop } from "vue-property-decorator";

import ImmunizationTimelineEntry from "@/models/immunizationTimelineEntry";

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
    private forecastVisible = false;

    private get entryIcon(): IconDefinition {
        return faSyringe;
    }

    private toggleDetails(): void {
        this.forecastVisible = !this.forecastVisible;
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
        <div slot="header-description">
            <strong>
                Status:
                <span data-testid="immunizationStatus">
                    {{ entry.immunization.status }}
                </span></strong
            >
        </div>

        <b-row slot="details-body" class="justify-content-between">
            <b-col>
                <b-row
                    v-for="agent in entry.immunization.immunizationAgents"
                    :key="agent.code"
                    class="my-2"
                >
                    <b-col>
                        <div class="detailSection">
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
                        </div>
                    </b-col>
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
                            <div class="detailSection">
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
                                                entry.immunization.forecast
                                                    .dueDate
                                            }}
                                        </div>
                                        <div data-testid="forecastStatus">
                                            <strong>Status: </strong>
                                            {{
                                                entry.immunization.forecast
                                                    .status
                                            }}
                                        </div>
                                    </b-col>
                                </b-row>
                            </div>
                            <div class="detailSection">
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
.col {
    padding: 0px;
    margin: 0px;
}
.row {
    padding: 0;
    margin: 0px;
}

.detailSection {
    margin-top: 15px;
}
</style>
