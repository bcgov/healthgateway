<script lang="ts">
import { faSearch, IconDefinition } from "@fortawesome/free-solid-svg-icons";
import Vue from "vue";
import { Component, Watch } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import ErrorCardComponent from "@/components/errorCard.vue";
import LoadingComponent from "@/components/loading.vue";
import CovidModalComponent from "@/components/modal/covid.vue";
import ImmunizationCardComponent from "@/components/modal/immunizationCard.vue";
import NoteEditComponent from "@/components/modal/noteEdit.vue";
import ProtectiveWordComponent from "@/components/modal/protectiveWord.vue";
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
import container from "@/plugins/inversify.config";
import { ILogger } from "@/services/interfaces";

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
        ImmunizationCard: ImmunizationCardComponent,
    },
})
export default class TimelineView extends Vue {
    @Action("setKeyword", { namespace: "timeline" })
    setKeyword!: (keyword: string) => void;

    @Action("getPatientData", { namespace: "user" })
    getPatientData!: (params: { hdid: string }) => Promise<void>;

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

    @Getter("isLoading", { namespace: "medication" })
    isMedicationLoading!: boolean;

    @Getter("isLoading", { namespace: "medication" })
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
    private whenImmunizationIsDeferred(isDeferred: boolean) {
        if (isDeferred) {
            this.showImmunizationAlert = true;
        }
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

    private showImmunizationAlert = false;

    private filterText = "";

    private isPacificTime = false;

    private logger!: ILogger;

    private readonly alertExpirySeconds = 5;

    private get unverifiedEmail(): boolean {
        return !this.user.verifiedEmail && this.user.hasEmail;
    }

    private get unverifiedSMS(): boolean {
        return !this.user.verifiedSMS && this.user.hasSMS;
    }

    private get hasNewTermsOfService(): boolean {
        return this.user.hasTermsOfServiceUpdated;
    }

    private get searchIcon(): IconDefinition {
        return faSearch;
    }

    private get isLoading(): boolean {
        return (
            this.isMedicationRequestLoading ||
            this.isMedicationLoading ||
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
            this.getPatientData({ hdid: this.user.hdid }),
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
    <div>
        <LoadingComponent v-if="isLoading" :is-custom="true"></LoadingComponent>
        <b-row class="my-2 fluid">
            <b-col id="timeline" class="col-12 col-lg-9 column-wrapper">
                <div class="px-2">
                    <b-alert
                        :show="hasNewTermsOfService"
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
                        :show="unverifiedEmail || unverifiedSMS"
                        dismissible
                        variant="info"
                        class="no-print"
                    >
                        <h4>Please complete your profile</h4>
                        <span>
                            Your email or cell phone number have not been
                            verified. To complete your profile and receive
                            notifications from the Health Gateway, visit the
                            <router-link
                                id="profilePageLink"
                                variant="primary"
                                to="/profile"
                                >Profile Page</router-link
                            >
                            <span>.</span>
                        </span>
                    </b-alert>
                    <b-alert
                        :show="!isPacificTime"
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
                        :show="
                            showImmunizationAlert && immunizationIsDeferred
                                ? alertExpirySeconds
                                : false
                        "
                        dismissible
                        variant="info"
                        class="no-print"
                    >
                        <h4 data-testid="immunizationLoading">
                            Still searching for immunization records
                        </h4>
                    </b-alert>
                    <b-alert
                        :show="
                            showImmunizationAlert && !immunizationIsDeferred
                                ? alertExpirySeconds
                                : false
                        "
                        dismissible
                        variant="info"
                        class="no-print"
                    >
                        <span
                            v-if="patientImmunizations.length > 0"
                            data-testid="immunizationReady"
                        >
                            <h4>
                                Additional immunization records found. Loading
                                into timeline
                            </h4>
                        </span>
                        <span v-else data-testid="immunizationEmpty">
                            <h4>No additional records found</h4>
                        </span>
                    </b-alert>
                </div>

                <div id="pageTitle" class="px-2">
                    <h1 id="subject">Health Care Timeline</h1>
                    <hr class="mb-0" />
                </div>
                <div
                    class="sticky-top sticky-offset px-2"
                    :class="{ 'header-offset': isHeaderShown }"
                >
                    <b-row class="no-print justify-content-between">
                        <b-col>
                            <div class="form-group has-filter">
                                <font-awesome-icon
                                    :icon="searchIcon"
                                    class="form-control-feedback"
                                    fixed-width
                                ></font-awesome-icon>
                                <b-form-input
                                    v-model="filterText"
                                    data-testid="filterTextInput"
                                    type="text"
                                    placeholder=""
                                    maxlength="50"
                                    debounce="250"
                                ></b-form-input>
                            </div>
                        </b-col>
                        <b-col v-if="!isLoading" class="col-auto pl-2">
                            <Filters />
                        </b-col>
                    </b-row>
                </div>
                <LinearTimeline
                    v-show="isLinearView && !isLoading"
                    :timeline-entries="filteredTimelineEntries"
                    :total-entries="getTotalCount()"
                >
                </LinearTimeline>
                <CalendarTimeline
                    v-show="!isLinearView && !isLoading"
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
        <CovidModalComponent :is-loading="isLoading" />
        <ProtectiveWordComponent :is-loading="isLoading" />
        <NoteEditComponent :is-loading="isLoading" />
        <EntryDetailsComponent :is-loading="isLoading" />
        <ImmunizationCard />
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
}

.column-wrapper {
    border: 1px;
}

#pageTitle {
    color: $primary;

    hr {
        border-top: 2px solid $primary;
    }

    h1 {
        @media (max-width: 575px) {
            font-size: 2em !important;
        }
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

.has-filter .form-control {
    padding-left: 2.375rem;
}

.has-filter .form-control-feedback {
    position: absolute;
    z-index: 2;
    display: block;
    width: 2.375rem;
    height: 2.375rem;
    line-height: 2.375rem;
    text-align: center;
    pointer-events: none;
    color: #aaa;
    padding: 12px;
}

.btn-light {
    border-color: $primary;
    color: $primary;
}

.z-index-large {
    z-index: 50;
}

.sticky-top {
    z-index: 49 !important;
}
</style>
