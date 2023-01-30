<script lang="ts">
import Vue from "vue";
import { Component, Prop, Ref } from "vue-property-decorator";
import { Getter } from "vuex-class";

import MessageModalComponent from "@/components/modal/MessageModalComponent.vue";
import { EntryType, entryTypeMap } from "@/constants/entryType";
import { DateWrapper } from "@/models/dateWrapper";
import HospitalVisitTimelineEntry from "@/models/hospitalVisitTimelineEntry";
import User from "@/models/user";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger } from "@/services/interfaces";

import EntrycardTimelineComponent from "./EntrycardTimelineComponent.vue";

// eslint-disable-next-line @typescript-eslint/no-explicit-any
const options: any = {
    components: {
        EntryCard: EntrycardTimelineComponent,
        MessageModalComponent,
    },
};

@Component(options)
export default class HospitalVisitTimelineComponent extends Vue {
    @Prop()
    entry!: HospitalVisitTimelineEntry;

    @Prop()
    index!: number;

    @Prop()
    datekey!: string;

    @Prop()
    isMobileDetails!: boolean;

    @Getter("user", { namespace: "user" })
    user!: User;

    @Ref("messageModal")
    readonly messageModal!: MessageModalComponent;

    private logger!: ILogger;

    private created(): void {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
    }

    public get entryIcon(): string | undefined {
        return entryTypeMap.get(EntryType.HospitalVisit)?.icon;
    }

    public formatDate(date: DateWrapper): string {
        return date.format("yyyy-MMM-dd, t");
    }
}
</script>

<template>
    <EntryCard
        :card-id="index + '-' + datekey"
        :entry-icon="entryIcon"
        :title="entry.facility"
        :subtitle="entry.visitType"
        :entry="entry"
        :is-mobile-details="isMobileDetails"
    >
        <div slot="details-body">
            <div class="my-2">
                <div data-testid="hospital-visit-location">
                    <strong>Location: </strong>
                    <span>{{ entry.facility }}</span>
                    <hg-button
                        v-if="entry.facility"
                        :id="`hospital-visit-location-${index}-${datekey}`"
                        aria-label="Info"
                        href="#"
                        variant="link"
                        data-testid="hospital-visit-location-info-button"
                        class="shadow-none align-baseline p-0 ml-1"
                    >
                        <hg-icon icon="info-circle" size="small" />
                    </hg-button>
                    <b-popover
                        v-if="entry.facility"
                        :target="`hospital-visit-location-${index}-${datekey}`"
                        triggers="hover focus"
                        placement="topright"
                        boundary="viewport"
                        data-testid="hospital-visit-location-info-popover"
                    >
                        <span>
                            Virtual visits show your provider's location.
                        </span>
                    </b-popover>
                </div>
                <div data-testid="hospital-visit-provider">
                    <strong>Provider: </strong>
                    <span>{{ entry.provider }}</span>
                    <hg-button
                        v-if="entry.facility"
                        :id="`hospital-visit-provider-${index}-${datekey}`"
                        aria-label="Info"
                        href="#"
                        variant="link"
                        data-testid="hospital-visit-provider-info-button"
                        class="shadow-none align-baseline p-0 ml-1"
                    >
                        <hg-icon icon="info-circle" size="small" />
                    </hg-button>
                    <b-popover
                        v-if="entry.facility"
                        :target="`hospital-visit-provider-${index}-${datekey}`"
                        triggers="hover focus"
                        placement="topright"
                        boundary="viewport"
                        data-testid="hospital-visit-provider-info-popover"
                    >
                        <span>
                            Inpatient visits only show the first attending
                            physician.
                        </span>
                    </b-popover>
                </div>
                <div data-testid="hospital-visit-date">
                    <strong>Visit Date: </strong>
                    <span
                        data-testid="laboratory-collection-date-value"
                        class="text-nowrap"
                    >
                        {{ formatDate(entry.admitDateTime) }}
                    </span>
                    <hg-button
                        v-if="entry.outpatient"
                        :id="`hospital-visit-date-${index}-${datekey}`"
                        aria-label="Info"
                        href="#"
                        variant="link"
                        data-testid="hospital-visit-date-info-button"
                        class="shadow-none align-baseline p-0 ml-1"
                    >
                        <hg-icon icon="info-circle" size="small" />
                    </hg-button>
                    <b-popover
                        v-if="entry.outpatient"
                        :target="`hospital-visit-date-${index}-${datekey}`"
                        triggers="hover focus"
                        placement="topright"
                        boundary="viewport"
                        data-testid="hospital-visit-date-info-popover"
                    >
                        <span>
                            Outpatient visits may only show the first in a
                            series of dates.
                        </span>
                    </b-popover>
                </div>
                <div data-testid="hospital-visit-discharge-date">
                    <strong>Discharge Date: </strong>
                    <span
                        v-if="entry.endDateTime !== undefined"
                        class="text-nowrap"
                    >
                        {{ formatDate(entry.endDateTime) }}
                    </span>
                    <span v-else>not available</span>
                </div>
            </div>
        </div>
    </EntryCard>
</template>
