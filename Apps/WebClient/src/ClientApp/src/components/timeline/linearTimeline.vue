<script lang="ts">
import Vue from "vue";
import { Component, Prop, Watch } from "vue-property-decorator";
import { Getter } from "vuex-class";

import EventBus, { EventMessageName } from "@/eventbus";
import { DateWrapper } from "@/models/dateWrapper";
import TimelineEntry, { DateGroup, EntryType } from "@/models/timelineEntry";
import TimelineFilter from "@/models/timelineFilter";

import EncounterTimelineComponent from "./entryCard/encounter.vue";
import ImmunizationTimelineComponent from "./entryCard/immunization.vue";
import LaboratoryTimelineComponent from "./entryCard/laboratory.vue";
import MedicationTimelineComponent from "./entryCard/medication.vue";
import NoteTimelineComponent from "./entryCard/note.vue";

@Component({
    components: {
        MedicationComponent: MedicationTimelineComponent,
        ImmunizationComponent: ImmunizationTimelineComponent,
        LaboratoryComponent: LaboratoryTimelineComponent,
        EncounterComponent: EncounterTimelineComponent,
        NoteComponent: NoteTimelineComponent,
    },
})
export default class LinearTimelineComponent extends Vue {
    @Getter("isHeaderShown", { namespace: "navbar" }) isHeaderShown!: boolean;

    @Prop() private timelineEntries!: TimelineEntry[];
    @Prop({ default: 0 }) private totalEntries!: number;
    @Prop() private isVisible!: boolean;
    @Prop() private filter!: TimelineFilter;

    private filteredTimelineEntries: TimelineEntry[] = [];
    private visibleTimelineEntries: TimelineEntry[] = [];
    private currentPage = 1;
    private eventBus = EventBus;
    private dateGroups: DateGroup[] = [];
    private hasFilter = false;

    @Watch("filter", { deep: true })
    private applyTimelineFilter() {
        this.hasFilter = this.filter.hasActiveFilter();
        this.filteredTimelineEntries = this.timelineEntries.filter((entry) =>
            entry.filterApplies(this.filter)
        );
    }

    @Watch("timelineEntries")
    private refreshEntries() {
        this.applyTimelineFilter();
    }

    @Watch("visibleTimelineEntries")
    private onVisibleEntriesUpdate() {
        if (this.visibleTimelineEntries.length > 0) {
            if (this.isVisible) {
                this.eventBus.$emit(
                    EventMessageName.TimelinePageUpdate,
                    this.visibleTimelineEntries[0].date
                );
            }
        }

        this.dateGroups = DateGroup.createGroups(this.visibleTimelineEntries);
        this.dateGroups = DateGroup.sortGroup(this.dateGroups);
    }

    private created() {
        this.eventBus.$on(
            EventMessageName.CalendarDateEventClick,
            (eventDate: DateWrapper) => {
                this.setPageFromDate(eventDate);
                // Wait for next render cycle until the pages have been calculated and displayed
                this.$nextTick().then(() => {
                    this.focusOnDate(eventDate);
                });
            }
        );

        this.eventBus.$on(
            EventMessageName.CalendarMonthUpdated,
            this.setPageFromDate
        );

        this.eventBus.$on(EventMessageName.AddedNote, this.onEntryAdded);
    }

    private linkGen(pageNum: number) {
        return `?page=${pageNum}`;
    }

    private get numberOfPages(): number {
        let result = 1;
        if (this.filteredTimelineEntries.length > this.filter.pageSize) {
            result = Math.ceil(
                this.filteredTimelineEntries.length / this.filter.pageSize
            );
        }
        return result;
    }

    @Watch("currentPage")
    @Watch("filter.pageSize")
    @Watch("filteredTimelineEntries")
    private calculateVisibleEntries() {
        // Handle the current page being beyond the max number of pages
        if (this.currentPage > this.numberOfPages) {
            this.currentPage = this.numberOfPages;
        }
        // Get the section of the array that contains the paginated section
        let lowerIndex = (this.currentPage - 1) * this.filter.pageSize;
        let upperIndex = Math.min(
            this.currentPage * this.filter.pageSize,
            this.filteredTimelineEntries.length
        );
        this.visibleTimelineEntries = this.filteredTimelineEntries.slice(
            lowerIndex,
            upperIndex
        );
    }

    @Watch("filter.pageSize")
    private onEntriesPerPageChange() {
        this.currentPage = 1;
    }

    private getVisibleCount(): number {
        return this.visibleTimelineEntries.length;
    }

    private setPageFromDate(eventDate: DateWrapper) {
        let index = this.filteredTimelineEntries.findIndex((entry) =>
            entry.date.isSame(eventDate)
        );
        this.currentPage = Math.floor(index / this.filter.pageSize) + 1;
    }

    private get timelineIsEmpty(): boolean {
        return this.filteredTimelineEntries.length == 0;
    }

    private onEntryAdded(entry: TimelineEntry) {
        this.$nextTick().then(() => {
            this.setPageFromDate(entry.date);
            this.$nextTick().then(() => {
                this.focusOnDate(entry.date);
            });
        });
    }

    private focusOnDate(date: DateWrapper) {
        const dateEpoch = date.fromEpoch();
        let container: HTMLElement[] = this.$refs[dateEpoch] as HTMLElement[];
        container[0].querySelector("button")?.focus();
    }

    private getComponentForEntry(entryType: EntryType): string {
        switch (entryType) {
            case EntryType.Medication:
                return "MedicationComponent";

            case EntryType.Immunization:
                return "ImmunizationComponent";

            case EntryType.Laboratory:
                return "LaboratoryComponent";

            case EntryType.Encounter:
                return "EncounterComponent";

            case EntryType.Note:
                return "NoteComponent";
            default:
                return "";
        }
    }
}
</script>

<template>
    <div>
        <b-row
            class="no-print sticky-top sticky-offset pt-2 pl-2"
            :class="{ 'header-offset': isHeaderShown }"
        >
            <b-col>
                <b-pagination-nav
                    v-show="!timelineIsEmpty"
                    v-model="currentPage"
                    :link-gen="linkGen"
                    :number-of-pages="numberOfPages"
                    data-testid="pagination"
                    first-number
                    last-number
                    next-text="Next"
                    prev-text="Prev"
                    use-router
                    class="pb-0"
                ></b-pagination-nav>
            </b-col>
        </b-row>
        <b-row
            v-if="!timelineIsEmpty"
            class="sticky-top sticky-line"
            :class="{ 'header-offset': isHeaderShown }"
        />
        <b-row
            id="listControls"
            class="no-print"
            data-testid="displayCountText"
        >
            <b-col class="p-2">
                Displaying {{ getVisibleCount() }} out of
                {{ totalEntries }} records
            </b-col>
        </b-row>
        <div id="timeData" data-testid="linearTimelineData">
            <div
                v-for="dateGroup in dateGroups"
                :key="dateGroup.key"
                :ref="dateGroup.key"
            >
                <div
                    v-for="(entry, index) in dateGroup.entries"
                    :key="entry.type + '-' + entry.id"
                >
                    <component
                        :is="getComponentForEntry(entry.type)"
                        :datekey="dateGroup.key"
                        :entry="entry"
                        :index="index"
                        data-testid="timelineCard"
                    />
                </div>
            </div>
        </div>
        <div v-if="timelineIsEmpty" class="text-center pt-2">
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
            <b-row class="px-2">
                <b-col>
                    <p
                        class="text-center pt-2 noTimelineEntriesText"
                        data-testid="noTimelineEntriesText"
                    >
                        <span v-if="hasFilter"
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

.sticky-offset {
    top: $timeline-filter-height;
    background-color: white;
    z-index: 2;
    &.header-offset {
        top: $header-height + $timeline-filter-height;
    }
}

.sticky-line {
    top: $timeline-filter-height + $timeline-pagination-height;
    background-color: white;
    border-bottom: solid $primary 2px;
    margin-top: -2px;
    z-index: 1;
    @media (max-width: 575px) {
        top: 107px;
    }
    &.header-offset {
        top: $header-height + $timeline-filter-height +
            $timeline-pagination-height;
    }
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
