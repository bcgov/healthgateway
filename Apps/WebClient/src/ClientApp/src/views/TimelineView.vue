<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import {
    faCheckCircle,
    faEdit,
    faFileMedical,
    faFileWaveform,
    faHouseMedical,
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
import { Component } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import NoteEditComponent from "@/components/modal/NoteEditComponent.vue";
import ProtectiveWordComponent from "@/components/modal/ProtectiveWordComponent.vue";
import BreadcrumbComponent from "@/components/navmenu/BreadcrumbComponent.vue";
import AddNoteButtonComponent from "@/components/timeline/AddNoteButtonComponent.vue";
import EntryDetailsComponent from "@/components/timeline/entryCard/EntryDetailsComponent.vue";
import FilterComponent from "@/components/timeline/FilterComponent.vue";
import LinearTimelineComponent from "@/components/timeline/LinearTimelineComponent.vue";
import { EntryType, entryTypeMap } from "@/constants/entryType";
import BreadcrumbItem from "@/models/breadcrumbItem";
import { ClinicalDocument } from "@/models/clinicalDocument";
import ClinicalDocumentTimelineEntry from "@/models/clinicalDocumentTimelineEntry";
import type { WebClientConfiguration } from "@/models/configData";
import Covid19LaboratoryOrderTimelineEntry from "@/models/covid19LaboratoryOrderTimelineEntry";
import { DateWrapper } from "@/models/dateWrapper";
import { Encounter, HospitalVisit } from "@/models/encounter";
import EncounterTimelineEntry from "@/models/encounterTimelineEntry";
import HospitalVisitTimelineEntry from "@/models/hospitalVisitTimelineEntry";
import { ImmunizationEvent } from "@/models/immunizationModel";
import ImmunizationTimelineEntry from "@/models/immunizationTimelineEntry";
import { Covid19LaboratoryOrder, LaboratoryOrder } from "@/models/laboratory";
import LaboratoryOrderTimelineEntry from "@/models/laboratoryOrderTimelineEntry";
import MedicationRequest from "@/models/medicationRequest";
import MedicationRequestTimelineEntry from "@/models/medicationRequestTimelineEntry";
import MedicationStatementHistory from "@/models/medicationStatementHistory";
import MedicationTimelineEntry from "@/models/medicationTimelineEntry";
import NoteTimelineEntry from "@/models/noteTimelineEntry";
import TimelineEntry from "@/models/timelineEntry";
import TimelineFilter, { TimelineFilterBuilder } from "@/models/timelineFilter";
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
    faFileWaveform,
    faHouseMedical,
    faMicroscope,
    faPills,
    faQuestion,
    faSearch,
    faStethoscope,
    faSyringe,
    faVial
);

enum FilterLabelType {
    Keyword = "Keyword",
    Type = "Record Type",
    Date = "Date",
}

// eslint-disable-next-line @typescript-eslint/no-explicit-any
const options: any = {
    components: {
        BToast,
        BreadcrumbComponent,
        ProtectiveWordComponent,
        NoteEditComponent,
        EntryDetailsComponent,
        LinearTimeline: LinearTimelineComponent,
        Filters: FilterComponent,
        "add-note-button": AddNoteButtonComponent,
    },
};

@Component(options)
export default class TimelineView extends Vue {
    @Getter("webClient", { namespace: "config" })
    config!: WebClientConfiguration;

    @Action("retrieve", { namespace: "immunization" })
    retrieveImmunizations!: (params: { hdid: string }) => Promise<void>;

    @Action("retrieveHealthVisits", { namespace: "encounter" })
    retrieveHealthVisits!: (params: { hdid: string }) => Promise<void>;

    @Action("retrieveHospitalVisits", { namespace: "encounter" })
    retrieveHospitalVisits!: (params: { hdid: string }) => Promise<void>;

    @Action("retrieve", { namespace: "note" })
    retrieveNotes!: (params: { hdid: string }) => Promise<void>;

    @Action("retrieveCovid19LaboratoryOrders", { namespace: "laboratory" })
    retrieveCovid19LaboratoryOrders!: (params: {
        hdid: string;
    }) => Promise<void>;

    @Action("retrieveLaboratoryOrders", { namespace: "laboratory" })
    retrieveLaboratoryOrders!: (params: { hdid: string }) => Promise<void>;

    @Action("retrieveMedications", { namespace: "medication" })
    retrieveMedications!: (params: {
        hdid: string;
        protectiveWord?: string;
    }) => Promise<void>;

    @Action("retrieveSpecialAuthorityRequests", { namespace: "medication" })
    retrieveSpecialAuthorityRequests!: (params: {
        hdid: string;
    }) => Promise<void>;

    @Action("retrieve", { namespace: "clinicalDocument" })
    retrieveClinicalDocuments!: (params: { hdid: string }) => Promise<void>;

    @Action("retrieve", { namespace: "comment" })
    retrieveComments!: (params: { hdid: string }) => Promise<void>;

    @Action("setFilter", { namespace: "timeline" })
    setFilter!: (filterBuilder: TimelineFilterBuilder) => void;

    @Getter("medicationsAreLoading", { namespace: "medication" })
    medicationsAreLoading!: (hdid: string) => boolean;

    @Getter("specialAuthorityRequestsAreLoading", { namespace: "medication" })
    specialAuthorityRequestsAreLoading!: (hdid: string) => boolean;

    @Getter("isLoading", { namespace: "comment" })
    isCommentLoading!: boolean;

    @Getter("covid19LaboratoryOrdersAreLoading", { namespace: "laboratory" })
    covid19LaboratoryOrdersAreLoading!: (hdid: string) => boolean;

    @Getter("laboratoryOrdersAreQueued", { namespace: "laboratory" })
    laboratoryOrdersAreQueued!: (hdid: string) => boolean;

    @Getter("laboratoryOrdersAreLoading", { namespace: "laboratory" })
    laboratoryOrdersAreLoading!: (hdid: string) => boolean;

    @Getter("healthVisitsAreLoading", { namespace: "encounter" })
    healthVisitsAreLoading!: (hdid: string) => boolean;

    @Getter("hospitalVisitsAreLoading", { namespace: "encounter" })
    hospitalVisitsAreLoading!: (hdid: string) => boolean;

    @Getter("isLoading", { namespace: "immunization" })
    isImmunizationLoading!: (hdid: string) => boolean;

    @Getter("isDeferredLoad", { namespace: "immunization" })
    isImmunizationDeferred!: (hdid: string) => boolean;

    @Getter("isLoading", { namespace: "note" })
    isNoteLoading!: boolean;

    @Getter("isLoading", { namespace: "clinicalDocument" })
    isClinicalDocumentLoading!: (hdid: string) => boolean;

    @Getter("immunizations", { namespace: "immunization" })
    patientImmunizations!: (hdid: string) => ImmunizationEvent[];

    @Getter("healthVisits", { namespace: "encounter" })
    healthVisits!: (hdid: string) => Encounter[];

    @Getter("hospitalVisits", { namespace: "encounter" })
    hospitalVisits!: (hdid: string) => HospitalVisit[];

    @Getter("medications", { namespace: "medication" })
    medications!: (hdid: string) => MedicationStatementHistory[];

    @Getter("specialAuthorityRequests", { namespace: "medication" })
    specialAuthorityRequests!: (hdid: string) => MedicationRequest[];

    @Getter("covid19LaboratoryOrders", { namespace: "laboratory" })
    covid19LaboratoryOrders!: (hdid: string) => Covid19LaboratoryOrder[];

    @Getter("laboratoryOrders", { namespace: "laboratory" })
    laboratoryOrders!: (hdid: string) => LaboratoryOrder[];

    @Getter("records", { namespace: "clinicalDocument" })
    clinicalDocuments!: (hdid: string) => ClinicalDocument[];

    @Getter("notes", { namespace: "note" })
    userNotes!: UserNote[];

    @Getter("getEntryComments", { namespace: "comment" })
    getEntryComments!: (entyId: string) => UserComment[] | null;

    @Getter("filter", { namespace: "timeline" })
    filter!: TimelineFilter;

    @Getter("entryTypes", { namespace: "timeline" })
    entryTypes!: Set<EntryType>;

    @Getter("user", { namespace: "user" }) user!: User;

    @Getter("isHeaderShown", { namespace: "navbar" }) isHeaderShown!: boolean;

    private logger!: ILogger;

    private breadcrumbItems: BreadcrumbItem[] = [
        {
            text: "Timeline",
            to: "/timeline",
            active: true,
            dataTestId: "breadcrumb-timeline",
        },
    ];

    private get isLaboratoryQueued(): boolean {
        return this.laboratoryOrdersAreQueued(this.user.hdid);
    }

    private get timelineEntries(): TimelineEntry[] {
        this.logger.debug("Updating timeline Entries");

        let timelineEntries = [];

        // Add the Special Authority request entries to the timeline list
        for (const request of this.specialAuthorityRequests(this.user.hdid)) {
            timelineEntries.push(
                new MedicationRequestTimelineEntry(
                    request,
                    this.getEntryComments
                )
            );
        }

        // Add the medication entries to the timeline list
        for (const medication of this.medications(this.user.hdid)) {
            timelineEntries.push(
                new MedicationTimelineEntry(medication, this.getEntryComments)
            );
        }

        // Add the COVID-19 laboratory entries to the timeline list
        for (const order of this.covid19LaboratoryOrders(this.user.hdid)) {
            timelineEntries.push(
                new Covid19LaboratoryOrderTimelineEntry(
                    order,
                    this.getEntryComments
                )
            );
        }

        // Add the laboratory entries to the timeline list
        for (const order of this.laboratoryOrders(this.user.hdid)) {
            timelineEntries.push(
                new LaboratoryOrderTimelineEntry(order, this.getEntryComments)
            );
        }

        // Add the health visit entries to the timeline list
        for (const healthVisit of this.healthVisits(this.user.hdid)) {
            timelineEntries.push(
                new EncounterTimelineEntry(healthVisit, this.getEntryComments)
            );
        }

        // Add the hospital visit entries to the timeline list
        for (const visit of this.hospitalVisits(this.user.hdid)) {
            timelineEntries.push(
                new HospitalVisitTimelineEntry(visit, this.getEntryComments)
            );
        }

        // Add the clinical document entries to the timeline list
        for (const clinicalDocument of this.clinicalDocuments(this.user.hdid)) {
            timelineEntries.push(
                new ClinicalDocumentTimelineEntry(
                    clinicalDocument,
                    this.getEntryComments
                )
            );
        }

        // Add the Note entries to the timeline list
        for (const note of this.userNotes) {
            timelineEntries.push(new NoteTimelineEntry(note));
        }

        // Add the immunization entries to the timeline list
        for (const immunization of this.patientImmunizations(this.user.hdid)) {
            timelineEntries.push(new ImmunizationTimelineEntry(immunization));
        }

        timelineEntries = this.sortEntries(timelineEntries);
        return timelineEntries;
    }

    public get filteredTimelineEntries(): TimelineEntry[] {
        let filteredEntries = [];

        if (this.filter.hasActiveFilter()) {
            filteredEntries = this.timelineEntries.filter((entry) =>
                entry.filterApplies(this.filter)
            );
        } else {
            filteredEntries = this.timelineEntries;
        }

        return filteredEntries;
    }

    private get isFullyLoaded(): boolean {
        return (
            !this.specialAuthorityRequestsAreLoading(this.user.hdid) &&
            !this.medicationsAreLoading(this.user.hdid) &&
            !this.covid19LaboratoryOrdersAreLoading(this.user.hdid) &&
            !this.laboratoryOrdersAreLoading(this.user.hdid) &&
            !this.isImmunizationLoading(this.user.hdid) &&
            !this.isImmunizationDeferred(this.user.hdid) &&
            !this.healthVisitsAreLoading(this.user.hdid) &&
            !this.hospitalVisitsAreLoading(this.user.hdid) &&
            !this.isClinicalDocumentLoading(this.user.hdid) &&
            !this.isNoteLoading &&
            !this.isCommentLoading
        );
    }

    private get isFilterLoading(): boolean {
        const filtersLoaded = [];
        filtersLoaded.push(
            this.isSelectedFilterModuleLoading(
                EntryType.MedicationRequest,
                this.specialAuthorityRequestsAreLoading(this.user.hdid)
            )
        );

        filtersLoaded.push(
            this.isSelectedFilterModuleLoading(
                EntryType.Medication,
                this.medicationsAreLoading(this.user.hdid)
            )
        );

        filtersLoaded.push(
            this.isSelectedFilterModuleLoading(
                EntryType.Immunization,
                this.isImmunizationLoading(this.user.hdid)
            )
        );

        filtersLoaded.push(
            this.isSelectedFilterModuleLoading(
                EntryType.Covid19LaboratoryOrder,
                this.covid19LaboratoryOrdersAreLoading(this.user.hdid)
            )
        );

        filtersLoaded.push(
            this.isSelectedFilterModuleLoading(
                EntryType.LaboratoryOrder,
                this.laboratoryOrdersAreLoading(this.user.hdid)
            )
        );

        filtersLoaded.push(
            this.isSelectedFilterModuleLoading(
                EntryType.Encounter,
                this.healthVisitsAreLoading(this.user.hdid)
            )
        );

        filtersLoaded.push(
            this.isSelectedFilterModuleLoading(
                EntryType.HospitalVisit,
                this.hospitalVisitsAreLoading(this.user.hdid)
            )
        );

        filtersLoaded.push(
            this.isSelectedFilterModuleLoading(
                EntryType.Note,
                this.isNoteLoading
            )
        );

        filtersLoaded.push(
            this.isSelectedFilterModuleLoading(
                EntryType.ClinicalDocument,
                this.isClinicalDocumentLoading(this.user.hdid)
            )
        );

        const filterLoading = filtersLoaded.includes(true);
        this.logger.debug(`Timeline filter loading: ${filterLoading}`);

        return filterLoading;
    }

    private get isFilterModuleSelected(): boolean {
        const entryTypes = Array.from(this.entryTypes);
        this.logger.debug(
            `Number of imeline filter modules selected: ${entryTypes.length}`
        );
        return entryTypes.length > 0;
    }

    private get showContentPlaceholders(): boolean {
        if (this.isFilterModuleSelected) {
            return this.isFilterLoading;
        }
        return !this.isFullyLoaded && this.filteredTimelineEntries.length === 0;
    }

    private get showTimelineEntries(): boolean {
        return this.timelineEntries.length > 0 || this.isFullyLoaded;
    }

    private get isNoteEnabled(): boolean {
        return this.config.modules["Note"];
    }

    private get filterLabels(): [string, string][] {
        const labels: [string, string][] = [];

        if (this.filter.keyword) {
            labels.push([FilterLabelType.Keyword, `"${this.filter.keyword}"`]);
        }

        this.entryTypes.forEach((entryType) => {
            const label = entryTypeMap.get(entryType)?.name;
            if (label) {
                labels.push([FilterLabelType.Type, label]);
            }
        });

        const startDate = this.filter.startDate
            ? `From ${new DateWrapper(this.filter.startDate).format()}`
            : "";
        const endDate = this.filter.endDate
            ? `To ${new DateWrapper(this.filter.endDate).format()}`
            : "";
        if (startDate && endDate) {
            labels.push([FilterLabelType.Date, `${startDate} ${endDate}`]);
        } else if (startDate) {
            labels.push([FilterLabelType.Date, startDate]);
        } else if (endDate) {
            labels.push([FilterLabelType.Date, endDate]);
        }

        return labels;
    }

    private created(): void {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        this.fetchTimelineData();
    }

    private fetchTimelineData(): void {
        Promise.all([
            this.retrieveMedications({ hdid: this.user.hdid }),
            this.retrieveSpecialAuthorityRequests({ hdid: this.user.hdid }),
            this.retrieveImmunizations({ hdid: this.user.hdid }),
            this.retrieveCovid19LaboratoryOrders({ hdid: this.user.hdid }),
            this.retrieveLaboratoryOrders({ hdid: this.user.hdid }),
            this.retrieveHealthVisits({ hdid: this.user.hdid }),
            this.retrieveHospitalVisits({ hdid: this.user.hdid }),
            this.retrieveClinicalDocuments({ hdid: this.user.hdid }),
            this.retrieveNotes({ hdid: this.user.hdid }),
            this.retrieveComments({ hdid: this.user.hdid }),
        ]).catch((err) =>
            this.logger.error(`Error loading timeline data: ${err}`)
        );
    }

    private isFilterApplied(entryType: EntryType): boolean {
        const entryTypes = Array.from(this.entryTypes);
        const filterApplied = !!entryTypes.includes(entryType);
        this.logger.debug(
            `Timeline filter entry type: ${entryType} applied: ${filterApplied}`
        );
        return filterApplied;
    }

    private isSelectedFilterModuleLoading(
        entryType: EntryType,
        loading: boolean
    ): boolean {
        const filterApplied = this.isFilterApplied(entryType);
        const isLoading = filterApplied && loading;
        this.logger.debug(
            `Timeline filter entry type: ${entryType} applied: ${filterApplied} - filter loading: ${loading} and filter isLoading: ${isLoading}`
        );
        return isLoading;
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

    private clearFilter(label: string, value: string | undefined): void {
        let keyword = this.filter.keyword;
        let startDate = this.filter.startDate;
        let endDate = this.filter.endDate;
        let entryTypes = [...this.entryTypes];

        switch (label) {
            case FilterLabelType.Keyword:
                keyword = "";
                break;
            case FilterLabelType.Date:
                startDate = "";
                endDate = "";
                break;
            case FilterLabelType.Type:
                entryTypes = entryTypes.filter(
                    (e) => entryTypeMap.get(e)?.name !== value
                );
                break;
        }

        const builder = TimelineFilterBuilder.create()
            .withKeyword(keyword)
            .withStartDate(startDate)
            .withEndDate(endDate)
            .withEntryTypes(entryTypes);

        this.setFilter(builder);
    }

    private clearFilters(): void {
        const builder = TimelineFilterBuilder.create();
        this.setFilter(builder);
    }
}
</script>

<template>
    <div>
        <b-toast
            v-if="!isFullyLoaded"
            id="loading-toast"
            visible
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
        <page-title title="Timeline">
            <div class="float-right">
                <add-note-button v-if="isNoteEnabled && !isNoteLoading" />
            </div>
        </page-title>
        <div
            v-if="showTimelineEntries"
            class="sticky-top sticky-offset"
            :class="{ 'header-offset': isHeaderShown }"
        >
            <b-row
                class="no-print justify-content-between py-2"
                align-v="start"
            >
                <b-col class="col-auto">
                    <Filters class="my-1" />
                </b-col>
                <b-col class="mx-2">
                    <b-form-tag
                        v-for="[label, value] in filterLabels"
                        :key="`${label}-${value}`"
                        variant="light"
                        class="filter-label p-1 mr-2 my-1"
                        :title="`${label} Filter`"
                        data-testid="filter-label"
                        @remove="clearFilter(label, value)"
                        >{{ value }}</b-form-tag
                    >
                    <hg-button
                        v-if="filterLabels.length > 0"
                        class="p-1 mt-n1"
                        data-testid="clear-filters-button"
                        variant="link"
                        @click="clearFilters"
                    >
                        Clear All
                    </hg-button>
                </b-col>
            </b-row>
        </div>
        <LinearTimeline
            v-show="showTimelineEntries"
            :timeline-entries="filteredTimelineEntries"
        />
        <content-placeholders
            v-if="showContentPlaceholders"
            data-testid="content-placeholders"
        >
            <content-placeholders-heading :img="true" />
            <content-placeholders-text :lines="3" />
        </content-placeholders>
        <ProtectiveWordComponent
            :is-loading="medicationsAreLoading(user.hdid)"
        />
        <NoteEditComponent :is-loading="isNoteLoading" />
        <EntryDetailsComponent />
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

hr {
    border-top: 2px solid $primary;
}

.form-group {
    margin-bottom: 0px;
}

.sticky-offset {
    background-color: white;
    z-index: 2;

    &.header-offset {
        top: $header-height;
    }
}

.btn-light {
    border-color: $primary;
    color: $primary;
}

.z-index-large {
    z-index: 50;
}

.filter-label {
    font-size: 1rem;
}
</style>
