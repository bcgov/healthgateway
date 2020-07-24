<style lang="scss"></style>
<template>
    <div class="timeline-calendar">
        <CalendarComponent
            v-if="filteredTimelineEntries.length > 0"
            :date-groups="dateGroups"
            :filter-text="filterText"
            :filter-types="filterTypes"
            :is-list-view="isListView"
        />
        <div v-else class="text-center">
            No records
        </div>
    </div>
</template>

<script lang="ts">
import Vue from "vue";
import { Component, Prop, Watch } from "vue-property-decorator";
import moment from "moment";
import TimelineEntry, { DateGroup } from "@/models/timelineEntry";
import CalendarComponent from "@/components/calendar/calendar.vue";

@Component({
    components: {
        CalendarComponent,
    },
})
export default class CalendarTimelineComponent extends Vue {
    @Prop() private timelineEntries!: TimelineEntry[];
    @Prop() private isListView!: boolean;
    @Prop() private totalEntries!: number;
    @Prop() private filterText!: string;
    @Prop() private filterTypes!: string[];

    private filteredTimelineEntries: TimelineEntry[] = [];

    @Watch("filterText")
    @Watch("filterTypes")
    private applyTimelineFilter() {
        this.filteredTimelineEntries = this.timelineEntries.filter((entry) =>
            entry.filterApplies(this.filterText, this.filterTypes)
        );
    }

    @Watch("timelineEntries")
    private refreshEntries() {
        this.applyTimelineFilter();
    }

    private get dateGroups(): DateGroup[] {
        if (this.filteredTimelineEntries.length === 0) {
            return [];
        }
        let groups = this.filteredTimelineEntries.reduce<
            Record<string, TimelineEntry[]>
        >((groups, entry) => {
            // Get the string version of the date and get the date
            const date = new Date(entry.date).setHours(0, 0, 0, 0);

            // Create a new group if it the date doesnt exist in the map
            if (!groups[date]) {
                groups[date] = [];
            }
            groups[date].push(entry);
            return groups;
        }, {});
        let groupArrays = Object.keys(groups).map<DateGroup>((dateKey) => {
            return {
                key: dateKey,
                date: new Date(groups[dateKey][0].date),
                entries: groups[
                    dateKey
                ].sort((a: TimelineEntry, b: TimelineEntry) =>
                    a.type > b.type ? 1 : a.type < b.type ? -1 : 0
                ),
            };
        });
        return this.sortGroup(groupArrays);
    }

    private sortGroup(groupArrays: DateGroup[]) {
        groupArrays.sort((a, b) =>
            a.date > b.date ? -1 : a.date < b.date ? 1 : 0
        );
        return groupArrays;
    }
}
</script>
