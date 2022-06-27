<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import {
    faCheckCircle,
    faEdit,
    faFileMedical,
    faMicroscope,
    faPills,
    faQuestion,
    faSearch,
    faStethoscope,
    faSyringe,
    faVial,
} from "@fortawesome/free-solid-svg-icons";
import { BToast } from "bootstrap-vue";
import Vue from "vue";
import { Component, Watch } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import ErrorCardComponent from "@/components/ErrorCardComponent.vue";
import NoteEditComponent from "@/components/modal/NoteEditComponent.vue";
import ProtectiveWordComponent from "@/components/modal/ProtectiveWordComponent.vue";
import BreadcrumbComponent from "@/components/navmenu/BreadcrumbComponent.vue";
import ResourceCentreComponent from "@/components/ResourceCentreComponent.vue";
import AddNoteButtonComponent from "@/components/timeline/AddNoteButtonComponent.vue";
import EntryDetailsComponent from "@/components/timeline/entryCard/EntryDetailsComponent.vue";
import FilterComponent from "@/components/timeline/FilterComponent.vue";
import LinearTimelineComponent from "@/components/timeline/LinearTimelineComponent.vue";
import BreadcrumbItem from "@/models/breadcrumbItem";
import type { WebClientConfiguration } from "@/models/configData";
import Covid19LaboratoryOrderTimelineEntry from "@/models/covid19LaboratoryOrderTimelineEntry";
import Encounter from "@/models/encounter";
import EncounterTimelineEntry from "@/models/encounterTimelineEntry";
import { ImmunizationEvent } from "@/models/immunizationModel";
import ImmunizationTimelineEntry from "@/models/immunizationTimelineEntry";
import { Covid19LaboratoryOrder, LaboratoryOrder } from "@/models/laboratory";
import LaboratoryOrderTimelineEntry from "@/models/laboratoryOrderTimelineEntry";
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
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger } from "@/services/interfaces";

library.add(
    faCheckCircle,
    faEdit,
    faFileMedical,
    faMicroscope,
    faPills,
    faQuestion,
    faSearch,
    faStethoscope,
    faSyringe,
    faVial
);

@Component({
    components: {
        BToast,
        BreadcrumbComponent,
        ProtectiveWordComponent,
        NoteEditComponent,
        EntryDetailsComponent,
        LinearTimeline: LinearTimelineComponent,
        ErrorCard: ErrorCardComponent,
        Filters: FilterComponent,
        "resource-centre": ResourceCentreComponent,
        "add-note-button": AddNoteButtonComponent,
    },
})
export default class TimelineView extends Vue {
    @Getter("webClient", { namespace: "config" })
    config!: WebClientConfiguration;

    @Action("setKeyword", { namespace: "timeline" })
    setKeyword!: (keyword: string) => void;

    @Action("retrievePatientData", { namespace: "user" })
    retrievePatientData!: () => Promise<void>;

    @Action("retrieve", { namespace: "immunization" })
    retrieveImmunizations!: (params: { hdid: string }) => Promise<void>;

    @Action("retrieve", { namespace: "encounter" })
    retrieveEncounters!: (params: { hdid: string }) => Promise<void>;

    @Action("retrieve", { namespace: "note" })
    retrieveNotes!: (params: { hdid: string }) => Promise<void>;

    @Action("retrieveCovid19LaboratoryOrders", { namespace: "laboratory" })
    retrieveCovid19LaboratoryOrders!: (params: {
        hdid: string;
    }) => Promise<void>;

    @Action("retrieveLaboratoryOrders", { namespace: "laboratory" })
    retrieveLaboratoryOrders!: (params: { hdid: string }) => Promise<void>;

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

    @Getter("covid19LaboratoryOrdersAreLoading", { namespace: "laboratory" })
    isCovid19LaboratoryLoading!: boolean;

    @Getter("laboratoryOrdersAreQueued", { namespace: "laboratory" })
    isLaboratoryQueued!: boolean;

    @Getter("laboratoryOrdersAreLoading", { namespace: "laboratory" })
    isLaboratoryLoading!: boolean;

    @Getter("isLoading", { namespace: "encounter" })
    isEncounterLoading!: boolean;

    @Getter("isLoading", { namespace: "immunization" })
    isImmunizationLoading!: boolean;

    @Getter("isDeferredLoad", { namespace: "immunization" })
    isImmunizationDeferred!: boolean;

    @Getter("isLoading", { namespace: "note" })
    isNoteLoading!: boolean;

    @Getter("immunizations", { namespace: "immunization" })
    patientImmunizations!: ImmunizationEvent[];

    @Getter("covidImmunizations", { namespace: "immunization" })
    covidImmunizations!: ImmunizationEvent[];

    @Getter("patientEncounters", { namespace: "encounter" })
    patientEncounters!: Encounter[];

    @Getter("medicationStatements", { namespace: "medication" })
    medicationStatements!: MedicationStatementHistory[];

    @Getter("medicationRequests", { namespace: "medication" })
    medicationRequests!: MedicationRequest[];

    @Getter("covid19LaboratoryOrders", { namespace: "laboratory" })
    covid19LaboratoryOrders!: Covid19LaboratoryOrder[];

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

    @Getter("user", { namespace: "user" }) user!: User;

    @Getter("isHeaderShown", { namespace: "navbar" }) isHeaderShown!: boolean;

    private filterText = "";
    private logger!: ILogger;

    private breadcrumbItems: BreadcrumbItem[] = [
        {
            text: "Timeline",
            to: "/timeline",
            active: true,
            dataTestId: "breadcrumb-timeline",
        },
    ];

    @Watch("filterText")
    private onFilterTextChanged() {
        this.setKeyword(this.filterText);
    }

    private get timelineEntries(): TimelineEntry[] {
        this.logger.debug("Updating timeline Entries");

        let timelineEntries = [];
        // Add the medication request entries to the timeline list
        for (const medicationRequest of this.medicationRequests) {
            timelineEntries.push(
                new MedicationRequestTimelineEntry(
                    medicationRequest,
                    this.getEntryComments
                )
            );
        }

        // Add the medication entries to the timeline list
        for (const medication of this.medicationStatements) {
            timelineEntries.push(
                new MedicationTimelineEntry(medication, this.getEntryComments)
            );
        }

        // Add the COVID-19 Laboratory entries to the timeline list
        for (const order of this.covid19LaboratoryOrders) {
            timelineEntries.push(
                new Covid19LaboratoryOrderTimelineEntry(
                    order,
                    this.getEntryComments
                )
            );
        }

        // Add the Laboratory entries to the timeline list
        for (const order of this.laboratoryOrders) {
            timelineEntries.push(
                new LaboratoryOrderTimelineEntry(order, this.getEntryComments)
            );
        }

        // Add the Encounter entries to the timeline list
        for (const encounter of this.patientEncounters) {
            timelineEntries.push(
                new EncounterTimelineEntry(encounter, this.getEntryComments)
            );
        }

        // Add the Note entries to the timeline list
        for (const note of this.userNotes) {
            timelineEntries.push(new NoteTimelineEntry(note));
        }

        // Add the immunization entries to the timeline list
        for (const immunization of this.patientImmunizations) {
            timelineEntries.push(new ImmunizationTimelineEntry(immunization));
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

    private get isFullyLoaded(): boolean {
        return (
            !this.isMedicationRequestLoading &&
            !this.isMedicationStatementLoading &&
            !this.isImmunizationLoading &&
            !this.isImmunizationDeferred &&
            !this.isCovid19LaboratoryLoading &&
            !this.isLaboratoryLoading &&
            !this.isEncounterLoading &&
            !this.isNoteLoading &&
            !this.isCommentLoading
        );
    }

    private get showTimelineEntries(): boolean {
        return this.timelineEntries.length > 0 || this.isFullyLoaded;
    }

    private get isNoteEnabled(): boolean {
        return this.config.modules["Note"];
    }

    private created() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        this.fetchTimelineData();
    }

    private mounted() {
        this.filterText = this.keyword;
    }

    private fetchTimelineData() {
        Promise.all([
            this.retrievePatientData(),
            this.retrieveMedications({ hdid: this.user.hdid }),
            this.retrieveMedicationRequests({ hdid: this.user.hdid }),
            this.retrieveImmunizations({ hdid: this.user.hdid }),
            this.retrieveCovid19LaboratoryOrders({ hdid: this.user.hdid }),
            this.retrieveLaboratoryOrders({ hdid: this.user.hdid }),
            this.retrieveEncounters({ hdid: this.user.hdid }),
            this.retrieveNotes({ hdid: this.user.hdid }),
            this.retrieveComments({ hdid: this.user.hdid }),
        ]).catch((err) => {
            this.logger.error(`Error loading timeline data: ${err}`);
        });
    }

    private sortEntries(timelineEntries: TimelineEntry[]): TimelineEntry[] {
        return timelineEntries.sort((a, b) => {
            if (a.date.isBefore(b.date)) {
                return 1;
            }
            if (a.date.isAfter(b.date)) {
                return -1;
            }
            return 0;
        });
    }
}
</script>

<template>
    <div class="m-3 m-md-4 flex-grow-1 d-flex flex-column">
        <b-toast
            id="loading-toast"
            :visible="!isFullyLoaded"
            toaster="b-toaster-top-center"
            variant="info"
            solid
            no-auto-hide
            no-close-button
            is-status
            data-testid="loading-toast"
        >
            <div class="text-center">Retrieving your health records</div>
        </b-toast>
        <div
            v-if="!isFullyLoaded"
            v-show="false"
            data-testid="loading-in-progress"
        />
        <BreadcrumbComponent :items="breadcrumbItems" />
        <b-alert
            v-if="isLaboratoryQueued"
            show
            dismissible
            variant="info"
            class="no-print"
            data-testid="laboratory-orders-queued-alert-message"
        >
            <span>
                We are getting your lab results. It may take up to 48 hours
                until you can see them.
            </span>
        </b-alert>
        <b-row>
            <b-col id="timeline" class="col-12 col-lg-9 column-wrapper">
                <page-title title="Timeline">
                    <div class="float-right">
                        <add-note-button
                            v-if="isNoteEnabled && !isNoteLoading"
                        />
                    </div>
                </page-title>
                <div
                    v-if="showTimelineEntries"
                    class="sticky-top sticky-offset px-2 pb-2"
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
                                    />
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
                        <b-col class="col-auto pl-2">
                            <Filters />
                        </b-col>
                    </b-row>
                </div>
                <LinearTimeline
                    v-show="showTimelineEntries"
                    :timeline-entries="filteredTimelineEntries"
                />
                <b-row v-if="!showTimelineEntries">
                    <b-col>
                        <div class="px-2">
                            <content-placeholders>
                                <content-placeholders-heading :img="true" />
                                <content-placeholders-text :lines="3" />
                            </content-placeholders>
                        </div>
                    </b-col>
                </b-row>
            </b-col>
        </b-row>
        <resource-centre />
        <ProtectiveWordComponent :is-loading="isMedicationStatementLoading" />
        <NoteEditComponent :is-loading="isNoteLoading" />
        <EntryDetailsComponent />
    </div>
</template>

<style lang="scss" scoped>
@use "sass:math";
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

hr {
    border-top: 2px solid $primary;
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
    $icon-padding: math.div($icon-size-padded - $icon-size, 2);

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
