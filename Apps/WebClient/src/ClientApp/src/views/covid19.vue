<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faCheckCircle } from "@fortawesome/free-solid-svg-icons";
import Vue from "vue";
import { Component, Watch } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import LoadingComponent from "@/components/loading.vue";
import EntryDetailsComponent from "@/components/timeline/entryCard/entryDetails.vue";
import ImmunizationTimelineComponent from "@/components/timeline/entryCard/immunization.vue";
import EventBus, { EventMessageName } from "@/eventbus";
import type { WebClientConfiguration } from "@/models/configData";
import { ImmunizationEvent } from "@/models/immunizationModel";
import ImmunizationTimelineEntry from "@/models/immunizationTimelineEntry";
import PatientData from "@/models/patientData";
import TimelineEntry from "@/models/timelineEntry";
import User from "@/models/user";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import { ILogger } from "@/services/interfaces";
import SnowPlow from "@/utility/snowPlow";

library.add(faCheckCircle);

@Component({
    components: {
        "immunization-timeline": ImmunizationTimelineComponent,
        "entry-details": EntryDetailsComponent,
        loading: LoadingComponent,
    },
})
export default class Covid19View extends Vue {
    @Getter("webClient", { namespace: "config" })
    config!: WebClientConfiguration;

    @Getter("user", { namespace: "user" })
    user!: User;

    @Getter("patientData", { namespace: "user" })
    patientData!: PatientData;

    @Getter("isLoading", { namespace: "user" })
    isPatientLoading!: boolean;

    @Getter("isLoading", { namespace: "immunization" })
    isImmunizationLoading!: boolean;

    @Getter("isDeferredLoad", { namespace: "immunization" })
    immunizationIsDeferred!: boolean;

    @Getter("covidImmunizations", { namespace: "immunization" })
    covidImmunizations!: ImmunizationEvent[];

    @Action("getPatientData", { namespace: "user" })
    getPatientData!: () => Promise<void>;

    @Action("retrieve", { namespace: "immunization" })
    retrieveImmunizations!: (params: { hdid: string }) => Promise<void>;

    private logger!: ILogger;
    private eventBus = EventBus;
    private autoOpenCard = true;

    private get isLoading(): boolean {
        return (
            this.isPatientLoading ||
            this.isImmunizationLoading ||
            this.immunizationIsDeferred
        );
    }

    private get timelineEntries(): TimelineEntry[] {
        if (this.isLoading) {
            return [];
        }

        this.logger.debug("Updating COVID-19 Entries");

        let timelineEntries = [];

        // Add the COVID-19 entries to the timeline list
        for (let immunization of this.covidImmunizations) {
            timelineEntries.push(new ImmunizationTimelineEntry(immunization));
        }

        timelineEntries = this.sortEntries(timelineEntries);

        return timelineEntries;
    }

    private get covidCardDisabled(): boolean {
        return (
            this.isLoading ||
            this.patientData.hdid === undefined ||
            this.covidImmunizations.length === 0
        );
    }

    @Watch("isPatientLoading")
    @Watch("isImmunizationLoading")
    @Watch("immunizationIsDeferred")
    private onIsLoadingChanged() {
        if (!this.covidCardDisabled && this.autoOpenCard) {
            this.showCard();
            this.autoOpenCard = false;
        }
    }

    private sortEntries(timelineEntries: TimelineEntry[]): TimelineEntry[] {
        return timelineEntries.sort((a, b) =>
            a.date.isAfter(b.date) ? -1 : a.date.isBefore(b.date) ? 1 : 0
        );
    }

    private showCard(): void {
        SnowPlow.trackEvent({
            action: "view_card",
            text: "COVID Card",
        });
        this.eventBus.$emit(EventMessageName.TimelineCovidCard);
    }

    private fetchData() {
        Promise.all([
            this.getPatientData(),
            this.retrieveImmunizations({ hdid: this.user.hdid }),
        ]).catch((err) => {
            this.logger.error(`Error loading COVID-19 data: ${err}`);
        });
    }

    private created() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        this.fetchData();
    }
}
</script>

<template>
    <b-row no-gutters class="m-3 flex-grow-1">
        <b-col cols="12" lg="9">
            <loading :is-loading="isLoading" />
            <page-title title="COVID-19">
                <hg-button
                    :disabled="covidCardDisabled"
                    data-testid="covidcard-btn"
                    class="float-right"
                    variant="primary"
                    @click="showCard()"
                >
                    <hg-icon icon="check-circle" size="medium" class="mr-2" />
                    <span>BC Vaccine Card</span>
                </hg-button>
            </page-title>
            <div v-if="timelineEntries.length > 0" class="mx-md-n2">
                <immunization-timeline
                    v-for="(entry, index) in timelineEntries"
                    :key="entry.type + '-' + entry.id"
                    :datekey="entry.date.fromEpoch()"
                    :entry="entry"
                    :index="index"
                    data-testid="timelineCard"
                />
            </div>
            <div v-else class="text-center">
                <b-row>
                    <b-col>
                        <img
                            class="mx-auto d-block"
                            src="@/assets/images/timeline/empty-state.png"
                            width="200"
                            height="auto"
                            alt="..."
                        />
                    </b-col>
                </b-row>
                <b-row>
                    <b-col>
                        <p
                            class="text-center pt-2 noCovidImmunizationsText"
                            data-testid="noCovidImmunizationsText"
                        >
                            No records found
                        </p>
                    </b-col>
                </b-row>
            </div>
            <entry-details />
        </b-col>
    </b-row>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

.noCovidImmunizationsText {
    font-size: 1.5rem;
    color: #6c757d;
}
</style>
