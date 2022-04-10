<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import {
    faChevronLeft,
    faChevronRight,
} from "@fortawesome/free-solid-svg-icons";
import Vue from "vue";
import { directive as onClickaway } from "vue-clickaway";
import { Component, Emit, Prop, Watch } from "vue-property-decorator";

import { DateWrapper } from "@/models/dateWrapper";

library.add(faChevronLeft, faChevronRight);

class MonthToDisplay {
    public title = "";
    public hasData = false;
}

@Component({
    directives: {
        onClickaway: onClickaway,
    },
})
export default class MonthYearPickerComponent extends Vue {
    @Prop() readonly currentMonth!: DateWrapper;
    @Prop() availableMonths!: DateWrapper[];

    public isYearOpen = false;
    public isMonthOpen = false;
    public selectedYear: number = new DateWrapper().year();
    public selectedMonth: number = new DateWrapper().month();
    private selectedDate: DateWrapper = new DateWrapper();
    private years: number[] = [];

    @Watch("currentMonth")
    public onCurrentMonthChange(currentMonth: DateWrapper): void {
        this.selectedDate = currentMonth;
        this.close();
    }

    @Watch("availableMonths")
    public onAvailableMonths(): void {
        this.availableMonths.forEach((date) => {
            var year: number = date.year();
            if (!this.years.some((y) => y == year)) {
                this.years.push(year);
            }
        });
        var currentYear: number = this.selectedDate.year();
        if (!this.years.some((y) => y == currentYear)) {
            this.years.push(currentYear);
        }
        // Sort years by descending
        this.years.sort((a, b) => b - a);
    }

    private get monthsToDisplay(): MonthToDisplay[] {
        let availableMonthsOfSelectedYear = this.availableMonths.filter(
            (m) => m.year() === this.selectedYear
        );

        let monthsToDisplay = [
            { title: "Jan", hasData: false },
            { title: "Feb", hasData: false },
            { title: "Mar", hasData: false },
            { title: "Apr", hasData: false },
            { title: "May", hasData: false },
            { title: "Jun", hasData: false },
            { title: "Jul", hasData: false },
            { title: "Aug", hasData: false },
            { title: "Sep", hasData: false },
            { title: "Oct", hasData: false },
            { title: "Nov", hasData: false },
            { title: "Dec", hasData: false },
        ];
        availableMonthsOfSelectedYear.forEach((date) => {
            // Months are indexed 0-11
            let monthIndex: number = date.month() - 1;
            monthsToDisplay[monthIndex].hasData = true;
        });
        return monthsToDisplay;
    }

    private get isOpen(): boolean {
        return this.isYearOpen || this.isMonthOpen;
    }

    private get dateText(): string {
        return this.selectedDate.format("MMMM yyyy");
    }

    private isCurrentYear(year: number): boolean {
        return this.currentMonth.year() === year;
    }

    private isCurrentMonth(displayMonth: MonthToDisplay): boolean {
        const monthIndex = this.currentMonth.month() - 1;
        return (
            monthIndex === this.monthsToDisplay.indexOf(displayMonth) &&
            this.currentMonth.year() === this.selectedYear
        );
    }

    private selectYear(year: number): void {
        this.selectedYear = year;
        this.open();
    }

    private previousYear(): void {
        this.selectedYear =
            this.years[this.years.indexOf(this.selectedYear) + 1];
    }
    private nextYear(): void {
        this.selectedYear =
            this.years[this.years.indexOf(this.selectedYear) - 1];
    }

    private selectMonth(monthIndex: number): void {
        if (this.monthsToDisplay[monthIndex].hasData) {
            this.selectedMonth = monthIndex + 1;
            this.selectedDate = DateWrapper.fromNumerical(
                this.selectedYear,
                this.selectedMonth,
                1
            );
            this.dateChanged();
            this.close();
        }
    }

    @Emit()
    public dateChanged(): DateWrapper {
        return this.selectedDate;
    }

    private close(): void {
        this.isYearOpen = false;
        this.isMonthOpen = false;
        this.selectedMonth = this.selectedDate.month();
        this.selectedYear = this.selectedDate.year();
    }

    private open(): void {
        this.isMonthOpen = !this.isMonthOpen;
        this.isYearOpen = !this.isMonthOpen;
    }
}
</script>

<template>
    <div v-on-clickaway="close" class="picker-wrapper">
        <hg-button
            id="currentDate"
            variant="secondary"
            squared
            block
            @click="open()"
        >
            {{ dateText }}
        </hg-button>
        <span class="picker-body">
            <b-row v-show="isYearOpen" class="years-wrapper">
                <b-col
                    v-for="(year, i) of years"
                    :key="i"
                    :data-testid="`yearBtn${year}`"
                    cols="12"
                >
                    <hg-button
                        :class="{ selected: isCurrentYear(year) }"
                        variant="icon-light"
                        block
                        @click="selectYear(year)"
                    >
                        {{ year }}
                    </hg-button>
                </b-col>
            </b-row>
            <span v-show="isMonthOpen">
                <b-row>
                    <b-col class="col-2">
                        <hg-button
                            squared
                            :disabled="
                                years.indexOf(selectedYear) == years.length - 1
                            "
                            variant="icon-light"
                            @click="previousYear()"
                        >
                            <hg-icon icon="chevron-left" size="small" />
                        </hg-button>
                    </b-col>
                    <b-col class="col-8">
                        <hg-button
                            id="selectedYearBtn"
                            variant="icon-light"
                            squared
                            block
                            @click="open()"
                        >
                            {{ selectedYear }}
                        </hg-button>
                    </b-col>
                    <b-col class="col-2">
                        <hg-button
                            squared
                            :disabled="years.indexOf(selectedYear) == 0"
                            variant="icon-light"
                            @click="nextYear()"
                        >
                            <hg-icon icon="chevron-right" size="small" />
                        </hg-button>
                    </b-col>
                </b-row>
                <b-row>
                    <b-col
                        v-for="(month, i) of monthsToDisplay"
                        :key="i"
                        cols="4"
                    >
                        <hg-button
                            :data-testid="`monthBtn${month.title}`"
                            variant="icon-light"
                            class="month-item"
                            :class="{ selected: isCurrentMonth(month) }"
                            :disabled="!month.hasData"
                            block
                            @click="selectMonth(i)"
                        >
                            {{ month.title }}</hg-button
                        >
                    </b-col>
                </b-row>
            </span>
        </span>
    </div>
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

.picker-wrapper {
    min-width: 160px;
    max-width: 160px;
    z-index: 10000;
}

.picker-body {
    position: absolute;
    box-shadow: $lightGrey 4px 4px 4px;
    border-right: 1px solid $soft_background;
    border-left: 1px solid $soft_background;
    border-bottom: 1px solid $soft_background;
    top: 50px;
    left: 0;
    right: 0;
    min-width: 200px;
    max-width: 200px;
    z-index: 10000;
    background-color: $light_background;
}

.years-wrapper {
    max-height: 250px;
    overflow-y: scroll;
}
</style>
