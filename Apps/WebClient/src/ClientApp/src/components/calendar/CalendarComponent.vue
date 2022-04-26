<script lang="ts">
import Vue from "vue";
import { Component, Prop, Watch } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import { DateWrapper } from "@/models/dateWrapper";
import { DateGroup } from "@/models/timelineEntry";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger } from "@/services/interfaces";

import CalendarBody from "./CalendarBodyComponent.vue";
import CalendarHeader from "./CalendarHeaderComponent.vue";

@Component({
    components: {
        CalendarHeader,
        CalendarBody,
    },
})
export default class CalendarComponent extends Vue {
    @Action("setCalendarDate", { namespace: "timeline" }) setCalendarDate!: (
        date: DateWrapper
    ) => void;

    @Getter("isHeaderShown", { namespace: "navbar" }) isHeaderShown!: boolean;

    @Getter("isLinearView", { namespace: "timeline" }) isLinearView!: boolean;

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

    private currentMonth: DateWrapper = new DateWrapper();
    private logger!: ILogger;

    private get availableMonths() {
        return this.dateGroups.reduce<DateWrapper[]>((groups, entry) => {
            const fullYear = entry.date.year();
            if (!isNaN(fullYear)) {
                // Get the month and year and dismiss the day
                const monthYear = entry.date.startOf("month");

                // Create a new group if it the date doesnt exist in the map
                if (
                    groups.findIndex(
                        (month) =>
                            month.year() === monthYear.year() &&
                            month.month() === monthYear.month()
                    ) === -1
                ) {
                    groups.push(monthYear);
                }
            } else {
                this.logger.error(
                    `Invalid entry date: ${JSON.stringify(entry)}`
                );
            }
            return groups;
        }, []);
    }

    @Watch("currentMonth")
    private onCurrentMonth() {
        if (!this.isLinearView) {
            let dateGroup: DateGroup = this.dateGroups.find((d) =>
                this.currentMonth.isSame(d.date, "month")
            ) as DateGroup;

            this.setCalendarDate(dateGroup.entries[0].date);
        }
    }

    private created() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
    }
}
</script>

<template>
    <div class="calendar">
        <!-- header pick month -->
        <b-row>
            <b-col>
                <CalendarHeader
                    class="sticky-top sticky-offset p-2"
                    :class="{ 'header-offset': isHeaderShown }"
                    :current-month.sync="currentMonth"
                    :title-format="titleFormat"
                    :available-months="availableMonths"
                />
            </b-col>
            <slot name="add-note" />
        </b-row>
        <!-- body display date day and events -->
        <CalendarBody
            class="pt-2 px-0 px-md-2"
            :current-month="currentMonth"
            :date-groups="dateGroups"
            :month-names="monthNames"
            :week-names="weekNames"
            :first-day="firstDay"
        >
        </CalendarBody>
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
    top: $timeline-filter-height + $timeline-dates-height;
    background-color: white;
    border-bottom: solid $primary 2px;
    margin-top: -2px;
    z-index: 1;
    @media (max-width: 575px) {
        top: $timeline-filter-height + $timeline-dates-height;
    }
    &.header-offset {
        top: $header-height + $timeline-filter-height + $timeline-dates-height;
    }
}
</style>
