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
                                <strong> Immunizing agent </strong>
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
                                {{ entry.getDisplayFormat(agent.productName) }}
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
                                {{
                                    entry.getDisplayFormat(
                                        entry.immunization.providerOrClinic
                                    )
                                }}
                            </b-col>
                            <b-col
                                class="px-1"
                                data-testid="immunizationLotNumber"
                            >
                                {{ entry.getDisplayFormat(agent.lotNumber) }}
                            </b-col>
                        </b-row>
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
