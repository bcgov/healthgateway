<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";
.calendar-header {
    padding-bottom: 15px;

    .btn-outline-primary {
        font-size: 2em;
        background-color: white;
    }
    .btn-outline-primary:focus {
        color: $primary;
        background-color: white;
    }
    .btn-outline-primary:hover {
        color: white;
        background-color: $primary;
    }
    .btn-outline-primary:active {
        color: white;
    }

    .arrow-icon {
        font-size: 1em;
    }
    .left-button {
        border-radius: 5px 0px 0px 5px;
        border-right: 0px;
    }
    .right-button {
        border-radius: 0px 5px 5px 0px;
    }
}
</style>
<template>
    <b-row class="calendar-header">
        <b-col cols="col-sm-auto" class="p-0">
            <b-btn
                v-show="hasAvailableMonths"
                variant="light"
                :disabled="monthIndex == 0"
                class="arrow-icon left-button px-2 m-0"
                @click.stop="previousMonth"
            >
                <font-awesome-icon :icon="leftIcon" />
            </b-btn>
        </b-col>
        <b-col cols="col-sm-auto" class="p-0">
            <MonthYearPickerComponent
                v-show="hasAvailableMonths"
                :current-month="currentMonth"
                :available-months="availableMonths"
                @date-changed="dateSelected"
            />
        </b-col>
        <b-col cols="col-sm-auto" class="p-0">
            <b-btn
                v-show="hasAvailableMonths"
                variant="light"
                :disabled="monthIndex == availableMonths.length - 1"
                class="arrow-icon right-button px-2 m-0"
                @click.stop="nextMonth"
            >
                <font-awesome-icon :icon="rightIcon" />
            </b-btn>
        </b-col>
        <b-col
            class="header-right px-0 col-12 order-first col-sm order-sm-last"
        >
            <slot name="month-list-toggle"></slot>
        </b-col>
    </b-row>
</template>
<script lang="ts">
import Vue from "vue";
import EventBus from "@/eventbus";
import { Component, Prop, Watch } from "vue-property-decorator";
import MonthYearPickerComponent from "@/components/monthYearPicker.vue";
import CalendarBody from "./body.vue";
import { DateGroup } from "@/models/timelineEntry";
import { DateWrapper } from "@/models/dateWrapper";

@Component({
    components: {
        CalendarBody,
        MonthYearPickerComponent,
    },
})
export default class CalendarComponent extends Vue {
    @Prop() currentMonth!: DateWrapper;
    @Prop() availableMonths!: DateWrapper[];

    private monthIndex: number = 0;
    private headerDate: DateWrapper = new DateWrapper();
    private eventBus = EventBus;
    private leftIcon: string = "chevron-left";
    private rightIcon: string = "chevron-right";

    private get hasAvailableMonths() {
        return this.availableMonths.length > 0;
    }

    @Watch("currentMonth")
    public onCurrentMonthChange(currentMonth: DateWrapper) {
        this.dateSelected(currentMonth);
    }

    @Watch("availableMonths")
    public onAvailableMonthsChange() {
        if (this.monthIndex !== this.availableMonths.length - 1) {
            this.monthIndex = this.availableMonths.length - 1;
        } else {
            this.onMonthIndexChange();
        }
    }

    @Watch("monthIndex")
    public onMonthIndexChange() {
        this.headerDate = this.availableMonths[this.monthIndex];
        this.dispatchEvent();
    }

    private created() {
        this.dispatchEvent();
    }

    private previousMonth() {
        if (this.monthIndex > 0) {
            this.monthIndex -= 1;
        }
    }

    private nextMonth() {
        if (this.monthIndex + 1 < this.availableMonths.length) {
            this.monthIndex += 1;
        }
    }

    private dateSelected(date: DateWrapper) {
        this.monthIndex = this.availableMonths.findIndex(
            (d) => d.year() === date.year() && d.month() === date.month()
        );
    }

    private dispatchEvent() {
        if (this.headerDate) {
            let firstMonthDate = this.headerDate.startOf("month");
            this.$emit("update:currentMonth", firstMonthDate);
        }
    }
}
</script>
