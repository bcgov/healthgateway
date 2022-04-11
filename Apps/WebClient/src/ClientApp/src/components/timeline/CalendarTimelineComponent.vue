<script lang="ts">
import Vue from "vue";
import { Component, Prop } from "vue-property-decorator";
import { Getter } from "vuex-class";

import CalendarComponent from "@/components/calendar/CalendarComponent.vue";
import TimelineEntry, { DateGroup } from "@/models/timelineEntry";

@Component({
    components: {
        CalendarComponent,
    },
})
export default class CalendarTimelineComponent extends Vue {
    @Prop() private timelineEntries!: TimelineEntry[];

    @Getter("hasActiveFilter", { namespace: "timeline" })
    hasActiveFilter!: boolean;

    private get timelineIsEmpty(): boolean {
        return this.timelineEntries.length === 0;
    }

    private get dateGroups(): DateGroup[] {
        if (this.timelineIsEmpty) {
            return [];
        }

        let newGroupArray = DateGroup.createGroups(this.timelineEntries);
        return DateGroup.sortGroups(newGroupArray, false);
    }
}
</script>

<template>
    <div class="timeline-calendar">
        <CalendarComponent v-show="!timelineIsEmpty" :date-groups="dateGroups">
            <template #add-note>
                <slot name="add-note" />
            </template>
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
.noTimelineEntriesText {
    font-size: 1.5rem;
    color: #6c757d;
}
</style>
