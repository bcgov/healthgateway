<template>
    <div class="calendar mx-3">
        <!-- header pick month -->
        <CalendarHeader
            :current-date.sync="currentDate"
            :title-format="titleFormat"
        >
        </CalendarHeader>
        <!-- body display date day and events -->
        <CalendarBody
            :current-date="currentDate"
            :date-groups="dateGroups"
            :month-names="monthNames"
            :week-names="weekNames"
            :first-day="firstDay"
        >
        </CalendarBody>
    </div>
</template>
<script lang="ts">
import Vue from "vue";
import { Component, Prop } from "vue-property-decorator";
import CalendarHeader from "./header.vue";
import CalendarBody from "./body.vue";
import TimelineEntry, { DateGroup } from "@/models/timelineEntry";

@Component({
    components: {
        CalendarHeader,
        CalendarBody,
    },
})
export default class CalendarComponent extends Vue {
    @Prop() dateGroups!: DateGroup[];
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

    private currentDate: Date = new Date();
}
</script>
