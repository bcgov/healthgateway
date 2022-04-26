<script lang="ts">
import Vue from "vue";
import { Component, Prop } from "vue-property-decorator";

import { EntryType, entryTypeMap } from "@/constants/entryType";
import MedicationTimelineEntry from "@/models/medicationTimelineEntry";
import PhoneUtil from "@/utility/phoneUtil";

import EntrycardTimelineComponent from "./EntrycardTimelineComponent.vue";

@Component({
    components: {
        EntryCard: EntrycardTimelineComponent,
    },
})
export default class MedicationTimelineComponent extends Vue {
    @Prop() entry!: MedicationTimelineEntry;
    @Prop() index!: number;
    @Prop() datekey!: string;
    @Prop() isMobileDetails!: boolean;

    private get entryIcon(): string | undefined {
        return entryTypeMap.get(EntryType.Medication)?.icon;
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
        :title="entry.medication.brandName"
        :subtitle="entry.medication.genericName"
        :entry="entry"
        :is-mobile-details="isMobileDetails"
    >
        <b-row slot="details-body" class="justify-content-between">
            <b-col>
                <div>
                    <div class="detailSection">
                        <div data-testid="medicationPractitioner">
                            <strong>Practitioner:</strong>
                            {{ entry.practitionerSurname }}
                        </div>
                    </div>
                    <div class="detailSection">
                        <div>
                            <strong>Quantity:</strong>
                            {{ entry.medication.quantity }}
                        </div>
                        <div>
                            <strong>Strength:</strong>
                            {{ entry.medication.strength }}
                            {{ entry.medication.strengthUnit }}
                        </div>
                        <div>
                            <strong>Form:</strong>
                            {{ entry.medication.form }}
                        </div>
                        <div>
                            <strong>Manufacturer:</strong>
                            {{ entry.medication.manufacturer }}
                        </div>
                    </div>
                    <div class="detailSection">
                        <strong
                            >{{
                                entry.medication.isPin ? "PIN" : "DIN"
                            }}:</strong
                        >
                        {{ entry.medication.din }}
                    </div>
                    <div class="detailSection">
                        <div>
                            <strong>Filled At:</strong>
                        </div>
                        <div>
                            {{ entry.pharmacy.name }}
                        </div>
                        <div>
                            <strong>Address:</strong>
                            {{ entry.pharmacy.address }}
                        </div>
                        <div v-if="entry.pharmacy.phoneNumber !== ''">
                            <strong>Phone number:</strong>
                            {{ formatPhone(entry.pharmacy.phoneNumber) }}
                        </div>
                        <div v-if="entry.pharmacy.faxNumber !== ''">
                            <strong>Fax:</strong>
                            {{ formatPhone(entry.pharmacy.faxNumber) }}
                        </div>

                        <div class="detailSection border border-dark p-2 small">
                            <div>
                                <strong>Directions for Use:</strong>
                            </div>
                            <div class="pt-2">
                                {{ entry.directions }}
                            </div>
                        </div>
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
