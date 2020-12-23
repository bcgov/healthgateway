<script lang="ts">
import Vue from "vue";
import ImmunizationTimelineEntry from "@/models/immunizationTimelineEntry";
import { Component, Prop } from "vue-property-decorator";
import CommentSectionComponent from "@/components/timeline/commentSection.vue";

import { IconDefinition, faSyringe } from "@fortawesome/free-solid-svg-icons";

@Component({
    components: {
        CommentSection: CommentSectionComponent,
    },
})
export default class ImmunizationTimelineComponent extends Vue {
    @Prop() entry!: ImmunizationTimelineEntry;
    @Prop() index!: number;
    @Prop() datekey!: string;

    private get entryIcon(): IconDefinition {
        return faSyringe;
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
            <b-col class="entryTitle" data-testid="immunizationTitle">
                {{ entry.immunization.name }}
            </b-col>
        </b-row>
        <b-row
            v-if="entry.immunization.immunizationAgents.lenght > 0"
            class="my-2"
        >
            <b-col class="leftPane"></b-col>
            <b-col>
                <b-row>
                    <b-col data-testid="immunizationProductTitle">
                        <strong> Product </strong>
                    </b-col>
                    <b-col data-testid="immunizationAgentName">
                        <strong> Immunizing agent </strong>
                    </b-col>
                    <b-col data-testid="immunizationProviderTitle">
                        <strong> Provider/Clinic </strong>
                    </b-col>
                    <b-col data-testid="immunizationLotTitle">
                        <strong> Lot Number </strong>
                    </b-col>
                    <b-col data-testid="immunizationStatus">
                        <strong> Status </strong>
                    </b-col>
                </b-row>
            </b-col>
        </b-row>
        <b-row
            v-for="agent in entry.immunization.immunizationAgents"
            :key="agent.code"
            class="my-2"
        >
            <b-col class="leftPane"></b-col>
            <b-col>
                <b-row>
                    <b-col data-testid="immunizationProductName">
                        {{ agent.productName }}
                    </b-col>
                    <b-col data-testid="immunizationAgentName">
                        {{ agent.name }}
                    </b-col>
                    <b-col data-testid="immunizationProviderName">
                        {{ entry.immunization.providerOrClinic }}
                    </b-col>
                    <b-col data-testid="immunizationLotNumber">
                        {{ agent.lotNumber }}
                    </b-col>
                    <b-col data-testid="immunizationStatus">
                        {{ entry.immunization.status }}
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
    border-radius: 0px $radius $radius 0px;
}

.icon {
    background-color: $primary;
    color: white;
    text-align: center;
    padding: 10px 0;
    border-radius: $radius 0px 0px $radius;
}

.leftPane {
    width: 60px;
    max-width: 60px;
}

.detailsButton {
    padding: 0px;
}

.detailSection {
    margin-top: 15px;
}

.collapsed > .when-opened,
:not(.collapsed) > .when-closed {
    display: none;
}
</style>
