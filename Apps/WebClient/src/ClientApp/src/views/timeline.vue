<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faSearch } from "@fortawesome/free-solid-svg-icons";
import Vue from "vue";
import { Component, Watch } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import ErrorCardComponent from "@/components/errorCard.vue";
import LoadingComponent from "@/components/loading.vue";
import CovidModalComponent from "@/components/modal/covid.vue";
import NoteEditComponent from "@/components/modal/noteEdit.vue";
import ProtectiveWordComponent from "@/components/modal/protectiveWord.vue";
import ResourceCentreComponent from "@/components/resourceCentre.vue";
import CalendarTimelineComponent from "@/components/timeline/calendarTimeline.vue";
import EntryDetailsComponent from "@/components/timeline/entryCard/entryDetails.vue";
import FilterComponent from "@/components/timeline/filters.vue";
import LinearTimelineComponent from "@/components/timeline/linearTimeline.vue";
import { DateWrapper } from "@/models/dateWrapper";
import Encounter from "@/models/encounter";
import EncounterTimelineEntry from "@/models/encounterTimelineEntry";
import { ImmunizationEvent } from "@/models/immunizationModel";
import ImmunizationTimelineEntry from "@/models/immunizationTimelineEntry";
import { LaboratoryOrder } from "@/models/laboratory";
import LaboratoryTimelineEntry from "@/models/laboratoryTimelineEntry";
import MedicationRequest from "@/models/MedicationRequest";
import MedicationRequestTimelineEntry from "@/models/medicationRequestTimelineEntry";
import MedicationStatementHistory from "@/models/medicationStatementHistory";
import MedicationTimelineEntry from "@/models/medicationTimelineEntry";
import NoteTimelineEntry from "@/models/noteTimelineEntry";
import TimelineEntry from "@/models/timelineEntry";
import TimelineFilter from "@/models/timelineFilter";
import User from "@/models/user";
import { UserComment } from "@/models/userComment";
import UserNote from "@/models/userNote";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import { ILogger } from "@/services/interfaces";

library.add(faSearch);

@Component({
    components: {
        LoadingComponent,
        ProtectiveWordComponent,
        CovidModalComponent,
        NoteEditComponent,
        EntryDetailsComponent,
        LinearTimeline: LinearTimelineComponent,
        CalendarTimeline: CalendarTimelineComponent,
        ErrorCard: ErrorCardComponent,
        Filters: FilterComponent,
        "resource-centre": ResourceCentreComponent,
    },
})
export default class TimelineView extends Vue {
    @Action("setKeyword", { namespace: "timeline" })
    setKeyword!: (keyword: string) => void;

    @Action("getPatientData", { namespace: "user" })
    getPatientData!: () => Promise<void>;

    @Action("retrieve", { namespace: "immunization" })
    retrieveImmunizations!: (params: { hdid: string }) => Promise<void>;

    @Action("retrieve", { namespace: "encounter" })
    retrieveEncounters!: (params: { hdid: string }) => Promise<void>;

    @Action("retrieve", { namespace: "note" })
    retrieveNotes!: (params: { hdid: string }) => Promise<void>;

    @Action("retrieve", { namespace: "laboratory" })
    retrieveLaboratory!: (params: { hdid: string }) => Promise<void>;

    @Action("retrieveMedicationStatements", { namespace: "medication" })
    retrieveMedications!: (params: {
        hdid: string;
        protectiveWord?: string;
    }) => Promise<void>;

    @Action("retrieveMedicationRequests", { namespace: "medication" })
    retrieveMedicationRequests!: (params: { hdid: string }) => Promise<void>;

    @Action("retrieve", { namespace: "comment" })
    retrieveComments!: (params: { hdid: string }) => Promise<void>;

    @Getter("isMedicationStatementLoading", { namespace: "medication" })
    isMedicationStatementLoading!: boolean;

    @Getter("isMedicationRequestLoading", { namespace: "medication" })
    isMedicationRequestLoading!: boolean;

    @Getter("isLoading", { namespace: "comment" })
    isCommentLoading!: boolean;

    @Getter("isLoading", { namespace: "laboratory" })
    isLaboratoryLoading!: boolean;

    @Getter("isLoading", { namespace: "encounter" })
    isEncounterLoading!: boolean;

    @Getter("isLoading", { namespace: "immunization" })
    isImmunizationLoading!: boolean;

    @Getter("isLoading", { namespace: "note" })
    isNoteLoading!: boolean;

    @Getter("isDeferredLoad", { namespace: "immunization" })
    immunizationIsDeferred!: boolean;

    @Getter("immunizations", { namespace: "immunization" })
    patientImmunizations!: ImmunizationEvent[];

    @Getter("patientEncounters", { namespace: "encounter" })
    patientEncounters!: Encounter[];

    @Getter("medicationStatements", { namespace: "medication" })
    medicationStatements!: MedicationStatementHistory[];

    @Getter("medicationRequests", { namespace: "medication" })
    medicationRequests!: MedicationRequest[];

    @Getter("laboratoryOrders", { namespace: "laboratory" })
    laboratoryOrders!: LaboratoryOrder[];

    @Getter("notes", { namespace: "note" })
    userNotes!: UserNote[];

    @Getter("getEntryComments", { namespace: "comment" })
    getEntryComments!: (entyId: string) => UserComment[] | null;

    @Getter("filter", { namespace: "timeline" })
    filter!: TimelineFilter;

    @Getter("keyword", { namespace: "timeline" })
    readonly keyword!: string;

    @Getter("isLinearView", { namespace: "timeline" })
    readonly isLinearView!: boolean;

    @Getter("user", { namespace: "user" }) user!: User;

    @Getter("isHeaderShown", { namespace: "navbar" }) isHeaderShown!: boolean;

    @Watch("filterText")
    private onFilterTextChanged() {
        this.setKeyword(this.filterText);
    }

    @Watch("immunizationIsDeferred")
    private whenImmunizationIsDeferred() {
        this.dismissImmunizationBannerCountdown =
            this.dismissImmunizationBannerSeconds;
    }

    @Watch("isLinearView")
    private onIsLinearView() {
        if (this.isLinearView) {
            window.location.hash = "linear";
        } else {
            window.location.hash = "calendar";
        }
    }

    private get timelineEntries(): TimelineEntry[] {
        if (this.isLoading) {
            return [];
        }

        this.logger.debug("Updating timeline Entries");

        let timelineEntries = [];
        // Add the medication request entries to the timeline list
        for (let medicationRequest of this.medicationRequests) {
            timelineEntries.push(
                new MedicationRequestTimelineEntry(
                    medicationRequest,
                    this.getEntryComments
                )
            );
        }

        // Add the medication entries to the timeline list
        for (let medication of this.medicationStatements) {
            timelineEntries.push(
                new MedicationTimelineEntry(medication, this.getEntryComments)
            );
        }

        // Add the Laboratory entries to the timeline list
        for (let order of this.laboratoryOrders) {
            timelineEntries.push(
                new LaboratoryTimelineEntry(order, this.getEntryComments)
            );
        }

        // Add the Encounter entries to the timeline list
        for (let encounter of this.patientEncounters) {
            timelineEntries.push(
                new EncounterTimelineEntry(encounter, this.getEntryComments)
            );
        }

        // Add the Note entries to the timeline list
        for (let note of this.userNotes) {
            timelineEntries.push(new NoteTimelineEntry(note));
        }

        // Add the immunization entries to the timeline list
        if (!this.immunizationIsDeferred) {
            for (let immunization of this.patientImmunizations) {
                timelineEntries.push(
                    new ImmunizationTimelineEntry(immunization)
                );
            }
        }

        timelineEntries = this.sortEntries(timelineEntries);
        return timelineEntries;
    }

    public get filteredTimelineEntries(): TimelineEntry[] {
        let filteredEntries = [];

        if (this.keyword !== "" || this.filter.hasActiveFilter()) {
            filteredEntries = this.timelineEntries.filter((entry) =>
                entry.filterApplies(this.keyword, this.filter)
            );
        } else {
            filteredEntries = this.timelineEntries;
        }

        return filteredEntries;
    }

    private filterText = "";

    private isPacificTime = false;

    private logger!: ILogger;

    private readonly dismissImmunizationBannerSeconds = 5;
    private dismissImmunizationBannerCountdown = 0;

    private get unverifiedEmail(): boolean {
        return !this.user.verifiedEmail && this.user.hasEmail;
    }

    private get unverifiedSMS(): boolean {
        return !this.user.verifiedSMS && this.user.hasSMS;
    }

    private get hasNewTermsOfService(): boolean {
        return this.user.hasTermsOfServiceUpdated;
    }

    private get isLoading(): boolean {
        return (
            this.isMedicationRequestLoading ||
            this.isMedicationStatementLoading ||
            this.isImmunizationLoading ||
            this.isLaboratoryLoading ||
            this.isEncounterLoading ||
            this.isNoteLoading ||
            this.isCommentLoading
        );
    }

    private created() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        this.fetchTimelineData();
    }

    private mounted() {
        this.filterText = this.keyword;

        if (new DateWrapper().isInDST()) {
            !this.checkTimezone(true)
                ? (this.isPacificTime = false)
                : (this.isPacificTime = true);
        } else {
            !this.checkTimezone(false)
                ? (this.isPacificTime = false)
                : (this.isPacificTime = true);
        }
    }

    private checkTimezone(isDST: boolean): boolean {
        if (isDST) {
            return new Date().getTimezoneOffset() / 60 === 7;
        } else {
            return new Date().getTimezoneOffset() / 60 === 8;
        }
    }

    private fetchTimelineData() {
        Promise.all([
            this.getPatientData(),
            this.retrieveMedications({ hdid: this.user.hdid }),
            this.retrieveMedicationRequests({ hdid: this.user.hdid }),
            this.retrieveImmunizations({ hdid: this.user.hdid }),
            this.retrieveLaboratory({ hdid: this.user.hdid }),
            this.retrieveEncounters({ hdid: this.user.hdid }),
            this.retrieveNotes({ hdid: this.user.hdid }),
            this.retrieveComments({ hdid: this.user.hdid }),
        ]).catch((err) => {
            this.logger.error(`Error loading timeline data: ${err}`);
        });
    }

    private getTotalCount(): number {
        return this.timelineEntries.length;
    }

    private sortEntries(timelineEntries: TimelineEntry[]): TimelineEntry[] {
        return timelineEntries.sort((a, b) =>
            a.date.isAfter(b.date) ? -1 : a.date.isBefore(b.date) ? 1 : 0
        );
    }
}
</script>

<template>
    <div class="flex-grow-1 d-flex flex-column">
        <LoadingComponent
            :is-custom="true"
            :is-loading="isLoading"
        ></LoadingComponent>
        <b-row class="my-2 fluid">
            <b-col id="timeline" class="col-12 col-lg-9 column-wrapper">
                <div class="px-2">
                    <b-alert
                        v-if="hasNewTermsOfService"
                        show
                        dismissible
                        variant="info"
                        class="no-print"
                    >
                        <h4>Updated Terms of Service</h4>
                        <span>
                            The Terms of Service have been updated since your
                            last login. You can review them
                            <router-link
                                id="termsOfServiceLink"
                                variant="primary"
                                to="/termsOfService"
                            >
                                here</router-link
                            >.
                        </span>
                    </b-alert>
                    <b-alert
                        v-if="unverifiedEmail || unverifiedSMS"
                        id="incomplete-profile-banner"
                        show
                        dismissible
                        variant="info"
                        class="no-print"
                    >
                        <h4>Verify Contact Information</h4>
                        <span>
                            Your email or cell phone number has not been
                            verified. You can use the Health Gateway without
                            verified contact information, however, you will not
                            receive notifications. Visit the
                            <router-link
                                id="profilePageLink"
                                variant="primary"
                                to="/profile"
                                >Profile Page</router-link
                            >
                            to complete your verification.
                        </span>
                    </b-alert>
                    <b-alert
                        v-if="!isPacificTime"
                        show
                        dismissible
                        variant="info"
                        class="no-print"
                    >
                        <h4>Looks like you're in a different timezone.</h4>
                        <span>
                            Heads up: your health records are recorded and
                            displayed in Pacific Time.
                        </span>
                    </b-alert>
                    <b-alert
                        :show="dismissImmunizationBannerCountdown"
                        dismissible
                        variant="info"
                        class="no-print"
                        @dismissed="dismissImmunizationBannerCountdown = 0"
                    >
                        <h4
                            v-if="immunizationIsDeferred"
                            data-testid="immunizationLoading"
                        >
                            Still searching for immunization records
                        </h4>
                        <h4
                            v-else-if="patientImmunizations.length > 0"
                            data-testid="immunizationReady"
                        >
                            Additional immunization records found. Loading into
                            timeline
                        </h4>
                        <h4 v-else data-testid="immunizationEmpty">
                            No additional records found
                        </h4>
                    </b-alert>
                </div>

                <div id="pageTitle" class="px-2">
                    <h1 id="subject">Timeline</h1>
                    <hr class="mb-0" />
                </div>
                <div
                    class="sticky-top sticky-offset px-2"
                    :class="{ 'header-offset': isHeaderShown }"
                >
                    <b-row class="no-print justify-content-between">
                        <b-col>
                            <div class="form-group has-filter">
                                <hg-icon
                                    icon="search"
                                    size="medium"
                                    class="form-control-feedback"
                                />
                                <b-input-group>
                                    <b-form-input
                                        v-model="filterText"
                                        data-testid="filterTextInput"
                                        type="text"
                                        placeholder=""
                                        maxlength="50"
                                        debounce="250"
                                    ></b-form-input>
                                    <b-input-group-append>
                                        <hg-button
                                            v-show="filterText"
                                            data-testid="clearfilterTextBtn"
                                            variant="icon-input-light"
                                            @click="filterText = ''"
                                        >
                                            <hg-icon
                                                icon="times"
                                                size="medium"
                                                fixed-width
                                            />
                                        </hg-button>
                                    </b-input-group-append>
                                </b-input-group>
                            </div>
                        </b-col>
                        <b-col v-if="!isLoading" class="col-auto pl-2">
                            <Filters />
                        </b-col>
                    </b-row>
                </div>
                <LinearTimeline
                    v-if="isLinearView && !isLoading"
                    :timeline-entries="filteredTimelineEntries"
                    :total-entries="getTotalCount()"
                >
                </LinearTimeline>
                <CalendarTimeline
                    v-if="!isLinearView && !isLoading"
                    :timeline-entries="filteredTimelineEntries"
                    :total-entries="getTotalCount()"
                >
                </CalendarTimeline>
                <b-row v-if="isLoading">
                    <b-col>
                        <br />
                        <div class="px-2">
                            <content-placeholders>
                                <content-placeholders-heading :img="true" />
                                <content-placeholders-text :lines="3" />
                            </content-placeholders>
                            <br />
                            <br />
                            <content-placeholders>
                                <content-placeholders-heading :img="true" />
                                <content-placeholders-img />
                            </content-placeholders>
                        </div>
                    </b-col>
                </b-row>
            </b-col>
        </b-row>
        <resource-centre />
        <CovidModalComponent :is-loading="isLoading" />
        <ProtectiveWordComponent :is-loading="isLoading" />
        <NoteEditComponent :is-loading="isLoading" />
        <EntryDetailsComponent :is-loading="isLoading" />
    </div>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";
.row {
    margin: 0px;
    padding: 0px;
}

.col {
    margin: 0px;
    padding: 0px;
}
.sticky-top {
    transition: all 0.3s;
    z-index: 49 !important;
}

.column-wrapper {
    border: 1px;
}

#pageTitle {
    color: $primary;

    hr {
        border-top: 2px solid $primary;
    }
}

.form-group {
    margin-bottom: 0px;
}

.sticky-offset {
    padding-top: 1rem;
    background-color: white;
    z-index: 2;
    &.header-offset {
        top: $header-height;
    }
}

.has-filter {
    $icon-size: 1rem;
    $icon-size-padded: 2.375rem;
    $icon-padding: ($icon-size-padded - $icon-size) / 2;

    .form-control {
        padding-left: $icon-size-padded;
    }

    .form-control-feedback {
        position: absolute;
        z-index: 5;
        display: block;
        text-align: center;
        pointer-events: none;
        color: #aaa;
        padding: $icon-padding;
    }
}

.btn-light {
    border-color: $primary;
    color: $primary;
}

.z-index-large {
    z-index: 50;
}
</style>
