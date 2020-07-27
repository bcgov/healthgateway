<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

#currentDate {
    width: 200px;
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

.item:hover {
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
<template>
    <div v-on-clickaway="close" class="select">
        <b-btn
            id="currentDate"
            class="px-2 m-0"
            :variant="isOpen ? 'primary' : 'outline-primary'"
            @click="open()"
        >
            {{ dateText }}
        </b-btn>
        <b-row class="items years" :class="{ selectHide: !isYearOpen }">
            <b-col
                v-for="(year, i) of years"
                :key="i"
                class="item col-12"
                @click="selectYear(year)"
            >
                {{ year }}
            </b-col>
        </b-row>
        <b-row class="items" :class="{ selectHide: !isMonthOpen }">
            <b-col class="item col-12" @click="open()">
                {{ selectedYear }}
            </b-col>
            <b-col
                v-for="(month, i) of monthsToDisplay"
                :key="i"
                :class="getDisplayMonthCss(month)"
                @click="selectMonth(i)"
            >
                {{ month.Title }}
            </b-col>
        </b-row>
    </div>
</template>

<script lang="ts">
import Vue from "vue";
import { Component, Prop, Watch, Emit } from "vue-property-decorator";
import moment from "moment";
import { directive as onClickaway } from "vue-clickaway";
import { DateGroup } from "@/models/timelineEntry";
import { mount } from "@vue/test-utils";

class MonthToDisplay {
    public Title: string = "";
    public HasData: boolean = false;
}

@Component({
    directives: {
        onClickaway: onClickaway,
    },
})
export default class MonthYearPickerComponent extends Vue {
    @Prop() currentDate!: Date;
    @Prop() availableMonths!: Date[];
    public isYearOpen: boolean = false;
    public isMonthOpen: boolean = false;
    public selectedYear: number = new Date().getFullYear();
    public selectedMonth: number = new Date().getMonth();
    private selectedDate: Date = new Date();
    private years: number[] = [];
    private get monthsToDisplay() {
        let availableMonthsOfSelectedYear = this.availableMonths.filter(
            (m) => m.getFullYear() === this.selectedYear
        );
        let monthsToDisplay = [
            { Title: "Jan", HasData: false },
            { Title: "Feb", HasData: false },
            { Title: "Mar", HasData: false },
            { Title: "Apr", HasData: false },
            { Title: "May", HasData: false },
            { Title: "Jun", HasData: false },
            { Title: "Jul", HasData: false },
            { Title: "Aug", HasData: false },
            { Title: "Sep", HasData: false },
            { Title: "Oct", HasData: false },
            { Title: "Nov", HasData: false },
            { Title: "Dec", HasData: false },
        ];
        availableMonthsOfSelectedYear.forEach((date) => {
            const month: number = date.getMonth();
            monthsToDisplay[month].HasData = true;
        });
        debugger;
        return monthsToDisplay;
    }

    private getDisplayMonthCss(displayMonth: MonthToDisplay) {
        if (displayMonth.HasData) return "item col-4";
        else return "item col-4 no-data";
    }

    private get isOpen(): boolean {
        return this.isYearOpen || this.isMonthOpen;
    }

    private get dateText(): string {
        return moment(this.selectedDate).format("MMMM yyyy");
    }

    private selectYear(year: number): void {
        this.selectedYear = year;
        this.open();
    }

    private selectMonth(month: number): void {
        if (this.monthsToDisplay[month].HasData) {
            this.selectedMonth = month;
            this.selectedDate = new Date(
                this.selectedYear,
                this.selectedMonth,
                1
            );
            this.dateChanged();
            this.close();
        }
    }

    @Emit()
    public dateChanged() {
        return this.selectedDate;
    }

    private close(): void {
        this.isYearOpen = false;
        this.isMonthOpen = false;
        this.selectedMonth = this.selectedDate.getMonth();
        this.selectedYear = this.selectedDate.getFullYear();
    }

    private open(): void {
        this.isMonthOpen = !this.isMonthOpen;
        this.isYearOpen = !this.isMonthOpen;
    }

    @Watch("currentDate")
    public onCurrentDateChange(currentDate: Date) {
        this.selectedDate = currentDate;
        this.close();
    }

    @Watch("availableMonths")
    public onAvailableMonths() {
        this.availableMonths.forEach((date) => {
            var year: number = date.getFullYear();
            if (!this.years.some((y) => y == year)) {
                this.years.push(year);
            }
        });
        var currentYear: number = this.selectedDate.getFullYear();
        if (!this.years.some((y) => y == currentYear)) {
            this.years.push(currentYear);
        }
    }
}
</script>
