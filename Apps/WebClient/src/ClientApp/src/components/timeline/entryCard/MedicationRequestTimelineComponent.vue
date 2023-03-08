<script lang="ts">
import Vue from "vue";
import { Component, Prop } from "vue-property-decorator";

import DateTimeFormat from "@/constants/dateTimeFormat";
import { EntryType, entryTypeMap } from "@/constants/entryType";
import { DateWrapper } from "@/models/dateWrapper";
import MedicationRequestTimelineEntry from "@/models/medicationRequestTimelineEntry";

import EntrycardTimelineComponent from "./EntrycardTimelineComponent.vue";

// eslint-disable-next-line @typescript-eslint/no-explicit-any
const options: any = {
    components: {
        EntryCard: EntrycardTimelineComponent,
    },
};

@Component(options)
export default class MedicationRequestTimelineComponent extends Vue {
    @Prop() entry!: MedicationRequestTimelineEntry;
    @Prop() index!: number;
    @Prop() datekey!: string;
    @Prop() isMobileDetails!: boolean;

    @Prop({ default: false })
    commentsAreEnabled!: boolean;

    private get entryIcon(): string | undefined {
        return entryTypeMap.get(EntryType.SpecialAuthorityRequest)?.icon;
    }

    private formatDate(dateValue: string): string {
        return DateWrapper.format(dateValue, DateTimeFormat.formatDateString);
    }
}
</script>

<template>
    <EntryCard
        :card-id="index + '-' + datekey"
        :entry-icon="entryIcon"
        :title="entry.drugName"
        :subtitle="'Status: ' + entry.requestStatus"
        :entry="entry"
        :is-mobile-details="isMobileDetails"
        :allow-comment="commentsAreEnabled"
    >
        <b-row slot="details-body" class="justify-content-between">
            <b-col>
                <div>
                    <div class="detailSection">
                        <div data-testid="medicationPractitioner">
                            <strong>Prescriber Name:</strong>
                            {{
                                entry.prescriberFirstName +
                                " " +
                                entry.prescriberLastName
                            }}
                        </div>
                    </div>
                    <div class="detailSection">
                        <div>
                            <strong>Effective Date:</strong>
                            <span class="text-nowrap">
                                {{ formatDate(entry.effectiveDate) }}
                            </span>
                        </div>
                        <div>
                            <strong>Expiry Date:</strong>
                            <span class="text-nowrap">
                                {{ formatDate(entry.expiryDate) }}
                            </span>
                        </div>
                        <div>
                            <strong>Reference Number:</strong>
                            {{ entry.referenceNumber }}
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
    padding: 0px;
    margin: 0px;
}

.detailSection {
    margin-top: 15px;
}
</style>
