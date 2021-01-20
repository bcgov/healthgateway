<script lang="ts">
import Vue from "vue";
import { Component, Prop, Watch } from "vue-property-decorator";

import CalendarComponent from "@/components/calendar/calendar.vue";
import TimelineEntry, { DateGroup } from "@/models/timelineEntry";
import TimelineFilter from "@/models/timelineFilter";
@Component({
    components: {
        CalendarComponent,
    },
})
export default class CalendarTimelineComponent extends Vue {
    @Prop() private timelineEntries!: TimelineEntry[];
    @Prop() private isVisible!: boolean;
    @Prop() private totalEntries!: number;
    @Prop() private filter!: TimelineFilter;

    private filteredTimelineEntries: TimelineEntry[] = [];
    private dateGroups: DateGroup[] = [];
    private hasFilter = false;

    @Watch("filter", { deep: true })
    private applyTimelineFilter() {
        this.hasFilter = this.filter.hasActiveFilter();
        this.filteredTimelineEntries = this.timelineEntries.filter((entry) =>
            entry.filterApplies(this.filter)
        );

        this.dateGroups = DateGroup.createGroups(this.filteredTimelineEntries);
        this.dateGroups = DateGroup.sortGroup(this.dateGroups, false);
    }

    @Watch("timelineEntries")
    private refreshEntries() {
        this.applyTimelineFilter();
    }

    private get timelineIsEmpty(): boolean {
        return this.filteredTimelineEntries.length == 0;
    }
}
</script>

<template>
    <div class="timeline-calendar">
        <CalendarComponent
            :date-groups="dateGroups"
            :filter="filter"
            :is-visible="isVisible && !timelineIsEmpty"
        >
            <div slot="month-list-toggle">
                <slot name="month-list-toggle"></slot>
            </div>
        </CalendarComponent>
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
            <b-row>
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
.noTimelineEntriesText {
    font-size: 1.5rem;
    color: #6c757d;
}
</style>
