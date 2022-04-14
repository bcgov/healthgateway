<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faInfoCircle } from "@fortawesome/free-solid-svg-icons";
import Vue from "vue";
import { Component, Prop } from "vue-property-decorator";

import { EntryType, entryTypeMap } from "@/constants/entryType";
import EncounterTimelineEntry from "@/models/encounterTimelineEntry";

import EntrycardTimelineComponent from "./EntrycardTimelineComponent.vue";

library.add(faInfoCircle);

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

    private get entryIcon(): string | undefined {
        return entryTypeMap.get(EntryType.Encounter)?.icon;
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
                        class="infoIcon ml-2"
                        tabindex="0"
                    >
                        <hg-icon icon="info-circle" size="medium" />
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
    color: $aquaBlue;
}
</style>
