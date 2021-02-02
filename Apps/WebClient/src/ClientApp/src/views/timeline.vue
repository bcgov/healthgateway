<script lang="ts">
import { faSearch, IconDefinition } from "@fortawesome/free-solid-svg-icons";
import Vue from "vue";
import { Component, Ref, Watch } from "vue-property-decorator";
import { NavigationGuardNext, Route } from "vue-router";
import { Action, Getter } from "vuex-class";

import ErrorCardComponent from "@/components/errorCard.vue";
import LoadingComponent from "@/components/loading.vue";
import CovidModalComponent from "@/components/modal/covid.vue";
import NoteEditComponent from "@/components/modal/noteEdit.vue";
import ProtectiveWordComponent from "@/components/modal/protectiveWord.vue";
import CalendarTimelineComponent from "@/components/timeline/calendarTimeline.vue";
import FilterComponent from "@/components/timeline/filters.vue";
import LinearTimelineComponent from "@/components/timeline/linearTimeline.vue";
import { ActionType } from "@/constants/actionType";
import { ResultType } from "@/constants/resulttype";
import UserPreferenceType from "@/constants/userPreferenceType";
import EventBus, { EventMessageName } from "@/eventbus";
import BannerError from "@/models/bannerError";
import { Dictionary } from "@/models/baseTypes";
import type { WebClientConfiguration } from "@/models/configData";
import { DateWrapper } from "@/models/dateWrapper";
import EncounterTimelineEntry from "@/models/encounterTimelineEntry";
import { ImmunizationEvent } from "@/models/immunizationModel";
import ImmunizationTimelineEntry from "@/models/immunizationTimelineEntry";
import { LaboratoryOrder } from "@/models/laboratory";
import LaboratoryTimelineEntry from "@/models/laboratoryTimelineEntry";
import MedicationStatementHistory from "@/models/medicationStatementHistory";
import MedicationTimelineEntry from "@/models/medicationTimelineEntry";
import NoteTimelineEntry from "@/models/noteTimelineEntry";
import RequestResult from "@/models/requestResult";
import TimelineEntry, { EntryType } from "@/models/timelineEntry";
import TimelineFilter, { EntryTypeFilter } from "@/models/timelineFilter";
import User from "@/models/user";
import { UserComment } from "@/models/userComment";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import {
    IEncounterService,
    ILogger,
    IUserNoteService,
} from "@/services/interfaces";
import ErrorTranslator from "@/utility/errorTranslator";

const namespace = "user";

// Register the router hooks with their names
Component.registerHooks(["beforeRouteLeave"]);

@Component({
    components: {
        LoadingComponent,
        ProtectiveWordComponent,
        CovidModalComponent,
        NoteEditComponent,
        LinearTimeline: LinearTimelineComponent,
        CalendarTimeline: CalendarTimelineComponent,
        ErrorCard: ErrorCardComponent,
        Filters: FilterComponent,
    },
})
export default class TimelineView extends Vue {
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

    @Action("retrieveProfileComments", { namespace: "comment" })
    retrieveProfileComments!: (params: {
        hdid: string;
    }) => Promise<RequestResult<Dictionary<UserComment[]>>>;

    @Getter("webClient", { namespace: "config" })
    config!: WebClientConfiguration;

    @Action("addError", { namespace: "errorBanner" })
    addError!: (error: BannerError) => void;

    @Action("retrieve", { namespace: "immunization" })
    retrieveImmunizations!: (params: {
        hdid: string;
    }) => Promise<ImmunizationEvent[]>;

    @Getter("getStoredImmunizations", { namespace: "immunization" })
    patientImmunizations!: ImmunizationEvent[];

    @Getter("isDeferredLoad", { namespace: "immunization" })
    immunizationIsDeferred!: boolean;

    @Watch("immunizationIsDeferred")
    private whenImmunizationIsDeferred(newVal: boolean, oldVal: boolean) {
        if (newVal) {
            this.immunizationLoadDeferred = true;
        }

        if (!newVal && oldVal) {
            this.immunizationLoadReady = true;
            if (this.patientImmunizations.length === 0) {
                this.loadImmunizationEntries();
            }
        }
    }

    @Watch("filterText")
    private onFilterTextChanged() {
        this.filter.keyword = this.filterText;
    }

    @Watch("filter", { deep: true })
    private onFilterChanged() {
        this.filterText = this.filter.keyword;
    }

    private immunizationLoadDeferred = false;
    private immunizationLoadReady = false;

    private filterText = "";
    private filter: TimelineFilter = new TimelineFilter([]);
    private isListView = true;
    private timelineEntries: TimelineEntry[] = [];
    private isMedicationLoading = false;
    private isImmunizationLoading = false;
    private isLaboratoryLoading = false;
    private isEncounterLoading = false;
    private isNoteLoading = false;
    private isCommentLoading = false;
    private idleLogoutWarning = false;
    private protectiveWordAttempts = 0;
    private isAddingNote = false;
    private isEditingEntry = false;
    private isPacificTime = false;
    private isBlankNote = true;
    private unsavedChangesText =
        "You have unsaved changes. Are you sure you want to leave?";

    private eventBus = EventBus;

    private logger!: ILogger;

    @Ref("protectiveWordModal")
    readonly protectiveWordModal!: ProtectiveWordComponent;
    @Ref("covidModal")
    readonly covidModal!: CovidModalComponent;
    @Ref("noteEditModal")
    readonly noteEditModal!: NoteEditComponent;

    private mounted() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);

        this.fetchMedicationStatements();
        this.fetchImmunizations();
        this.fetchLaboratoryResults();
        this.fetchEncounters();
        this.fetchNotes();
        this.fetchComments();
        window.addEventListener("beforeunload", this.onBrowserClose);
        this.eventBus.$on(EventMessageName.TimelineCreateNote, () => {
            this.isAddingNote = true;
            this.noteEditModal.showModal();
        });
        this.eventBus.$on(
            EventMessageName.IdleLogoutWarning,
            (isVisible: boolean) => {
                this.idleLogoutWarning = isVisible;
            }
        );

        this.eventBus.$on(
            EventMessageName.TimelineEntryAdded,
            (entry: TimelineEntry) => {
                this.onEntryAdded(entry);
            }
        );
        this.eventBus.$on(
            EventMessageName.TimelineEntryEdit,
            (note: NoteTimelineEntry) => {
                this.onEntryEdit();
                this.noteEditModal.showModal(note);
            }
        );
        this.eventBus.$on(
            EventMessageName.TimelineEntryUpdated,
            (entry: TimelineEntry) => {
                this.onEntryUpdated(entry);
            }
        );
        this.eventBus.$on(
            EventMessageName.TimelineEntryDeleted,
            (entry: TimelineEntry) => {
                this.onEntryDeleted(entry);
            }
        );
        this.eventBus.$on(EventMessageName.TimelineNoteEditClose, () => {
            this.onNoteEditClose();
        });
        this.eventBus.$on(EventMessageName.IsNoteBlank, (isBlank: boolean) => {
            this.isBlankNote = isBlank;
        });
        this.eventBus.$on(
            EventMessageName.TimelineViewUpdated,
            (isListView: boolean) => {
                this.isListView = isListView;
            }
        );

        if (new DateWrapper().isInDST()) {
            !this.checkTimezone(true)
                ? (this.isPacificTime = false)
                : (this.isPacificTime = true);
        } else {
            !this.checkTimezone(false)
                ? (this.isPacificTime = false)
                : (this.isPacificTime = true);
        }

        let entryTypes: EntryTypeFilter[] = [
            {
                type: EntryType.Immunization,
                display: "Immunizations",
                isEnabled: this.config.modules[EntryType.Immunization],
                numEntries: 0,
                isSelected: false,
            },
            {
                type: EntryType.Medication,
                display: "Medications",
                isEnabled: this.config.modules[EntryType.Medication],
                numEntries: 0,
                isSelected: false,
            },
            {
                type: EntryType.Laboratory,
                display: "Laboratory",
                isEnabled: this.config.modules[EntryType.Laboratory],
                numEntries: 0,
                isSelected: false,
            },
            {
                type: EntryType.Encounter,
                display: "MSP Visits",
                isEnabled: this.config.modules[EntryType.Encounter],
                numEntries: 0,
                isSelected: false,
            },
            {
                type: EntryType.Note,
                display: "My Notes",
                isEnabled: this.config.modules[EntryType.Note],
                numEntries: 0,
                isSelected: false,
            },
        ];
        this.filter = new TimelineFilter(entryTypes);
    }

    private beforeDestroy() {
        this.eventBus.$off(EventMessageName.TimelineCreateNote);
        this.eventBus.$off(EventMessageName.IdleLogoutWarning);
        this.eventBus.$off(EventMessageName.TimelineEntryAdded);
        this.eventBus.$off(EventMessageName.TimelineEntryEdit);
        this.eventBus.$off(EventMessageName.TimelineEntryUpdated);
        this.eventBus.$off(EventMessageName.TimelineEntryDeleted);
        this.eventBus.$off(EventMessageName.TimelineNoteEditClose);
        this.eventBus.$off(EventMessageName.IsNoteBlank);
        this.eventBus.$off(EventMessageName.TimelineViewUpdated);
    }

    private beforeRouteLeave(
        to: Route,
        from: Route,
        next: NavigationGuardNext
    ) {
        if (
            !this.idleLogoutWarning &&
            (this.isAddingNote || this.isEditingEntry) &&
            !this.isBlankNote &&
            !confirm(this.unsavedChangesText)
        ) {
            return;
        }
        next();
    }

    private onBrowserClose(event: BeforeUnloadEvent) {
        if (
            !this.idleLogoutWarning &&
            !this.isBlankNote &&
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
            this.isNoteLoading ||
            this.isCommentLoading
        );
    }

    private onCovidSubmit() {
        this.eventBus.$emit(
            EventMessageName.SelectedFilter,
            EntryType.Laboratory
        );
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
                    this.setFilterTypeCount(
                        EntryType.Medication,
                        results.resourcePayload.length
                    );
                } else if (
                    results.resultStatus == ResultType.ActionRequired &&
                    results.resultError?.actionCode == ActionType.Protected
                ) {
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
        this.isImmunizationLoading = true;
        this.retrieveImmunizations({ hdid: this.user.hdid })
            .then(() => {
                this.loadImmunizationEntries();
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
                    this.setFilterTypeCount(
                        EntryType.Laboratory,
                        results.resourcePayload.length
                    );

                    if (results.resourcePayload.length > 0) {
                        this.protectiveWordModal.hideModal();
                        let showCovidModal = true;
                        const actionedCovidPreference =
                            UserPreferenceType.ActionedCovidModalAt;
                        if (
                            this.user.preferences[actionedCovidPreference] !=
                            undefined
                        ) {
                            const actionedCovidModalAt = new DateWrapper(
                                this.user.preferences[
                                    actionedCovidPreference
                                ].value
                            );
                            const mostRecentLabTime = new DateWrapper(
                                results.resourcePayload[0].messageDateTime
                            );
                            if (
                                actionedCovidModalAt.isAfter(mostRecentLabTime)
                            ) {
                                showCovidModal = false;
                            }
                        }
                        if (showCovidModal) {
                            this.covidModal.showModal();
                        }
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
                    this.setFilterTypeCount(
                        EntryType.Encounter,
                        results.resourcePayload.length
                    );
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
            .getNotes(this.user.hdid)
            .then((results) => {
                if (results.resultStatus == ResultType.Success) {
                    // Add the immunization entries to the timeline list
                    for (let result of results.resourcePayload) {
                        this.timelineEntries.push(
                            new NoteTimelineEntry(result)
                        );
                    }
                    this.sortEntries();
                    this.setFilterTypeCount(
                        EntryType.Note,
                        results.resourcePayload.length
                    );
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

    private fetchComments() {
        this.isCommentLoading = true;

        this.retrieveProfileComments({
            hdid: this.user.hdid,
        })
            .then((results) => {
                if (results.resultStatus == ResultType.Success) {
                    this.logger.debug("Profile Comments Loaded");
                } else {
                    this.logger.error(
                        "Error returned from the retrieve comments call: " +
                            JSON.stringify(results.resultError)
                    );
                    this.addError(
                        ErrorTranslator.toBannerError(
                            "Profile Comments Error",
                            results.resultError
                        )
                    );
                }
            })
            .catch((err) => {
                this.logger.error(err);
                this.addError(
                    ErrorTranslator.toBannerError("Profile Comments Error", err)
                );
            })
            .finally(() => {
                this.isCommentLoading = false;
            });
    }

    private loadImmunizationEntries() {
        if (this.immunizationLoadReady) {
            this.immunizationLoadDeferred = false;
            this.immunizationLoadReady = false;
        }
        // Add the immunization entries to the timeline list
        for (let immunization of this.patientImmunizations) {
            this.timelineEntries.push(
                new ImmunizationTimelineEntry(immunization)
            );
        }
        this.sortEntries();
        this.setFilterTypeCount(
            EntryType.Immunization,
            this.patientImmunizations.length
        );
    }

    private onEntryAdded(entry: TimelineEntry) {
        this.logger.debug(`Timeline Entry added: ${JSON.stringify(entry)}`);
        this.isAddingNote = false;
        if (entry) {
            this.timelineEntries.push(entry);
            this.sortEntries();
            if (entry.type === EntryType.Note) {
                this.addFilterTypeCount(EntryType.Note, 1);
            }
        }
    }

    private onEntryEdit() {
        this.isEditingEntry = true;
    }

    private onNoteEditClose() {
        this.isEditingEntry = false;
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
        if (entry.type === EntryType.Note) {
            this.addFilterTypeCount(EntryType.Note, -1);
        }
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

    private setFilterTypeCount(entryType: EntryType, count: number) {
        let typeFilter = this.filter.entryTypes.find(
            (x) => x.type === entryType
        );
        if (typeFilter) {
            typeFilter.numEntries = count;
        }
    }

    private filtersChanged(newFilter: TimelineFilter) {
        this.filter = newFilter;
        this.filter.keyword = this.filterText;
    }

    private addFilterTypeCount(entryType: EntryType, count: number) {
        let typeFilter = this.filter.entryTypes.find(
            (x) => x.type === entryType
        );
        if (typeFilter) {
            typeFilter.numEntries += count;
        }
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
                        :show="immunizationLoadDeferred"
                        variant="info"
                        class="no-print"
                    >
                        <span v-if="!immunizationLoadReady">
                            <h4 data-testid="immunizationLoading">
                                Still loading your immunization records
                            </h4>
                        </span>
                        <span v-else data-testid="immunizationReady">
                            <h4 data-testid="immunizationReadyHeader">
                                Your immunization records are ready
                            </h4>
                            <b-btn
                                data-testid="immunizationBtnReady"
                                variant="link"
                                class="detailsButton px-0"
                                @click="loadImmunizationEntries()"
                            >
                                Load to timeline.
                            </b-btn></span
                        >
                    </b-alert>
                </div>

                <div id="pageTitle" class="px-2">
                    <h1 id="subject">Health Care Timeline</h1>
                    <hr class="mb-0" />
                </div>
                <div class="sticky-top sticky-offset px-2">
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
                            <Filters
                                :is-list-view="isListView"
                                :filter.sync="filter"
                            />
                        </b-col>
                    </b-row>
                </div>
                <LinearTimeline
                    v-show="isListView && !isLoading"
                    :timeline-entries="timelineEntries"
                    :is-visible="isListView"
                    :total-entries="getTotalCount()"
                    :filter="filter"
                    :is-list-view="isListView"
                >
                </LinearTimeline>
                <CalendarTimeline
                    v-show="!isListView && !isLoading"
                    :timeline-entries="timelineEntries"
                    :is-visible="!isListView"
                    :total-entries="getTotalCount()"
                    :filter="filter"
                    :is-list-view="isListView"
                >
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
        <NoteEditComponent ref="noteEditModal" :is-loading="isLoading" />
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
