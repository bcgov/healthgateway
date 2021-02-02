<script lang="ts">
import Vue from "vue";
import { Component, Prop, Watch } from "vue-property-decorator";

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
            (firstEntryDate: DateWrapper) => {
                this.setPageFromDate(firstEntryDate);
            }
        );

        this.eventBus.$on(
            EventMessageName.TimelineEntryAdded,
            (entry: TimelineEntry) => {
                this.onEntryAdded(entry);
            }
        );
    }

    private beforeDestroy() {
        this.eventBus.$off(EventMessageName.CalendarDateEventClick);
        this.eventBus.$off(EventMessageName.CalendarMonthUpdated);
        this.eventBus.$off(EventMessageName.TimelineEntryAdded);
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

    private getHeadingDate(date: DateWrapper): string {
        return date.format("LLL d, yyyy");
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
        let index = this.filteredTimelineEntries.findIndex(
            (entry) => entry.date === eventDate
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
        container[0].focus();
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
        <b-row class="no-print sticky-top sticky-offset pt-2 pl-2">
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
        <b-row v-if="!timelineIsEmpty" class="sticky-top sticky-line" />
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
            <b-row
                v-for="dateGroup in dateGroups"
                :key="dateGroup.key"
                class="p-0 m-0"
            >
                <b-col cols="auto" class="p-0 m-0">
                    <div
                        :id="dateGroup.key"
                        :ref="dateGroup.key"
                        data-testid="dateGroup"
                        class="dateHeading pl-2"
                        tabindex="1"
                    >
                        {{ getHeadingDate(dateGroup.date) }}
                    </div>
                </b-col>
                <b-col class="pl-2">
                    <hr class="dateBreakLine" />
                </b-col>
                <component
                    :is="getComponentForEntry(entry.type)"
                    v-for="(entry, index) in dateGroup.entries"
                    :key="entry.type + '-' + entry.id"
                    :datekey="dateGroup.key"
                    :entry="entry"
                    :index="index"
                    data-testid="timelineCard"
                />
            </b-row>
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

.dateBreakLine {
    border-top: dashed 2px $primary;
    @media (max-width: 575px) {
        border-top: dashed 1px $primary;
    }
}

.dateHeading {
    padding-top: 0px;
    color: $primary;
    font-size: 1.3em;
    @media (max-width: 575px) {
        font-size: 1.1em;
    }
}

.sticky-offset {
    top: 54px;
    background-color: white;
    z-index: 2;
}

.sticky-line {
    top: 107px;
    background-color: white;
    border-bottom: solid $primary 2px;
    margin-top: -2px;
    z-index: 1;
    @media (max-width: 575px) {
        top: 107px;
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
