<template>
    <div class="calendar mx-3">
        <!-- header pick month -->
        <CalendarHeader
            :current-date.sync="currentDate"
            :title-format="titleFormat"
            :available-months="availableMonths"
        >
        </CalendarHeader>
        <!-- body display date day and events -->
        <CalendarBody
            :current-date="currentDate"
            :date-groups="dateGroups"
            :month-names="monthNames"
            :week-names="weekNames"
            :first-day="firstDay"
            :is-visible="isVisible"
        >
        </CalendarBody>
    </div>
</template>
<script lang="ts">
import Vue from "vue";
import { Component, Prop, Watch } from "vue-property-decorator";
import CalendarHeader from "./header.vue";
import CalendarBody from "./body.vue";
import TimelineEntry, { DateGroup } from "@/models/timelineEntry";
import {
    CalendarEntry,
    CalendarWeek,
    CalendarMonth,
} from "@/components/calendar/models";
import DateUtil from "@/utility/dateUtil";
import moment from "moment";
import EventBus from "@/eventbus";

@Component({
    components: {
        CalendarHeader,
        CalendarBody,
    },
})
export default class CalendarComponent extends Vue {
    @Prop() dateGroups!: DateGroup[];
    @Prop() private filterText!: string;
    @Prop() private filterTypes!: string[];
    @Prop() private isVisible!: boolean;

    @Prop({ default: 0, required: false }) firstDay!: number;
    @Prop({ default: "MMMM yyyy", required: false }) titleFormat!: string;
    @Prop({
        type: Array,
        default: () => {
            return [
                "January",
                "February",
                "March",
                "April",
                "May",
                "June",
                "July",
                "August",
                "September",
                "October",
                "November",
                "December",
            ];
        },
        required: false,
    })
    monthNames!: Array<string>;

    @Prop({
        type: Array,
        default: () => {
            return ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"];
        },
        required: false,
    })
    weekNames!: Array<string>;

    private availableMonths: Date[] = [];
    private currentDate: Date = new Date();
    private eventBus = EventBus;

    private mounted() {
        this.updateAvailableMonths();
        var self = this;
        this.eventBus.$on("timelinePageUpdate", function (eventDate: Date) {
            console.log(
                "calendar got the timelinePageUpdate sent by timeline vue for the " +
                    eventDate
            );
            self.currentDate = DateUtil.getMonthFirstDate(eventDate);
        });
    }

    @Watch("dateGroups")
    private updateAvailableMonths() {
        this.availableMonths = this.dateGroups.reduce<Date[]>(
            (groups, entry) => {
                // Get the month and year and dismiss the day
                const monthYear = new Date(
                    entry.date.getFullYear(),
                    entry.date.getMonth(),
                    1
                );

                // Create a new group if it the date doesnt exist in the map
                if (
                    groups.findIndex(
                        (month) =>
                            month.getFullYear() === monthYear.getFullYear() &&
                            month.getMonth() === monthYear.getMonth()
                    ) === -1
                ) {
                    groups.push(monthYear);
                }
                return groups;
            },
            []
        );
    }
}
</script>
