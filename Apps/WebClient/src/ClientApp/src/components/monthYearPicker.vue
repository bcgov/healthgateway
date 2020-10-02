<script lang="ts">
import Vue from "vue";
import { Component, Prop, Watch, Emit } from "vue-property-decorator";
import { directive as onClickaway } from "vue-clickaway";
import { DateGroup } from "@/models/timelineEntry";
import { DateWrapper } from "@/models/dateWrapper";

class MonthToDisplay {
    public title: string = "";
    public hasData: boolean = false;
}

@Component({
    directives: {
        onClickaway: onClickaway,
    },
})
export default class MonthYearPickerComponent extends Vue {
    @Prop() currentMonth!: DateWrapper;
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
        this.years = this.years.sort((a, b) => b - a);
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

    private getDisplayYearCss(year: number) {
        return `item col-12 ${
            this.currentMonth.year() === year ? "selected" : ""
        }`;
    }

    private getDisplayMonthCss(displayMonth: MonthToDisplay) {
        if (displayMonth.hasData) {
            return `item col-4 ${
                this.currentMonth.month() ===
                    this.monthsToDisplay.indexOf(displayMonth) &&
                this.currentMonth.year() === this.selectedYear
                    ? "selected"
                    : ""
            }`;
        } else {
            return "item col-4 no-data";
        }
    }

    private selectYear(year: number): void {
        this.selectedYear = year;
        this.open();
    }

    private previousYear(): void {
        this.selectedYear = this.years[
            this.years.indexOf(this.selectedYear) + 1
        ];
    }
    private nextYear(): void {
        this.selectedYear = this.years[
            this.years.indexOf(this.selectedYear) - 1
        ];
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
    <div v-on-clickaway="close" class="select">
        <b-btn
            id="currentDate"
            squared
            class="m-0"
            :variant="isOpen ? 'primary' : 'light'"
            @click="open()"
        >
            {{ dateText }}
        </b-btn>
        <b-row class="items years" :class="{ selectHide: !isYearOpen }">
            <b-col
                v-for="(year, i) of years"
                :key="i"
                :data-testid="`yearBtn${year}`"
                :class="getDisplayYearCss(year)"
                @click="selectYear(year)"
            >
                {{ year }}
            </b-col>
        </b-row>
        <b-row class="items" :class="{ selectHide: !isMonthOpen }">
            <b-col class="col-2 p-0">
                <b-btn
                    squared
                    class="m-0 w-100 h-100"
                    :disabled="years.indexOf(selectedYear) == years.length - 1"
                    variant="light"
                    @click="previousYear()"
                >
                    <font-awesome-icon icon="chevron-left" size="sm" />
                </b-btn>
            </b-col>
            <b-col class="col-8 p-0">
                <b-btn
                    id="selectedYearBtn"
                    squared
                    class="m-0 w-100 h-100"
                    variant="light"
                    @click="open()"
                >
                    {{ selectedYear }}
                </b-btn>
            </b-col>
            <b-col class="col-2 p-0">
                <b-btn
                    squared
                    class="m-0 w-100 h-100"
                    :disabled="years.indexOf(selectedYear) == 0"
                    variant="light"
                    @click="nextYear()"
                >
                    <font-awesome-icon icon="chevron-right" size="sm" />
                </b-btn>
            </b-col>
            <b-col
                v-for="(month, i) of monthsToDisplay"
                :key="i"
                :data-testid="`monthBtn${month.title}`"
                :variant="month == selectedMonth ? 'primary' : 'light'"
                :class="getDisplayMonthCss(month)"
                @click="selectMonth(i)"
            >
                {{ month.title }}
            </b-col>
        </b-row>
    </div>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

#currentDate {
    width: 140px;
}
.select {
    position: relative;
    width: 100%;
    text-align: left;
    outline: none;
}

.items {
    margin-top: 2px;
    line-height: 45px;
    min-width: 200px;
    overflow: hidden;
    box-shadow: $lightGrey 4px 4px 4px;
    border-right: 1px solid $soft_background;
    border-left: 1px solid $soft_background;
    border-bottom: 1px solid $soft_background;
    position: absolute;
    background-color: $light_background;
    left: 0;
    right: 0;
    z-index: 10000;
}

.years {
    max-height: 250px;
    overflow-y: scroll;
}

.item {
    color: $soft_text;
    cursor: pointer;
    text-align: center;
}

.item:hover,
.selected {
    background-color: $primary;
    color: $primary_text;
}

.selectHide {
    display: none;
}

.no-data {
    background-color: lightgray;
}
</style>
