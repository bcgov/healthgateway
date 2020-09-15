<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";
.sticky-offset {
    top: 70px;
    background-color: white;
    z-index: 2;
}
.sticky-line {
    top: 122px;
    background-color: white;
    border-bottom: solid $primary 2px;
    margin-top: -2px;
    z-index: 1;
    @media (max-width: 575px) {
        top: 160px;
    }
}
</style>
<template>
    <div class="calendar mx-3">
        <!-- header pick month -->
        <CalendarHeader
            class="sticky-top sticky-offset"
            :current-month.sync="currentMonth"
            :title-format="titleFormat"
            :available-months="availableMonths"
        >
            <div slot="month-list-toggle">
                <slot name="month-list-toggle"></slot>
            </div>
        </CalendarHeader>
        <b-row class="sticky-top sticky-line" />
        <!-- body display date day and events -->
        <CalendarBody
            v-show="isVisible"
            :current-month="currentMonth"
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
import container from "@/plugins/inversify.config";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger } from "@/services/interfaces";
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
    private logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
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
    private currentMonth: Date = new Date();
    private eventBus = EventBus;

    private mounted() {
        this.updateAvailableMonths();
        var self = this;
        this.eventBus.$on("timelinePageUpdate", function (eventDate: Date) {
            self.currentMonth = DateUtil.getMonthFirstDate(eventDate);
        });
    }

    @Watch("dateGroups")
    private updateAvailableMonths() {
        this.availableMonths = this.dateGroups.reduce<Date[]>(
            (groups, entry) => {
                const fullYear = entry.date.getFullYear();
                if (!isNaN(fullYear)) {
                    // Get the month and year and dismiss the day
                    const monthYear = new Date(
                        fullYear,
                        entry.date.getMonth(),
                        1
                    );

                    // Create a new group if it the date doesnt exist in the map
                    if (
                        groups.findIndex(
                            (month) =>
                                month.getFullYear() ===
                                    monthYear.getFullYear() &&
                                month.getMonth() === monthYear.getMonth()
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
            },
            []
        );
    }
}
</script>
