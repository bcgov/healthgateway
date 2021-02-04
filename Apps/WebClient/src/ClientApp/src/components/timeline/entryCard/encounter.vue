<script lang="ts">
import { faUserMd, IconDefinition } from "@fortawesome/free-solid-svg-icons";
import Vue from "vue";
import { Component, Prop } from "vue-property-decorator";

import EncounterTimelineEntry from "@/models/encounterTimelineEntry";
import PhoneUtil from "@/utility/phoneUtil";

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

    private get entryIcon(): IconDefinition {
        return faUserMd;
    }

    private formatPhone(phoneNumber: string): string {
        return PhoneUtil.formatPhone(phoneNumber);
    }
}
</script>

<template>
    <EntryCard
        :card-id="index + '-' + datekey"
        :entry-icon="entryIcon"
        :title="entry.practitionerName"
        :entry="entry"
    >
        <div slot="header-description">
            {{ entry.specialtyDescription }}
        </div>

        <b-row slot="details-body">
            <b-col>
                <div class="detailSection">
                    <div data-testid="encounterClinicLabel">
                        <strong>Clinic/Practitioner:</strong>
                    </div>
                    <div data-testid="encounterClinicName">
                        {{ entry.clinic.name }}
                    </div>
                    <div data-testid="encounterClinicAddress">
                        {{ entry.clinic.address }}
                    </div>
                    <div data-testid="encounterClinicPhone">
                        {{ formatPhone(entry.clinic.phoneNumber) }}
                    </div>
                </div>
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
