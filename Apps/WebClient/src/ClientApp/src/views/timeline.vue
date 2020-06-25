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
</style>
<template>
    <div>
        <LoadingComponent :is-loading="isLoading"></LoadingComponent>
        <b-row class="my-3 fluid justify-content-md-center">
            <b-col class="col-12 col-md-1 col-lg-1 column-wrapper no-print">
            </b-col>
            <b-col
                id="timeline"
                class="col-12 col-md-8 col-lg-6 column-wrapper"
            >
                <b-alert
                    :show="hasErrors"
                    dismissible
                    variant="danger"
                    class="no-print"
                >
                    <h4>Error</h4>
                    <span
                        >An unexpected error occured while processing the
                        request.</span
                    >
                </b-alert>
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

                <div id="pageTitle">
                    <h1 id="subject">Health Care Timeline</h1>
                    <hr />
                </div>
                <b-row class="no-print">
                    <b-col>
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
                </b-row>
                <b-row align-h="start" class="no-print">
                    <b-col v-if="isMedicationEnabled">
                        <b-form-checkbox
                            id="medicationFilter"
                            v-model="filterTypes"
                            name="medicationFilter"
                            value="Medication"
                        >
                            Medications
                        </b-form-checkbox>
                    </b-col>
                    <b-col v-if="isImmunizationEnabled">
                        <b-form-checkbox
                            id="immunizationFilter"
                            v-model="filterTypes"
                            name="immunizationFilter"
                            value="Immunization"
                        >
                            Immunizations
                        </b-form-checkbox>
                    </b-col>
                    <b-col v-if="isLaboratoryEnabled">
                        <b-form-checkbox
                            id="laboratoryFilter"
                            v-model="filterTypes"
                            name="laboratoryFilter"
                            value="Laboratory"
                        >
                            Laboratory
                        </b-form-checkbox>
                    </b-col>
                    <b-col v-if="isNoteEnabled">
                        <b-form-checkbox
                            id="notesFilter"
                            v-model="filterTypes"
                            name="notesFilter"
                            value="Note"
                        >
                            Notes
                        </b-form-checkbox>
                    </b-col>
                </b-row>
                <br />
                <b-row v-if="isAddingNote" class="pb-5">
                    <b-col>
                        <NoteTimelineComponent :is-add-mode="true" />
                    </b-col>
                </b-row>
                <LinearTimeline
                    :timeline-entries="filteredTimelineEntries"
                    :total-entries="getTotalCount()"
                />
            </b-col>
            <b-col class="col-3 col-md-2 col-lg-3 column-wrapper no-print">
                <HealthlinkComponent />
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
import moment from "moment";
import { Route } from "vue-router";
import EventBus from "@/eventbus";
import { WebClientConfiguration } from "@/models/configData";
import {
    IImmunizationService,
    ILaboratoryService,
    IMedicationService,
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
import NoteTimelineEntry from "@/models/noteTimelineEntry";
import MedicationStatement from "@/models/medicationStatement";
import UserNote from "@/models/userNote";
import RequestResult from "@/models/requestResult";
import { IconDefinition, faSearch } from "@fortawesome/free-solid-svg-icons";

import LoadingComponent from "@/components/loading.vue";
import ProtectiveWordComponent from "@/components/modal/protectiveWord.vue";
import CovidModalComponent from "@/components/modal/covid.vue";
import EntryCardTimelineComponent from "@/components/timeline/entrycard.vue";
import HealthlinkSidebarComponent from "@/components/timeline/healthlink.vue";
import NoteTimelineComponent from "@/components/timeline/note.vue";
import {
    LaboratoryOrder,
    LaboratoryReport,
    LaboratoryResult,
} from "@/models/laboratory";
import LinearTimelineComponent from "@/components/timeline/linearTimeline.vue";

const namespace: string = "user";

// Register the router hooks with their names
Component.registerHooks(["beforeRouteLeave"]);

@Component({
    components: {
        LoadingComponent,
        ProtectiveWordComponent,
        CovidModalComponent,
        EntryCardComponent: EntryCardTimelineComponent,
        HealthlinkComponent: HealthlinkSidebarComponent,
        NoteTimelineComponent,
        LinearTimeline: LinearTimelineComponent,
    },
})
export default class TimelineView extends Vue {
    @Getter("user", { namespace }) user!: User;
    @Action("getOrders", { namespace: "laboratory" })
    getLaboratoryOrders!: (params: {
        hdid: string;
    }) => Promise<RequestResult<LaboratoryOrder[]>>;
    @Getter("webClient", { namespace: "config" })
    config!: WebClientConfiguration;

    private filterText: string = "";
    private timelineEntries: TimelineEntry[] = [];
    private filteredTimelineEntries: TimelineEntry[] = [];
    private isMedicationLoading: boolean = false;
    private isImmunizationLoading: boolean = false;
    private isLaboratoryLoading: boolean = false;
    private isNoteLoading: boolean = false;
    private hasErrors: boolean = false;
    private idleLogoutWarning: boolean = false;
    private protectiveWordAttempts: number = 0;
    private isAddingNote: boolean = false;
    private isEditingEntry: boolean = false;
    private unsavedChangesText: string =
        "You have unsaved changes. Are you sure you want to leave?";

    private filterTypes: string[] = [];

    @Ref("protectiveWordModal")
    readonly protectiveWordModal!: ProtectiveWordComponent;
    @Ref("covidModal")
    readonly covidModal!: CovidModalComponent;

    private mounted() {
        this.initializeFilters();
        this.fetchMedicationStatements();
        this.fetchImmunizations();
        this.fetchLaboratoryResults();
        this.fetchNotes();
        window.addEventListener("beforeunload", this.onBrowserClose);
        let self = this;
        EventBus.$on("timelineCreateNote", function () {
            self.isAddingNote = true;
        });
        EventBus.$on("timelinePrintView", function () {
            self.printRecords();
        });
        EventBus.$on("idleLogoutWarning", function (isVisible: boolean) {
            self.idleLogoutWarning = isVisible;
        });

        EventBus.$on("timelineEntryAdded", function (entry: TimelineEntry) {
            self.onEntryAdded(entry);
        });
        EventBus.$on("timelineEntryEdit", function (entry: TimelineEntry) {
            self.onEntryEdit(entry);
        });
        EventBus.$on("timelineEntryUpdated", function (entry: TimelineEntry) {
            self.onEntryUpdated(entry);
        });
        EventBus.$on("timelineEntryDeleted", function (entry: TimelineEntry) {
            self.onEntryDeleted(entry);
        });
        EventBus.$on("timelineEntryAddClose", function (entry: TimelineEntry) {
            self.onEntryAddClose(entry);
        });
        EventBus.$on("timelineEntryEditClose", function (entry: TimelineEntry) {
            self.onEntryEditClose(entry);
        });
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
            this.isNoteLoading
        );
    }

    private get isMedicationEnabled(): boolean {
        return (
            this.config.modules["MedicationHistory"] ||
            this.config.modules["Medication"]
        );
    }

    private get isImmunizationEnabled(): boolean {
        return this.config.modules["Immunization"];
    }

    private get isLaboratoryEnabled(): boolean {
        return this.config.modules["Laboratory"];
    }

    private get isNoteEnabled(): boolean {
        return this.config.modules["Note"];
    }

    private initializeFilters(): void {
        if (this.isMedicationEnabled) {
            this.filterTypes.push("Medication");
        }
        if (this.isImmunizationEnabled) {
            this.filterTypes.push("Immunization");
        }
        if (this.isLaboratoryEnabled) {
            this.filterTypes.push("Laboratory");
        }
        if (this.isNoteEnabled) {
            this.filterTypes.push("Note");
        }
    }

    private onCovidSubmit() {
        this.filterTypes = ["Laboratory"];
    }

    private onCovidCancel() {
        // Display protective word modal if required
        if (this.protectiveWordAttempts > 0) {
            this.protectiveWordModal.showModal();
        }
    }

    private fetchMedicationStatements(protectiveWord?: string) {
        const medicationService: IMedicationService = container.get(
            SERVICE_IDENTIFIER.MedicationService
        );
        this.isMedicationLoading = true;
        const isOdrEnabled = this.config.modules["MedicationHistory"];
        let promise: Promise<RequestResult<MedicationStatement[]>>;
        if (isOdrEnabled) {
            promise = medicationService.getPatientMedicationStatementHistory(
                this.user.hdid,
                protectiveWord
            );
        } else {
            promise = medicationService.getPatientMedicationStatements(
                this.user.hdid,
                protectiveWord
            );
        }

        promise
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
                    this.applyTimelineFilter();
                } else if (results.resultStatus == ResultType.Protected) {
                    if (this.covidModal.isVisible) {
                        this.protectiveWordModal.showModal();
                    }
                    this.protectiveWordAttempts++;
                } else {
                    console.log(
                        "Error returned from the medication statements call: " +
                            results.resultMessage
                    );
                    this.hasErrors = true;
                }
            })
            .catch((err) => {
                this.hasErrors = true;
                console.log(err);
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
                    this.applyTimelineFilter();
                } else {
                    console.log(
                        "Error returned from the immunization call: " +
                            results.resultMessage
                    );
                    this.hasErrors = true;
                }
            })
            .catch((err) => {
                this.hasErrors = true;
                console.log(err);
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
                    this.applyTimelineFilter();

                    if (results.resourcePayload.length > 0) {
                        this.protectiveWordModal.hideModal();
                        this.covidModal.showModal();
                    }
                } else {
                    console.log(
                        "Error returned from the laboratory call: " +
                            results.resultMessage
                    );
                    this.hasErrors = true;
                }
            })
            .catch((err) => {
                this.hasErrors = true;
                console.log(err);
            })
            .finally(() => {
                this.isLaboratoryLoading = false;
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
                    this.applyTimelineFilter();
                } else {
                    console.log(
                        "Error returned from the note call: " +
                            results.resultMessage
                    );
                    this.hasErrors = true;
                }
            })
            .catch((err) => {
                this.hasErrors = true;
                console.log(err);
            })
            .finally(() => {
                this.isNoteLoading = false;
            });
    }

    private onEntryAdded(entry: TimelineEntry) {
        this.isAddingNote = false;
        if (entry) {
            this.timelineEntries.push(entry);
            this.sortEntries();
            this.applyTimelineFilter();
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
        console.log(entry);
        const index = this.timelineEntries.findIndex(
            (e) => e.id === entry.id && e.type === entry.type
        );
        this.timelineEntries.splice(index, 1);
        this.timelineEntries.push(entry);
        this.isEditingEntry = false;
        this.sortEntries();
        this.applyTimelineFilter();
    }

    private onEntryDeleted(entry: TimelineEntry) {
        console.log(entry);
        const index = this.timelineEntries.findIndex((e) => e.id == entry.id);
        this.timelineEntries.splice(index, 1);
        this.sortEntries();
        this.applyTimelineFilter();
    }

    private onProtectiveWordSubmit(value: string) {
        this.fetchMedicationStatements(value);
    }

    private onProtectiveWordCancel() {
        // Does nothing as it won't be able to fetch pharmanet data.
        console.log("protective word cancelled");
    }

    @Watch("filterText")
    @Watch("filterTypes")
    private applyTimelineFilter() {
        this.filteredTimelineEntries = this.timelineEntries.filter((entry) =>
            entry.filterApplies(this.filterText, this.filterTypes)
        );
    }

    private getTotalCount(): number {
        return this.timelineEntries.length;
    }

    private sortEntries() {
        this.timelineEntries.sort((a, b) =>
            a.date > b.date ? -1 : a.date < b.date ? 1 : 0
        );
    }

    private printRecords() {
        window.print();
    }
}
</script>
