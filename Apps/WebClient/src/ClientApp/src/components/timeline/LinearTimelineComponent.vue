<script lang="ts">
import Vue from "vue";
import { Component, Prop, Watch } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import { EntryType, entryTypeMap } from "@/constants/entryType";
import { DateWrapper } from "@/models/dateWrapper";
import TimelineEntry, { DateGroup } from "@/models/timelineEntry";
import TimelineFilter from "@/models/timelineFilter";
import User from "@/models/user";
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

// eslint-disable-next-line @typescript-eslint/no-explicit-any
const options: any = {
    components: {
        MedicationRequestComponent: MedicationRequestTimelineComponent,
        MedicationComponent: MedicationTimelineComponent,
        ImmunizationComponent: ImmunizationTimelineComponent,
        Covid19LaboratoryOrderComponent:
            Covid19LaboratoryOrderTimelineComponent,
        LaboratoryOrderComponent: LaboratoryOrderTimelineComponent,
        EncounterComponent: EncounterTimelineComponent,
        NoteComponent: NoteTimelineComponent,
        ClinicalDocumentComponent: ClinicalDocumentTimelineComponent,
        HospitalVisitComponent: HospitalVisitTimelineComponent,
    },
};

@Component(options)
export default class LinearTimelineComponent extends Vue {
    @Action("setLinearDate", { namespace: "timeline" })
    setLinearDate!: (linearDate: DateWrapper) => void;

    @Getter("linearDate", { namespace: "timeline" })
    linearDate!: DateWrapper;

    @Getter("selectedDate", { namespace: "timeline" })
    selectedDate!: DateWrapper | null;

    @Getter("isHeaderShown", { namespace: "navbar" })
    isHeaderShown!: boolean;

    @Getter("filter", { namespace: "timeline" })
    filter!: TimelineFilter;

    @Getter("entryTypes", { namespace: "timeline" })
    entryTypes!: Set<EntryType>;

    @Getter("hasActiveFilter", { namespace: "timeline" })
    hasActiveFilter!: boolean;

    @Getter("medicationsAreLoading", { namespace: "medication" })
    medicationsAreLoading!: (hdid: string) => boolean;

    @Getter("specialAuthorityRequestsAreLoading", { namespace: "medication" })
    specialAuthorityRequestsAreLoading!: (hdid: string) => boolean;

    @Getter("isLoading", { namespace: "comment" })
    isCommentLoading!: boolean;

    @Getter("covid19LaboratoryOrdersAreLoading", { namespace: "laboratory" })
    covid19LaboratoryOrdersAreLoading!: (hdid: string) => boolean;

    @Getter("laboratoryOrdersAreLoading", { namespace: "laboratory" })
    laboratoryOrdersAreLoading!: (hdid: string) => boolean;

    @Getter("healthVisitsAreLoading", { namespace: "encounter" })
    healthVisitsAreLoading!: (hdid: string) => boolean;

    @Getter("hospitalVisitsAreLoading", { namespace: "encounter" })
    hospitalVisitsAreLoading!: (hdid: string) => boolean;

    @Getter("isLoading", { namespace: "immunization" })
    isImmunizationLoading!: (hdid: string) => boolean;

    @Getter("isLoading", { namespace: "note" })
    isNoteLoading!: boolean;

    @Getter("isLoading", { namespace: "clinicalDocument" })
    isClinicalDocumentLoading!: (hdid: string) => boolean;

    @Getter("isDeferredLoad", { namespace: "immunization" })
    isImmunizationDeferred!: (hdid: string) => boolean;

    @Getter("user", { namespace: "user" })
    user!: User;

    @Prop()
    private timelineEntries!: TimelineEntry[];

    private currentPage = 1;

    private readonly pageSize = 25;

    private logger!: ILogger;

    private get isFullyLoaded(): boolean {
        const fullyLoaded =
            !this.isImmunizationLoading(this.user.hdid) &&
            !this.isImmunizationDeferred(this.user.hdid) &&
            !this.specialAuthorityRequestsAreLoading(this.user.hdid) &&
            !this.medicationsAreLoading(this.user.hdid) &&
            !this.covid19LaboratoryOrdersAreLoading(this.user.hdid) &&
            !this.laboratoryOrdersAreLoading(this.user.hdid) &&
            !this.healthVisitsAreLoading(this.user.hdid) &&
            !this.hospitalVisitsAreLoading(this.user.hdid) &&
            !this.isClinicalDocumentLoading(this.user.hdid) &&
            !this.isNoteLoading &&
            !this.isCommentLoading;
        this.logger.debug(`Linear Timeline is fully loaded: ${fullyLoaded}`);
        return fullyLoaded;
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
                EntryType.ClinicalDocument,
                this.isClinicalDocumentLoading(this.user.hdid)
            )
        );

        filtersLoaded.push(
            this.isSelectedFilterModuleLoading(
                EntryType.Note,
                this.isNoteLoading
            )
        );

        const filterLoading = filtersLoaded.includes(true);
        this.logger.debug(`Timeline filter loading: ${filterLoading}`);

        return filterLoading;
    }

    private get isOnlyImmunizationSelected(): boolean {
        return (
            this.entryTypes.size === 1 &&
            this.entryTypes.has(EntryType.Immunization)
        );
    }

    private get numberOfPages(): number {
        let pageCount = 1;
        if (this.timelineEntries.length > this.pageSize) {
            pageCount = Math.ceil(this.timelineEntries.length / this.pageSize);
        }
        return pageCount;
    }

    private get timelineIsEmpty(): boolean {
        this.logger.debug(
            `Linear Timeline Entries length: ${this.timelineEntries.length}`
        );
        return this.timelineEntries.length === 0;
    }

    private get visibleTimelineEntries(): TimelineEntry[] {
        if (this.timelineIsEmpty) {
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
            this.timelineEntries.length
        );
        return this.timelineEntries.slice(lowerIndex, upperIndex);
    }

    private get dateGroups(): DateGroup[] {
        if (this.timelineIsEmpty) {
            return [];
        }

        let newGroupArray = DateGroup.createGroups(this.visibleTimelineEntries);
        return DateGroup.sortGroups(newGroupArray);
    }

    @Watch("currentPage")
    private onCurrentPage(): void {
        if (this.visibleTimelineEntries.length > 0) {
            // Update the store
            this.setLinearDate(this.visibleTimelineEntries[0].date);
        }
    }

    @Watch("selectedDate")
    private onSelectedDate(): void {
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

    private linkGen(pageNum: number): string {
        return `?page=${pageNum}`;
    }

    private created(): void {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
    }

    private mounted(): void {
        this.setPageFromDate(this.linearDate);
    }

    private get showDisplayCount(): boolean {
        return this.visibleTimelineEntries.length > 0;
    }

    private get showEmptyState(): boolean {
        return this.timelineIsEmpty && !this.isFilterLoading;
    }

    private get timelineEntryCount(): number {
        return this.timelineEntries.length;
    }

    private get visibleTimelineEntryCount(): number {
        return this.visibleTimelineEntries.length;
    }

    private setPageFromDate(eventDate: DateWrapper): boolean {
        let index = this.timelineEntries.findIndex((entry) => {
            entry.date.isSame(eventDate);
        });

        if (index >= 0) {
            this.currentPage = Math.floor(index / this.pageSize) + 1;
            return true;
        } else {
            return false;
        }
    }

    private focusOnDate(date: DateWrapper): void {
        const dateEpoch = date.fromEpoch();
        const container = this.$refs[dateEpoch] as HTMLElement[];
        container[0].querySelector("button")?.focus();
    }

    private getComponentForEntry(entryType: EntryType): string {
        return entryTypeMap.get(entryType)?.component ?? "";
    }

    private isSelectedFilterModuleLoading(
        entryType: EntryType,
        loading: boolean
    ): boolean {
        const filterApplied = this.entryTypes.has(entryType);
        const isLoading = filterApplied && loading;
        this.logger.debug(
            `Timeline filter entry type: ${entryType} applied: ${filterApplied} - filter loading: ${loading} and filter isLoading: ${isLoading}`
        );
        return isLoading;
    }
}
</script>

<template>
    <div>
        <b-row
            v-if="showDisplayCount"
            id="listControls"
            class="no-print"
            data-testid="displayCountText"
        >
            <b-col class="py-2" data-testid="timeline-record-count">
                Displaying {{ visibleTimelineEntryCount }} out of
                {{ timelineEntryCount }} records
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
                    data-testid="timelineCard"
                />
            </div>
        </div>
        <b-row align-h="center">
            <b-col cols="auto">
                <b-pagination-nav
                    v-if="!timelineIsEmpty"
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
                            >No records found with the selected filters</span
                        >
                        <span v-else>No records found</span>
                    </p>
                </b-col>
            </b-row>
        </div>
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

.noTimelineEntriesText {
    font-size: 1.5rem;
    color: #6c757d;
}
</style>
<style lang="scss">
ul.pagination {
    margin-bottom: 8px;
}
</style>
