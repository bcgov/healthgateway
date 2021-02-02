<script lang="ts">
import Vue from "vue";
import { Component, Prop, Watch } from "vue-property-decorator";

import EventBus, { EventMessageName } from "@/eventbus";
import { DateWrapper } from "@/models/dateWrapper";
import TimelineEntry, { DateGroup } from "@/models/timelineEntry";
import TimelineFilter from "@/models/timelineFilter";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { ILogger } from "@/services/interfaces";

import CalendarBody from "./body.vue";
import CalendarHeader from "./header.vue";

@Component({
    components: {
        CalendarHeader,
        CalendarBody,
    },
})
export default class CalendarComponent extends Vue {
    @Prop() dateGroups!: DateGroup[];
    @Prop() private filter!: TimelineFilter;
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

    private availableMonths: DateWrapper[] = [];
    private currentMonth: DateWrapper = new DateWrapper();
    private eventBus = EventBus;
    private logger!: ILogger;

    private mounted() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        this.updateAvailableMonths();
        this.eventBus.$on(
            EventMessageName.TimelinePageUpdate,
            (eventDate: DateWrapper) => {
                this.currentMonth = eventDate.startOf("month");
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
        this.eventBus.$off(EventMessageName.TimelinePageUpdate);
        this.eventBus.$off(EventMessageName.TimelineEntryAdded);
    }

    private onEntryAdded(entry: TimelineEntry) {
        this.$nextTick().then(() => {
            this.currentMonth = entry.date.startOf("month");
        });
    }

    @Watch("dateGroups")
    private updateAvailableMonths() {
        this.availableMonths = this.dateGroups.reduce<DateWrapper[]>(
            (groups, entry) => {
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
            },
            []
        );
    }
}
</script>

<template>
    <div class="calendar">
        <!-- header pick month -->
        <CalendarHeader
            class="sticky-top sticky-offset p-2"
            :current-month.sync="currentMonth"
            :title-format="titleFormat"
            :available-months="availableMonths"
        >
        </CalendarHeader>
        <b-row class="sticky-top sticky-line" />
        <!-- body display date day and events -->
        <CalendarBody
            v-show="isVisible"
            class="pt-2 px-0 px-md-2"
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
</style>
