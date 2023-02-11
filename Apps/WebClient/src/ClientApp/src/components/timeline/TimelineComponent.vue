<script lang="ts">
import { BToast } from "bootstrap-vue";
import Vue from "vue";
import { Component, Prop, Watch } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import ProtectiveWordComponent from "@/components/modal/ProtectiveWordComponent.vue";
import EntryDetailsComponent from "@/components/timeline/entryCard/EntryDetailsComponent.vue";
import FilterComponent from "@/components/timeline/FilterComponent.vue";
import { EntryType, entryTypeMap } from "@/constants/entryType";
import { ClinicalDocument } from "@/models/clinicalDocument";
import ClinicalDocumentTimelineEntry from "@/models/clinicalDocumentTimelineEntry";
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
import TimelineEntry, { DateGroup } from "@/models/timelineEntry";
import TimelineFilter, { TimelineFilterBuilder } from "@/models/timelineFilter";
import { UserComment } from "@/models/userComment";
import UserNote from "@/models/userNote";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger } from "@/services/interfaces";

import ClinicalDocumentTimelineComponent from "./entryCard/ClinicalDocumentTimelineComponent.vue";
import Covid19LaboratoryOrderTimelineComponent from "./entryCard/Covid19LaboratoryOrderTimelineComponent.vue";
import EncounterTimelineComponent from "./entryCard/EncounterTimelineComponent.vue";
import HospitalVisitTimelineComponent from "./entryCard/HospitalVisitTimelineComponent.vue";
import ImmunizationTimelineComponent from "./entryCard/ImmunizationTimelineComponent.vue";
import LaboratoryOrderTimelineComponent from "./entryCard/LaboratoryOrderTimelineComponent.vue";
import MedicationRequestTimelineComponent from "./entryCard/MedicationRequestTimelineComponent.vue";
import MedicationTimelineComponent from "./entryCard/MedicationTimelineComponent.vue";
import NoteTimelineComponent from "./entryCard/NoteTimelineComponent.vue";

enum FilterLabelType {
    Keyword = "Keyword",
    Type = "Record Type",
    Date = "Date",
}

// eslint-disable-next-line @typescript-eslint/no-explicit-any
const options: any = {
    components: {
        BToast,
        ClinicalDocumentTimelineComponent,
        Covid19LaboratoryOrderTimelineComponent,
        EncounterTimelineComponent,
        EntryDetailsComponent,
        FilterComponent,
        HospitalVisitTimelineComponent,
        ImmunizationTimelineComponent,
        LaboratoryOrderTimelineComponent,
        MedicationRequestTimelineComponent,
        MedicationTimelineComponent,
        NoteTimelineComponent,
        ProtectiveWordComponent,
    },
};

@Component(options)
export default class TimelineComponent extends Vue {
    @Prop({ required: true })
    hdid!: string;

    @Prop({ default: [] })
    entryTypes!: EntryType[];

    @Prop({ default: false })
    commentsAreEnabled!: boolean;

    @Action("retrieveComments", { namespace: "comment" })
    retrieveComments!: (params: { hdid: string }) => Promise<void>;

    @Action("retrieveClinicalDocuments", { namespace: "clinicalDocument" })
    retrieveClinicalDocuments!: (params: { hdid: string }) => Promise<void>;

    @Action("retrieveHealthVisits", { namespace: "encounter" })
    retrieveHealthVisits!: (params: { hdid: string }) => Promise<void>;

    @Action("retrieveHospitalVisits", { namespace: "encounter" })
    retrieveHospitalVisits!: (params: { hdid: string }) => Promise<void>;

    @Action("retrieveImmunizations", { namespace: "immunization" })
    retrieveImmunizations!: (params: { hdid: string }) => Promise<void>;

    @Action("retrieveCovid19LaboratoryOrders", { namespace: "laboratory" })
    retrieveCovid19LaboratoryOrders!: (params: {
        hdid: string;
    }) => Promise<void>;

    @Action("retrieveLaboratoryOrders", { namespace: "laboratory" })
    retrieveLaboratoryOrders!: (params: { hdid: string }) => Promise<void>;

    @Action("retrieveNotes", { namespace: "note" })
    retrieveNotes!: (params: { hdid: string }) => Promise<void>;

    @Action("retrieveMedications", { namespace: "medication" })
    retrieveMedications!: (params: {
        hdid: string;
        protectiveWord?: string;
    }) => Promise<void>;

    @Action("retrieveSpecialAuthorityRequests", { namespace: "medication" })
    retrieveSpecialAuthorityRequests!: (params: {
        hdid: string;
    }) => Promise<void>;

    @Action("setFilter", { namespace: "timeline" })
    setFilter!: (filterBuilder: TimelineFilterBuilder) => void;

    @Action("setLinearDate", { namespace: "timeline" })
    setLinearDate!: (linearDate: DateWrapper) => void;

    @Getter("clinicalDocuments", { namespace: "clinicalDocument" })
    clinicalDocuments!: (hdid: string) => ClinicalDocument[];

    @Getter("clinicalDocumentsAreLoading", { namespace: "clinicalDocument" })
    clinicalDocumentsAreLoading!: (hdid: string) => boolean;

    @Getter("getEntryComments", { namespace: "comment" })
    getEntryComments!: (entryId: string) => UserComment[] | null;

    @Getter("commentsAreLoading", { namespace: "comment" })
    commentsAreLoading!: boolean;

    @Getter("healthVisits", { namespace: "encounter" })
    healthVisits!: (hdid: string) => Encounter[];

    @Getter("healthVisitsAreLoading", { namespace: "encounter" })
    healthVisitsAreLoading!: (hdid: string) => boolean;

    @Getter("hospitalVisits", { namespace: "encounter" })
    hospitalVisits!: (hdid: string) => HospitalVisit[];

    @Getter("hospitalVisitsAreLoading", { namespace: "encounter" })
    hospitalVisitsAreLoading!: (hdid: string) => boolean;

    @Getter("immunizations", { namespace: "immunization" })
    patientImmunizations!: (hdid: string) => ImmunizationEvent[];

    @Getter("immunizationsAreLoading", { namespace: "immunization" })
    immunizationsAreLoading!: (hdid: string) => boolean;

    @Getter("immunizationsAreDeferred", { namespace: "immunization" })
    immunizationsAreDeferred!: (hdid: string) => boolean;

    @Getter("covid19LaboratoryOrders", { namespace: "laboratory" })
    covid19LaboratoryOrders!: (hdid: string) => Covid19LaboratoryOrder[];

    @Getter("covid19LaboratoryOrdersAreLoading", { namespace: "laboratory" })
    covid19LaboratoryOrdersAreLoading!: (hdid: string) => boolean;

    @Getter("laboratoryOrders", { namespace: "laboratory" })
    laboratoryOrders!: (hdid: string) => LaboratoryOrder[];

    @Getter("laboratoryOrdersAreLoading", { namespace: "laboratory" })
    laboratoryOrdersAreLoading!: (hdid: string) => boolean;

    @Getter("laboratoryOrdersAreQueued", { namespace: "laboratory" })
    laboratoryOrdersAreQueued!: (hdid: string) => boolean;

    @Getter("medications", { namespace: "medication" })
    medications!: (hdid: string) => MedicationStatementHistory[];

    @Getter("medicationsAreLoading", { namespace: "medication" })
    medicationsAreLoading!: (hdid: string) => boolean;

    @Getter("specialAuthorityRequests", { namespace: "medication" })
    specialAuthorityRequests!: (hdid: string) => MedicationRequest[];

    @Getter("specialAuthorityRequestsAreLoading", { namespace: "medication" })
    specialAuthorityRequestsAreLoading!: (hdid: string) => boolean;

    @Getter("isHeaderShown", { namespace: "navbar" })
    isHeaderShown!: boolean;

    @Getter("notes", { namespace: "note" })
    userNotes!: UserNote[];

    @Getter("notesAreLoading", { namespace: "note" })
    notesAreLoading!: boolean;

    @Getter("selectedEntryTypes", { namespace: "timeline" })
    selectedEntryTypes!: Set<EntryType>;

    @Getter("filter", { namespace: "timeline" })
    filter!: TimelineFilter;

    @Getter("hasActiveFilter", { namespace: "timeline" })
    hasActiveFilter!: boolean;

    @Getter("linearDate", { namespace: "timeline" })
    linearDate!: DateWrapper;

    @Getter("selectedDate", { namespace: "timeline" })
    selectedDate!: DateWrapper | null;

    currentPage = 1;
    readonly pageSize = 25;
    logger!: ILogger;

    get dateGroups(): DateGroup[] {
        if (this.visibleTimelineEntries.length === 0) {
            return [];
        }

        let newGroupArray = DateGroup.createGroups(this.visibleTimelineEntries);
        return DateGroup.sortGroups(newGroupArray);
    }

    get filterLabels(): [string, string][] {
        const labels: [string, string][] = [];

        if (this.filter.keyword) {
            labels.push([FilterLabelType.Keyword, `"${this.filter.keyword}"`]);
        }

        this.selectedEntryTypes.forEach((entryType) => {
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

    get filteredTimelineEntries(): TimelineEntry[] {
        let entries = this.unfilteredTimelineEntries;
        if (this.filter.hasActiveFilter()) {
            entries = entries.filter((e) => e.filterApplies(this.filter));
        }
        return entries;
    }

    get selectedDatasetsAreLoading(): boolean {
        return this.entryTypes.some(
            (entryType) =>
                this.selectedEntryTypes.has(entryType) &&
                this.datasetIsLoading(entryType)
        );
    }

    get isFullyLoaded(): boolean {
        const loadingDatasets = this.entryTypes.some((entryType) =>
            this.datasetIsLoading(entryType)
        );

        const loadingComments =
            this.commentsAreEnabled && this.commentsAreLoading;

        const loadingMoreImmunizations =
            this.entryTypes.includes(EntryType.Immunization) &&
            this.immunizationsAreDeferred(this.hdid);

        return (
            !loadingDatasets && !loadingComments && !loadingMoreImmunizations
        );
    }

    get isOnlyImmunizationSelected(): boolean {
        return (
            this.selectedEntryTypes.size === 1 &&
            this.selectedEntryTypes.has(EntryType.Immunization)
        );
    }

    get numberOfPages(): number {
        let pageCount = 1;
        if (this.filteredTimelineEntries.length > this.pageSize) {
            pageCount = Math.ceil(
                this.filteredTimelineEntries.length / this.pageSize
            );
        }
        return pageCount;
    }

    get showContentPlaceholders(): boolean {
        if (this.selectedEntryTypes.size > 0) {
            return this.selectedDatasetsAreLoading;
        }
        return !this.isFullyLoaded && this.filteredTimelineEntries.length === 0;
    }

    get showDisplayCount(): boolean {
        return this.visibleTimelineEntries.length > 0;
    }

    get showEmptyState(): boolean {
        return (
            this.filteredTimelineEntries.length === 0 &&
            !this.selectedDatasetsAreLoading
        );
    }

    get showTimelineEntries(): boolean {
        return this.unfilteredTimelineEntries.length > 0 || this.isFullyLoaded;
    }

    get unfilteredTimelineEntries(): TimelineEntry[] {
        const getComments = this.getEntryComments;
        let entries = [];

        // Add the Special Authority request entries to the timeline list
        for (const request of this.specialAuthorityRequests(this.hdid)) {
            entries.push(
                new MedicationRequestTimelineEntry(request, getComments)
            );
        }

        // Add the medication entries to the timeline list
        for (const medication of this.medications(this.hdid)) {
            entries.push(new MedicationTimelineEntry(medication, getComments));
        }

        // Add the COVID-19 test result entries to the timeline list
        for (const result of this.covid19LaboratoryOrders(this.hdid)) {
            entries.push(
                new Covid19LaboratoryOrderTimelineEntry(result, getComments)
            );
        }

        // Add the lab result entries to the timeline list
        for (const order of this.laboratoryOrders(this.hdid)) {
            entries.push(new LaboratoryOrderTimelineEntry(order, getComments));
        }

        // Add the health visit entries to the timeline list
        for (const healthVisit of this.healthVisits(this.hdid)) {
            entries.push(new EncounterTimelineEntry(healthVisit, getComments));
        }

        // Add the hospital visit entries to the timeline list
        for (const visit of this.hospitalVisits(this.hdid)) {
            entries.push(new HospitalVisitTimelineEntry(visit, getComments));
        }

        // Add the clinical document entries to the timeline list
        for (const doc of this.clinicalDocuments(this.hdid)) {
            entries.push(new ClinicalDocumentTimelineEntry(doc, getComments));
        }

        // Add the note entries to the timeline list
        for (const note of this.userNotes) {
            entries.push(new NoteTimelineEntry(note));
        }

        // Add the immunization entries to the timeline list
        for (const immunization of this.patientImmunizations(this.hdid)) {
            entries.push(new ImmunizationTimelineEntry(immunization));
        }

        // Sort entries with newest first
        entries.sort((a, b) => {
            if (a.date.isBefore(b.date)) {
                return 1;
            }
            if (a.date.isAfter(b.date)) {
                return -1;
            }
            return 0;
        });

        return entries;
    }

    get visibleTimelineEntries(): TimelineEntry[] {
        if (this.filteredTimelineEntries.length === 0) {
            return [];
        }

        // Handle the current page being beyond the max number of pages
        if (this.currentPage > this.numberOfPages) {
            this.currentPage = this.numberOfPages;
        }

        // Get the section of the array that contains the paginated section
        let lowerIndex = (this.currentPage - 1) * this.pageSize;
        let upperIndex = Math.min(
            this.currentPage * this.pageSize,
            this.filteredTimelineEntries.length
        );
        return this.filteredTimelineEntries.slice(lowerIndex, upperIndex);
    }

    @Watch("currentPage")
    onCurrentPage(): void {
        if (this.visibleTimelineEntries.length > 0) {
            // Update the store
            this.setLinearDate(this.visibleTimelineEntries[0].date);
        }
    }

    @Watch("selectedDate")
    onSelectedDate(): void {
        if (
            this.selectedDate !== null &&
            this.setPageFromDate(this.selectedDate)
        ) {
            // Wait for next render cycle until the pages have been calculated and displayed
            this.$nextTick().then(() =>
                this.focusOnDate(this.selectedDate as DateWrapper)
            );
        }
    }

    created(): void {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        this.fetchTimelineData();
    }

    mounted(): void {
        this.setPageFromDate(this.linearDate);
    }

    clearFilter(label: string, value: string | undefined): void {
        let keyword = this.filter.keyword;
        let startDate = this.filter.startDate;
        let endDate = this.filter.endDate;
        let entryTypes = [...this.selectedEntryTypes];

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

    clearFilters(): void {
        const builder = TimelineFilterBuilder.create();
        this.setFilter(builder);
    }

    datasetIsLoading(entryType: EntryType): boolean {
        switch (entryType) {
            case EntryType.ClinicalDocument:
                return this.clinicalDocumentsAreLoading(this.hdid);
            case EntryType.Covid19TestResult:
                return this.covid19LaboratoryOrdersAreLoading(this.hdid);
            case EntryType.HealthVisit:
                return this.healthVisitsAreLoading(this.hdid);
            case EntryType.HospitalVisit:
                return this.hospitalVisitsAreLoading(this.hdid);
            case EntryType.Immunization:
                return this.immunizationsAreLoading(this.hdid);
            case EntryType.LabResult:
                return this.laboratoryOrdersAreLoading(this.hdid);
            case EntryType.Medication:
                return this.medicationsAreLoading(this.hdid);
            case EntryType.Note:
                return this.notesAreLoading;
            case EntryType.SpecialAuthorityRequest:
                return this.specialAuthorityRequestsAreLoading(this.hdid);
            default:
                throw new Error(`Unknown dataset "${entryType}"`);
        }
    }

    fetchDataset(entryType: EntryType): Promise<void> {
        switch (entryType) {
            case EntryType.ClinicalDocument:
                return this.retrieveClinicalDocuments({ hdid: this.hdid });
            case EntryType.Covid19TestResult:
                return this.retrieveCovid19LaboratoryOrders({
                    hdid: this.hdid,
                });
            case EntryType.HealthVisit:
                return this.retrieveHealthVisits({ hdid: this.hdid });
            case EntryType.HospitalVisit:
                return this.retrieveHospitalVisits({ hdid: this.hdid });
            case EntryType.Immunization:
                return this.retrieveImmunizations({ hdid: this.hdid });
            case EntryType.LabResult:
                return this.retrieveLaboratoryOrders({ hdid: this.hdid });
            case EntryType.Medication:
                return this.retrieveMedications({ hdid: this.hdid });
            case EntryType.Note:
                return this.retrieveNotes({ hdid: this.hdid });
            case EntryType.SpecialAuthorityRequest:
                return this.retrieveSpecialAuthorityRequests({
                    hdid: this.hdid,
                });
            default:
                return Promise.reject(`Unknown dataset "${entryType}"`);
        }
    }

    fetchTimelineData(): void {
        const promises = this.entryTypes.map((entryType) =>
            this.fetchDataset(entryType)
        );

        if (this.commentsAreEnabled) {
            promises.push(this.retrieveComments({ hdid: this.hdid }));
        }

        Promise.all(promises).catch((err) =>
            this.logger.error(`Error loading timeline data: ${err}`)
        );
    }

    focusOnDate(date: DateWrapper): void {
        const dateEpoch = date.fromEpoch();
        const container = this.$refs[dateEpoch] as HTMLElement[];
        container[0].querySelector("button")?.focus();
    }

    getComponentForEntry(entryType: EntryType): string {
        return entryTypeMap.get(entryType)?.component ?? "";
    }

    linkGen(pageNum: number): string {
        return `?page=${pageNum}`;
    }

    setPageFromDate(eventDate: DateWrapper): boolean {
        let index = this.filteredTimelineEntries.findIndex((entry) => {
            entry.date.isSame(eventDate);
        });

        if (index >= 0) {
            this.currentPage = Math.floor(index / this.pageSize) + 1;
            return true;
        } else {
            return false;
        }
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
        <b-alert
            v-if="laboratoryOrdersAreQueued(hdid)"
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
                    <FilterComponent
                        class="my-1"
                        :hdid="hdid"
                        :entry-types="entryTypes"
                    />
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
        <div v-show="showTimelineEntries">
            <b-row
                v-if="showDisplayCount"
                id="listControls"
                class="no-print"
                data-testid="displayCountText"
            >
                <b-col class="py-2" data-testid="timeline-record-count">
                    Displaying {{ visibleTimelineEntries.length }} out of
                    {{ filteredTimelineEntries.length }} records
                </b-col>
            </b-row>
            <div
                v-show="isOnlyImmunizationSelected"
                id="linear-timeline-immunization-disclaimer"
                class="pb-2"
            >
                <b-alert
                    show
                    variant="info"
                    class="mt-0 mb-1"
                    data-testid="linear-timeline-immunization-disclaimer-alert"
                >
                    <span>
                        If any of your immunizations are missing or incorrect,
                        <a
                            href="https://www.immunizationrecord.gov.bc.ca/"
                            target="_blank"
                            rel="noopener"
                            >fill in this online form</a
                        >.
                    </span>
                </b-alert>
            </div>
            <div id="timeData" data-testid="linearTimelineData">
                <div
                    v-for="dateGroup in dateGroups"
                    :key="dateGroup.key"
                    :ref="dateGroup.key"
                >
                    <component
                        :is="getComponentForEntry(entry.type)"
                        v-for="(entry, index) in dateGroup.entries"
                        :key="entry.type + '-' + entry.id"
                        :datekey="dateGroup.key"
                        :entry="entry"
                        :index="index"
                        :hdid="hdid"
                        data-testid="timelineCard"
                    />
                </div>
            </div>
            <b-row align-h="center">
                <b-col cols="auto">
                    <b-pagination-nav
                        v-if="filteredTimelineEntries.length > 0"
                        v-model="currentPage"
                        :link-gen="linkGen"
                        :number-of-pages="numberOfPages"
                        data-testid="pagination"
                        limit="4"
                        first-number
                        last-number
                        next-text="Next"
                        prev-text="Prev"
                        use-router
                        class="mt-3"
                    />
                </b-col>
            </b-row>
            <div v-if="showEmptyState" class="text-center pt-2">
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
                            class="text-center pt-2 noTimelineEntriesText"
                            data-testid="noTimelineEntriesText"
                        >
                            <span v-if="hasActiveFilter"
                                >No records found with the selected
                                filters</span
                            >
                            <span v-else>No records found</span>
                        </p>
                    </b-col>
                </b-row>
            </div>
        </div>
        <content-placeholders
            v-if="showContentPlaceholders"
            data-testid="content-placeholders"
        >
            <content-placeholders-heading :img="true" />
            <content-placeholders-text :lines="3" />
        </content-placeholders>
        <ProtectiveWordComponent :hdid="hdid" />
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

.form-group {
    margin-bottom: 0px;
}

.sticky-top {
    transition: all 0.3s;
    z-index: 49 !important;
}

.sticky-offset {
    background-color: white;
    z-index: 2;

    &.header-offset {
        top: $header-height;
    }
}

.noTimelineEntriesText {
    font-size: 1.5rem;
    color: #6c757d;
}

.filter-label {
    font-size: 1rem;
}
</style>
<style lang="scss">
ul.pagination {
    margin-bottom: 8px;
}
</style>
