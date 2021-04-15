<script lang="ts">
import {
    faInfo,
    faUserMd,
    IconDefinition,
} from "@fortawesome/free-solid-svg-icons";
import Vue from "vue";
import { Component, Prop } from "vue-property-decorator";

import EncounterTimelineEntry from "@/models/encounterTimelineEntry";

import EntrycardTimelineComponent from "./entrycard.vue";

@Component({
    components: {
        EntryCard: EntrycardTimelineComponent,
    },
})
export default class EncounterTimelineComponent extends Vue {
    @Prop() entry!: EncounterTimelineEntry;
    @Prop() index!: number;
    @Prop() datekey!: string;
    @Prop() isMobileDetails!: boolean;

    private get entryIcon(): IconDefinition {
        return faUserMd;
    }

    private get infoIcon(): IconDefinition {
        return faInfo;
    }
}
</script>

<template>
    <EntryCard
        :card-id="index + '-' + datekey"
        :entry-icon="entryIcon"
        :title="entry.specialtyDescription"
        :subtitle="entry.practitionerName"
        :entry="entry"
        :is-mobile-details="isMobileDetails"
    >
        <b-row slot="details-body">
            <b-col>
                <div class="d-inline-flex" data-testid="encounterClinicLabel">
                    <strong>Clinic/Practitioner:</strong>
                    <div
                        :id="`tooltip-info${index}-${datekey}`"
                        class="infoIcon ml-2 mt-1"
                        tabindex="0"
                    >
                        <font-awesome-icon :icon="infoIcon" />
                    </div>
                    <b-tooltip
                        variant="secondary"
                        :target="`tooltip-info${index}-${datekey}`"
                        placement="top"
                        triggers="hover focus"
                    >
                        Information is from the billing claim and may show a
                        different practitioner or clinic from the one you
                        visited. For more information, visit the
                        <router-link to="/faq">FAQ</router-link> page.
                    </b-tooltip>
                </div>
                <div data-testid="encounterClinicName">
                    {{ entry.clinic.name }}
                </div>
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
.infoIcon {
    background-color: $aquaBlue;
    color: $primary_text;
    text-align: center;
    border-radius: 50%;
    height: 15px;
    width: 15px;
    font-size: 0.5em;
}
</style>
