<script lang="ts">
import { faSyringe, IconDefinition } from "@fortawesome/free-solid-svg-icons";
import Vue from "vue";
import { Component, Prop } from "vue-property-decorator";

import CommentSectionComponent from "@/components/timeline/commentSection.vue";
import ImmunizationTimelineEntry from "@/models/immunizationTimelineEntry";

@Component({
    components: {
        CommentSection: CommentSectionComponent,
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
    <b-col class="timelineCard">
        <b-row class="entryHeading">
            <b-col class="icon leftPane">
                <font-awesome-icon
                    :icon="entryIcon"
                    size="2x"
                ></font-awesome-icon>
            </b-col>
            <b-col class="entryTitle">
                <b-row class="justify-content-between">
                    <b-col cols="auto" data-testid="immunizationTitle">
                        <strong>{{ entry.immunization.name }}</strong>
                    </b-col>
                    <b-col cols="auto" class="text-muted">
                        <strong>
                            Status:
                            <span data-testid="immunizationStatus">
                                {{ entry.immunization.status }}
                            </span></strong
                        >
                    </b-col>
                </b-row>
            </b-col>
        </b-row>
        <b-row
            v-if="entry.immunization.immunizationAgents.length > 0"
            class="entryDetails mt-3"
        >
            <b-col>
                <b-row>
                    <b-col>
                        <b-row>
                            <b-col
                                class="pl-2 pr-1"
                                data-testid="immunizationProductTitle"
                            >
                                <strong> Product </strong>
                            </b-col>
                            <b-col
                                class="px-1"
                                data-testid="immunizationAgentNameTitle"
                            >
                                <strong> Immunizing Agent </strong>
                            </b-col>
                            <b-col
                                class="px-1"
                                data-testid="immunizationProviderTitle"
                            >
                                <strong> Provider / Clinic </strong>
                            </b-col>
                            <b-col
                                class="px-1"
                                data-testid="immunizationLotTitle"
                            >
                                <strong> Lot Number </strong>
                            </b-col>
                        </b-row>
                    </b-col>
                </b-row>
                <b-row
                    v-for="agent in entry.immunization.immunizationAgents"
                    :key="agent.code"
                    class="my-2"
                >
                    <b-col>
                        <b-row>
                            <b-col
                                class="pl-2 pr-1"
                                data-testid="immunizationProductName"
                            >
                                {{ agent.productName }}
                            </b-col>
                            <b-col
                                class="px-1"
                                data-testid="immunizationAgentName"
                            >
                                {{ agent.name }}
                            </b-col>
                            <b-col
                                class="px-1"
                                data-testid="immunizationProviderName"
                            >
                                {{ entry.immunization.providerOrClinic }}
                            </b-col>
                            <b-col
                                class="px-1"
                                data-testid="immunizationLotNumber"
                            >
                                {{ agent.lotNumber }}
                            </b-col>
                        </b-row>
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
                            :id="'entryDetails-' + index + '-' + datekey"
                            v-model="forecastVisible"
                        >
                            <div>
                                <div class="detailSection">
                                    <b-row>
                                        <b-col>
                                            <strong>Immunization</strong>
                                        </b-col>
                                        <b-col>
                                            <strong>Due Date</strong>
                                        </b-col>
                                        <b-col>
                                            <strong>Status</strong>
                                        </b-col>
                                    </b-row>
                                    <b-row>
                                        <b-col>
                                            <span
                                                data-testid="forecastDisplayName"
                                                >{{
                                                    entry.immunization.forecast
                                                        .displayName
                                                }}</span
                                            >
                                        </b-col>
                                        <b-col>
                                            <span
                                                data-testid="forecastDueDate"
                                                >{{
                                                    entry.immunization.forecast
                                                        .dueDate
                                                }}</span
                                            >
                                        </b-col>
                                        <b-col>
                                            <span
                                                data-testid="forecastStatus"
                                                >{{
                                                    entry.immunization.forecast
                                                        .status
                                                }}</span
                                            >
                                        </b-col>
                                    </b-row>
                                </div>

                                <div class="detailSection">
                                    <div>
                                        <br />
                                        <p
                                            data-testid="forecastFollowDirections"
                                        >
                                            Please follow directions from your
                                            COVID vaccine provider for
                                            information on COVID-19 2nd dose.
                                            For information on recommended
                                            immunizations, please visit
                                            <a
                                                href="https://immunizebc.ca/"
                                                target="blank_"
                                                >https://immunizebc.ca/</a
                                            >
                                            or contact your local Public Health
                                            Unit.
                                        </p>
                                    </div>
                                </div>
                            </div>
                        </b-collapse>
                    </b-col>
                </b-row>
            </b-col>
        </b-row>
    </b-col>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

$radius: 15px;

.timelineCard {
    border-radius: $radius $radius $radius $radius;
    border-color: $soft_background;
    border-style: solid;
    border-width: 2px;
}

.entryTitle {
    background-color: $soft_background;
    color: $primary;
    padding: 13px 15px;
    font-weight: bold;
    margin-right: -1px;
    border-radius: 0px $radius 0px 0px;
}

.icon {
    background-color: $primary;
    color: white;
    text-align: center;
    padding: 10px 0;
    border-radius: $radius 0px 0px 0px;
}

.leftPane {
    width: 60px;
    max-width: 60px;
}

.entryDetails {
    padding-left: 60px;
}

@media screen and (max-width: 600px) {
    .entryDetails {
        padding-left: 0px;
    }
}
</style>
