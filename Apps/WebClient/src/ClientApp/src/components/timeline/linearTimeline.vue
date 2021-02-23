<script lang="ts">
import Vue from "vue";
import { Component, Prop, Watch } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

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
    @Action("setLinearDate", { namespace: "timeline" })
    setLinearDate!: (linearDate: DateWrapper) => void;

    @Getter("calendarDate", { namespace: "timeline" })
    calendarDate!: DateWrapper;

    @Getter("selectedDate", { namespace: "timeline" })
    selectedDate!: DateWrapper | null;

    @Getter("isHeaderShown", { namespace: "navbar" }) isHeaderShown!: boolean;

    @Getter("filter", { namespace: "timeline" }) filter!: TimelineFilter;

    @Getter("isLinearView", { namespace: "timeline" })
    isLinearView!: TimelineFilter;

    @Getter("hasActiveFilter", { namespace: "timeline" })
    hasActiveFilter!: boolean;

    @Prop() private timelineEntries!: TimelineEntry[];

    @Prop({ default: 0 }) private totalEntries!: number;

    private visibleTimelineEntries: TimelineEntry[] = [];
    private currentPage = 1;
    private dateGroups: DateGroup[] = [];

    private readonly pageSize = 25;

    @Watch("visibleTimelineEntries")
    private onVisibleEntriesUpdate() {
        this.dateGroups = DateGroup.createGroups(this.visibleTimelineEntries);
        this.dateGroups = DateGroup.sortGroups(this.dateGroups);
    }

    @Watch("calendarDate")
    private onCalendarDate() {
        if (!this.isLinearView) {
            this.setPageFromDate(this.calendarDate);
        }
    }

    @Watch("selectedDate")
    private onSelectedDate() {
        if (this.selectedDate !== null) {
            const selectedDate = this.selectedDate as DateWrapper;

            this.setPageFromDate(selectedDate);
            // Wait for next render cycle until the pages have been calculated and displayed
            this.$nextTick().then(() => {
                const selectedDate = this.selectedDate as DateWrapper;
                this.focusOnDate(selectedDate);
            });
        }
    }

    private linkGen(pageNum: number) {
        return `?page=${pageNum}`;
    }

    private get numberOfPages(): number {
        let result = 1;
        if (this.timelineEntries.length > this.pageSize) {
            result = Math.ceil(this.timelineEntries.length / this.pageSize);
        }
        return result;
    }

    private get timelineIsEmpty(): boolean {
        return this.timelineEntries.length == 0;
    }

    @Watch("currentPage")
    @Watch("timelineEntries")
    private calculateVisibleEntries() {
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
        this.visibleTimelineEntries = this.timelineEntries.slice(
            lowerIndex,
            upperIndex
        );

        if (this.isLinearView && this.visibleTimelineEntries.length > 0) {
            // Update the store
            this.setLinearDate(this.visibleTimelineEntries[0].date);
        }
    }

    private getVisibleCount(): number {
        return this.visibleTimelineEntries.length;
    }

    private setPageFromDate(eventDate: DateWrapper) {
        let index = this.timelineEntries.findIndex((entry) =>
            entry.date.isSame(eventDate)
        );
        this.currentPage = Math.floor(index / this.pageSize) + 1;
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
