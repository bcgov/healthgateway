<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";
.column-wrapper {
    border: 1px;
}

#pageTitle {
    color: $primary;
}

#pageTitle hr {
    border-top: 2px solid $primary;
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

.view-selector {
    min-width: 170px;
    .btn-outline-primary {
        font-size: 1em;
        background-color: white;
    }
    .btn-outline-primary:focus {
        color: white;
        background-color: $primary;
    }
    .btn-outline-primary:hover {
        color: white;
        background-color: $primary;
    }
    .month-view-btn {
        border-radius: 5px 0px 0px 5px;
        border-right: 0px;
    }
    .list-view-btn {
        border-radius: 0px 5px 5px 0px;
    }
}

.z-index-large {
    z-index: 50;
}

.sticky-top {
    z-index: 49 !important;
}
.sticky-offset {
    padding-top: 1rem;
    background-color: white;
    z-index: 2;
}
</style>
<template>
    <div>
        <TimelineLoadingComponent v-if="isLoading"></TimelineLoadingComponent>
        <b-row class="my-3 fluid justify-content-md-center">
            <b-col id="timeline" class="col-12 col-lg-9 column-wrapper">
                <b-alert
                    :show="hasNewTermsOfService"
                    dismissible
                    variant="info"
                    class="no-print"
                >
                    <h4>Updated Terms of Service</h4>
                    <span>
                        The Terms of Service have been updated since your last
                        login. You can review them
                        <router-link
                            id="termsOfServiceLink"
                            variant="primary"
                            to="/termsOfService"
                        >
                            here </router-link
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
                        Your email or cell phone number have not been verified.
                        To complete your profile and receive notifications from
                        the Health Gateway, visit the
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
                        Heads up: your health records are recorded and displayed
                        in Pacific Time.
                    </span>
                </b-alert>

                <div id="pageTitle">
                    <h1 id="subject">Health Care Timeline</h1>
                    <hr class="mb-0" />
                </div>
                <div class="sticky-top sticky-offset">
                    <b-row class="no-print justify-content-between">
                        <b-col class="col">
                            <div class="form-group has-filter">
                                <font-awesome-icon
                                    :icon="searchIcon"
                                    class="form-control-feedback"
                                    fixed-width
                                ></font-awesome-icon>
                                <b-form-input
                                    v-model="filterText"
                                    type="text"
                                    placeholder=""
                                    maxlength="50"
                                    debounce="250"
                                ></b-form-input>
                            </div>
                        </b-col>
                        <b-col class="col-auto pl-0">
                            <Filters @filters-changed="filtersChanged" />
                        </b-col>
                    </b-row>
                </div>
                <b-row v-if="isAddingNote" class="pb-5">
                    <b-col>
                        <NoteTimelineComponent :is-add-mode="true" />
                    </b-col>
                </b-row>
                <LinearTimeline
                    v-show="isListView && !isLoading"
                    :timeline-entries="timelineEntries"
                    :is-visible="isListView"
                    :total-entries="getTotalCount()"
                    :filter-text="filterText"
                    :filter-types="filterTypes"
                >
                    <b-row
                        slot="month-list-toggle"
                        class="view-selector justify-content-end"
                    >
                        <b-col cols="auto" class="pr-0">
                            <b-btn
                                class="month-view-btn btn-outline-primary px-2 m-0"
                                :class="{ active: false }"
                                @click.stop="toggleMonthView"
                            >
                                Month
                            </b-btn>
                        </b-col>
                        <b-col cols="auto" class="pl-0">
                            <b-btn
                                class="list-view-btn btn-outline-primary px-2 m-0"
                                :class="{ active: true }"
                            >
                                List
                            </b-btn>
                        </b-col>
                    </b-row>
                </LinearTimeline>
                <CalendarTimeline
                    v-show="!isListView && !isLoading"
                    :timeline-entries="timelineEntries"
                    :is-visible="!isListView"
                    :total-entries="getTotalCount()"
                    :filter-text="filterText"
                    :filter-types="filterTypes"
                >
                    <b-row
                        slot="month-list-toggle"
                        class="view-selector justify-content-end"
                    >
                        <b-col cols="auto" class="pr-0">
                            <b-btn
                                class="month-view-btn btn-outline-primary px-2 m-0"
                                :class="{ active: true }"
                            >
                                Month
                            </b-btn>
                        </b-col>
                        <b-col cols="auto" class="pl-0">
                            <b-btn
                                class="list-view-btn btn-outline-primary px-2 m-0"
                                :class="{ active: false }"
                                @click.stop="toggleListView"
                            >
                                List
                            </b-btn>
                        </b-col>
                    </b-row>
                </CalendarTimeline>
                <b-row v-if="isLoading">
                    <b-col>
                        <content-placeholders>
                            <content-placeholders-text :lines="1" />
                        </content-placeholders>
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
        <CovidModalComponent
            ref="covidModal"
            :is-loading="isLoading"
            @submit="onCovidSubmit"
            @cancel="onCovidCancel"
        />
        <ProtectiveWordComponent
            ref="protectiveWordModal"
            :error="protectiveWordAttempts > 1"
            :is-loading="isLoading"
            @submit="onProtectiveWordSubmit"
            @cancel="onProtectiveWordCancel"
        />
    </div>
</template>

<script lang="ts">
import Vue from "vue";
import { Component, Ref, Watch } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";
import { Route } from "vue-router";
import EventBus from "@/eventbus";
import { WebClientConfiguration } from "@/models/configData";
import {
    ILogger,
    IImmunizationService,
    ILaboratoryService,
    IMedicationService,
    IEncounterService,
    IUserNoteService,
} from "@/services/interfaces";
import container from "@/plugins/inversify.config";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ResultType } from "@/constants/resulttype";
import User from "@/models/user";
import TimelineEntry, { EntryType } from "@/models/timelineEntry";
import MedicationTimelineEntry from "@/models/medicationTimelineEntry";
import ImmunizationTimelineEntry from "@/models/immunizationTimelineEntry";
import LaboratoryTimelineEntry from "@/models/laboratoryTimelineEntry";
import MedicationStatementHistory from "../models/medicationStatementHistory";
import NoteTimelineEntry from "@/models/noteTimelineEntry";
import UserNote from "@/models/userNote";
import RequestResult from "@/models/requestResult";
import { IconDefinition, faSearch } from "@fortawesome/free-solid-svg-icons";

import TimelineLoadingComponent from "@/components/timelineLoading.vue";
import ProtectiveWordComponent from "@/components/modal/protectiveWord.vue";
import CovidModalComponent from "@/components/modal/covid.vue";
import EntryCardTimelineComponent from "@/components/timeline/entrycard.vue";
import NoteTimelineComponent from "@/components/timeline/note.vue";
import {
    LaboratoryOrder,
    LaboratoryReport,
    LaboratoryResult,
} from "@/models/laboratory";
import LinearTimelineComponent from "@/components/timeline/linearTimeline.vue";
import CalendarTimelineComponent from "@/components/timeline/calendarTimeline.vue";
import ErrorCardComponent from "@/components/errorCard.vue";
import BannerError from "@/models/bannerError";
import ErrorTranslator from "@/utility/errorTranslator";
import EncounterTimelineEntry from "@/models/encounterTimelineEntry";
import FilterComponent from "@/components/timeline/filters.vue";
import { DateWrapper } from "@/models/dateWrapper";

const namespace: string = "user";

// Register the router hooks with their names
Component.registerHooks(["beforeRouteLeave"]);

@Component({
    components: {
        TimelineLoadingComponent,
        ProtectiveWordComponent,
        CovidModalComponent,
        EntryCardComponent: EntryCardTimelineComponent,
        NoteTimelineComponent,
        LinearTimeline: LinearTimelineComponent,
        CalendarTimeline: CalendarTimelineComponent,
        ErrorCard: ErrorCardComponent,
        Filters: FilterComponent,
    },
})
export default class TimelineView extends Vue {
    private logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    @Getter("user", { namespace }) user!: User;

    @Action("getOrders", { namespace: "laboratory" })
    getLaboratoryOrders!: (params: {
        hdid: string;
    }) => Promise<RequestResult<LaboratoryOrder[]>>;

    @Action("getMedicationStatements", { namespace: "medication" })
    getMedicationStatements!: (params: {
        hdid: string;
        protectiveWord?: string;
    }) => Promise<RequestResult<MedicationStatementHistory[]>>;

    @Getter("webClient", { namespace: "config" })
    config!: WebClientConfiguration;

    @Action("addError", { namespace: "errorBanner" })
    addError!: (error: BannerError) => void;

    private filterText: string = "";
    private filterTypes: string[] = [];
    private timelineEntries: TimelineEntry[] = [];
    private isMedicationLoading: boolean = false;
    private isImmunizationLoading: boolean = false;
    private isLaboratoryLoading: boolean = false;
    private isEncounterLoading: boolean = false;
    private isNoteLoading: boolean = false;
    private idleLogoutWarning: boolean = false;
    private protectiveWordAttempts: number = 0;
    private isAddingNote: boolean = false;
    private isEditingEntry: boolean = false;
    private isPacificTime: boolean = false;
    private unsavedChangesText: string =
        "You have unsaved changes. Are you sure you want to leave?";

    private isListView: boolean = true;
    private eventBus = EventBus;

    @Ref("protectiveWordModal")
    readonly protectiveWordModal!: ProtectiveWordComponent;
    @Ref("covidModal")
    readonly covidModal!: CovidModalComponent;

    private mounted() {
        this.fetchMedicationStatements();
        this.fetchImmunizations();
        this.fetchLaboratoryResults();
        this.fetchEncounters();
        this.fetchNotes();
        window.addEventListener("beforeunload", this.onBrowserClose);
        let self = this;
        this.eventBus.$on("timelineCreateNote", function () {
            self.isAddingNote = true;
        });
        this.eventBus.$on("timelinePrintView", function () {
            self.printRecords();
        });
        this.eventBus.$on("idleLogoutWarning", function (isVisible: boolean) {
            self.idleLogoutWarning = isVisible;
        });

        this.eventBus.$on("timelineEntryAdded", function (
            entry: TimelineEntry
        ) {
            self.onEntryAdded(entry);
        });
        this.eventBus.$on("timelineEntryEdit", function (entry: TimelineEntry) {
            self.onEntryEdit(entry);
        });
        this.eventBus.$on("timelineEntryUpdated", function (
            entry: TimelineEntry
        ) {
            self.onEntryUpdated(entry);
        });
        this.eventBus.$on("timelineEntryDeleted", function (
            entry: TimelineEntry
        ) {
            self.onEntryDeleted(entry);
        });
        this.eventBus.$on("timelineEntryAddClose", function (
            entry: TimelineEntry
        ) {
            self.onEntryAddClose(entry);
        });
        this.eventBus.$on("timelineEntryEditClose", function (
            entry: TimelineEntry
        ) {
            self.onEntryEditClose(entry);
        });
        this.eventBus.$on("calendarDateEventClick", function (
            eventDate: DateWrapper
        ) {
            self.isListView = true;
        });
        /*if (moment().isDST()) {
            !this.checkTimezone(true)
                ? (this.isPacificTime = false)
                : (this.isPacificTime = true);
        } else {
            !this.checkTimezone(false)
                ? (this.isPacificTime = false)
                : (this.isPacificTime = true);
        }*/
    }

    private beforeRouteLeave(to: Route, from: Route, next: any) {
        if (
            !this.idleLogoutWarning &&
            (this.isAddingNote || this.isEditingEntry) &&
            !confirm(this.unsavedChangesText)
        ) {
            return;
        }
        next();
    }

    private onBrowserClose(event: BeforeUnloadEvent) {
        if (
            !this.idleLogoutWarning &&
            (this.isAddingNote || this.isEditingEntry)
        ) {
            event.returnValue = this.unsavedChangesText;
        }
    }

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
            this.isMedicationLoading ||
            this.isImmunizationLoading ||
            this.isLaboratoryLoading ||
            this.isEncounterLoading ||
            this.isNoteLoading
        );
    }

    private onCovidSubmit() {
        this.eventBus.$emit("filterSelected", "laboratory");
    }

    private onCovidCancel() {
        // Display protective word modal if required
        if (this.protectiveWordAttempts > 0) {
            this.protectiveWordModal.showModal();
        }
    }

    private checkTimezone(isDST: boolean): boolean {
        if (isDST) {
            return new Date().getTimezoneOffset() / 60 === 7;
        } else {
            return new Date().getTimezoneOffset() / 60 === 8;
        }
    }

    private fetchMedicationStatements(protectiveWord?: string) {
        this.isMedicationLoading = true;

        this.getMedicationStatements({
            hdid: this.user.hdid,
            protectiveWord: protectiveWord,
        })
            .then((results) => {
                if (results.resultStatus == ResultType.Success) {
                    this.protectiveWordAttempts = 0;
                    // Add the medication entries to the timeline list
                    for (let result of results.resourcePayload) {
                        this.timelineEntries.push(
                            new MedicationTimelineEntry(result)
                        );
                    }
                    this.sortEntries();
                } else if (results.resultStatus == ResultType.Protected) {
                    if (!this.covidModal.show) {
                        this.protectiveWordModal.showModal();
                    }
                    this.protectiveWordAttempts++;
                } else {
                    this.logger.error(
                        "Error returned from the medication statements call: " +
                            JSON.stringify(results.resultError)
                    );
                    this.addError(
                        ErrorTranslator.toBannerError(
                            "Fetch Medications Error",
                            results.resultError
                        )
                    );
                }
            })
            .catch((err) => {
                this.logger.error(err);
                this.addError(
                    ErrorTranslator.toBannerError(
                        "Fetch Medications Error",
                        err
                    )
                );
            })
            .finally(() => {
                this.isMedicationLoading = false;
            });
    }

    private fetchImmunizations() {
        const immunizationService: IImmunizationService = container.get(
            SERVICE_IDENTIFIER.ImmunizationService
        );
        this.isImmunizationLoading = true;
        immunizationService
            .getPatientImmunizations(this.user.hdid)
            .then((results) => {
                if (results.resultStatus == ResultType.Success) {
                    // Add the immunization entries to the timeline list
                    for (let result of results.resourcePayload) {
                        this.timelineEntries.push(
                            new ImmunizationTimelineEntry(result)
                        );
                    }
                    this.sortEntries();
                } else {
                    this.logger.error(
                        "Error returned from the immunization call: " +
                            JSON.stringify(results.resultError)
                    );
                    this.addError(
                        ErrorTranslator.toBannerError(
                            "Fetch Immunizations Error",
                            results.resultError
                        )
                    );
                }
            })
            .catch((err) => {
                this.logger.error(err);
                this.addError(
                    ErrorTranslator.toBannerError(
                        "Fetch Immunizations Error",
                        err
                    )
                );
            })
            .finally(() => {
                this.isImmunizationLoading = false;
            });
    }

    private fetchLaboratoryResults() {
        const laboratoryService: ILaboratoryService = container.get(
            SERVICE_IDENTIFIER.LaboratoryService
        );
        this.isLaboratoryLoading = true;
        this.getLaboratoryOrders({ hdid: this.user.hdid })
            .then((results) => {
                if (results.resultStatus == ResultType.Success) {
                    // Add the laboratory entries to the timeline list
                    for (let result of results.resourcePayload) {
                        this.timelineEntries.push(
                            new LaboratoryTimelineEntry(result)
                        );
                    }
                    this.sortEntries();

                    if (results.resourcePayload.length > 0) {
                        this.protectiveWordModal.hideModal();
                        this.covidModal.showModal();
                    }
                } else {
                    this.logger.error(
                        "Error returned from the laboratory call: " +
                            JSON.stringify(results.resultError)
                    );
                    this.addError(
                        ErrorTranslator.toBannerError(
                            "Fetch Laboratory Error",
                            results.resultError
                        )
                    );
                }
            })
            .catch((err) => {
                this.logger.error(err);
                this.addError(
                    ErrorTranslator.toBannerError("Fetch Laboratory Error", err)
                );
            })
            .finally(() => {
                this.isLaboratoryLoading = false;
            });
    }

    private fetchEncounters() {
        this.isEncounterLoading = true;
        const encounterService: IEncounterService = container.get(
            SERVICE_IDENTIFIER.EncounterService
        );
        this.isEncounterLoading = true;
        encounterService
            .getPatientEncounters(this.user.hdid)
            .then((results) => {
                if (results.resultStatus == ResultType.Success) {
                    // Add the encounter entries to the timeline list
                    for (let result of results.resourcePayload) {
                        this.timelineEntries.push(
                            new EncounterTimelineEntry(result)
                        );
                    }
                    this.sortEntries();
                } else {
                    this.logger.error(
                        "Error returned from the encounter call: " +
                            JSON.stringify(results.resultError)
                    );
                    this.addError(
                        ErrorTranslator.toBannerError(
                            "Fetch Encounter Error",
                            results.resultError
                        )
                    );
                }
            })
            .catch((err) => {
                this.logger.error(err);
                this.addError(
                    ErrorTranslator.toBannerError("Fetch Encounter Error", err)
                );
            })
            .finally(() => {
                this.isEncounterLoading = false;
            });
    }

    private fetchNotes() {
        const noteService: IUserNoteService = container.get(
            SERVICE_IDENTIFIER.UserNoteService
        );
        this.isNoteLoading = true;
        noteService
            .getNotes()
            .then((results) => {
                if (results.resultStatus == ResultType.Success) {
                    // Add the immunization entries to the timeline list
                    for (let result of results.resourcePayload) {
                        this.timelineEntries.push(
                            new NoteTimelineEntry(result)
                        );
                    }
                    this.sortEntries();
                } else {
                    this.logger.error(
                        "Error returned from the note call: " +
                            JSON.stringify(results.resultError)
                    );
                    this.addError(
                        ErrorTranslator.toBannerError(
                            "Fetch Notes Error",
                            results.resultError
                        )
                    );
                }
            })
            .catch((err) => {
                this.logger.error(err);
                this.addError(
                    ErrorTranslator.toBannerError("Fetch Notes Error", err)
                );
            })
            .finally(() => {
                this.isNoteLoading = false;
            });
    }

    private onEntryAdded(entry: TimelineEntry) {
        this.logger.debug(`Timeline Entry added: ${JSON.stringify(entry)}`);
        this.isAddingNote = false;
        if (entry) {
            this.timelineEntries.push(entry);
            this.sortEntries();
        }
    }

    private onEntryEdit(entry: TimelineEntry) {
        this.isEditingEntry = true;
    }

    private onEntryEditClose(entry: TimelineEntry) {
        this.isEditingEntry = false;
    }

    private onEntryAddClose(entry: TimelineEntry) {
        this.isAddingNote = false;
    }

    private onEntryUpdated(entry: TimelineEntry) {
        this.logger.debug(`Timeline Entry updated: ${JSON.stringify(entry)}`);
        const index = this.timelineEntries.findIndex(
            (e) => e.id === entry.id && e.type === entry.type
        );
        this.timelineEntries.splice(index, 1);
        this.timelineEntries.push(entry);
        this.isEditingEntry = false;
        this.sortEntries();
    }

    private onEntryDeleted(entry: TimelineEntry) {
        this.logger.debug(`Timeline Entry deleted: ${JSON.stringify(entry)}`);
        const index = this.timelineEntries.findIndex((e) => e.id == entry.id);
        this.timelineEntries.splice(index, 1);
        this.sortEntries();
    }

    private onProtectiveWordSubmit(value: string) {
        this.fetchMedicationStatements(value);
    }

    private onProtectiveWordCancel() {
        // Does nothing as it won't be able to fetch pharmanet data.
        this.logger.debug("protective word cancelled");
    }

    private getTotalCount(): number {
        return this.timelineEntries.length;
    }

    private sortEntries() {
        this.timelineEntries.sort((a, b) =>
            a.date.isAfter(b.date) ? -1 : a.date.isBefore(b.date) ? 1 : 0
        );
    }

    private toggleListView() {
        this.isListView = true;
        window.location.hash = "linear";
    }
    private toggleMonthView() {
        this.isListView = false;
        window.location.hash = "calendar";
    }

    private printRecords() {
        window.print();
    }

    private filtersChanged(newFilters: string[]) {
        this.filterTypes = newFilters;
    }
}
</script>
