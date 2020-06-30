<style lang="scss">
</style>
<template>
    <div class="timeline-calendar">
        <CalendarComponent :date-groups="dateGroups" locale="en" />
    </div>
</template>

<script lang="ts">
import Vue from "vue";
import { Component, Prop } from "vue-property-decorator";
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
    @Prop() private totalEntries!: number;

    private linkGen(pageNum: number) {
        return `?page=${pageNum}`;
    }

    private getHeadingDate(date: Date): string {
        return moment(date).format("ll");
    }

    private get dateGroups(): DateGroup[] {
        if (this.timelineEntries.length === 0) {
            return [];
        }
        let groups = this.timelineEntries.reduce<
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
                date: groups[dateKey][0].date,
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
