<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import {
    faChevronLeft,
    faChevronRight,
} from "@fortawesome/free-solid-svg-icons";
import Vue from "vue";
import { Component, Prop, Watch } from "vue-property-decorator";
import { Getter } from "vuex-class";

import MonthYearPickerComponent from "@/components/MonthYearPickerComponent.vue";
import { DateWrapper } from "@/models/dateWrapper";

import CalendarBody from "./CalendarBodyComponent.vue";

library.add(faChevronLeft, faChevronRight);

@Component({
    components: {
        CalendarBody,
        MonthYearPickerComponent,
    },
})
export default class CalendarHeaderComponent extends Vue {
    @Prop() readonly currentMonth!: DateWrapper;
    @Prop() readonly availableMonths!: DateWrapper[];

    @Getter("calendarDate", { namespace: "timeline" })
    calendarDate!: DateWrapper;

    @Getter("linearDate", { namespace: "timeline" }) linearDate!: DateWrapper;

    @Getter("selectedDate", { namespace: "timeline" })
    selectedDate!: DateWrapper;

    private monthIndex = 0;
    private headerDate: DateWrapper | null = null;

    private get hasAvailableMonths() {
        return this.availableMonths.length > 0;
    }

    @Watch("linearDate")
    private onLinearDate() {
        this.dateSelected(this.linearDate.startOf("month"));
    }

    @Watch("selectedDate")
    private onSelectedDate() {
        this.$nextTick().then(() => {
            this.dateSelected(this.selectedDate.startOf("month"));
        });
    }

    @Watch("availableMonths")
    public onAvailableMonthsChange(): void {
        if (this.monthIndex !== this.availableMonths.length - 1) {
            this.monthIndex = this.availableMonths.length - 1;
        } else {
            this.onMonthIndexChange();
        }
    }

    @Watch("monthIndex")
    public onMonthIndexChange(): void {
        this.headerDate = this.availableMonths[this.monthIndex];
        this.dispatchEvent();
    }

    private created() {
        this.dateSelected(this.calendarDate.startOf("month"));
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

<template>
    <b-row class="calendar-header">
        <b-col cols="col-sm-auto">
            <hg-button
                v-show="hasAvailableMonths"
                variant="secondary"
                :disabled="monthIndex == 0"
                class="arrow-icon left-button px-2 m-0"
                @click.stop="previousMonth"
            >
                <hg-icon icon="chevron-left" size="medium" />
            </hg-button>
        </b-col>
        <b-col cols="col-sm-auto">
            <MonthYearPickerComponent
                v-show="hasAvailableMonths"
                :current-month="currentMonth"
                :available-months="availableMonths"
                @date-changed="dateSelected"
            />
        </b-col>
        <b-col cols="col-sm-auto">
            <hg-button
                v-show="hasAvailableMonths"
                variant="secondary"
                :disabled="monthIndex == availableMonths.length - 1"
                class="arrow-icon right-button px-2 m-0"
                @click.stop="nextMonth"
            >
                <hg-icon icon="chevron-right" size="medium" />
            </hg-button>
        </b-col>
    </b-row>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

div[class^="col"],
div[class*=" col"] {
    padding: 0px;
    margin: 0px;
}

div[class^="row"],
div[class*=" row"] {
    padding: 0px;
    margin: 0px;
}

.calendar-header {
    .arrow-icon {
        font-size: 1em;
    }
    .left-button {
        border-radius: 5px 0px 0px 5px;
        border-right: 0px;
    }
    .right-button {
        border-radius: 0px 5px 5px 0px;
        border-left: 0px;
    }
}
</style>
