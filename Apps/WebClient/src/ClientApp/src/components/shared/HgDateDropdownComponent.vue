<script lang="ts">
import Vue from "vue";
import { Component, Emit, Model, Prop, Watch } from "vue-property-decorator";

import { DateWrapper } from "@/models/dateWrapper";

interface ISelectOption {
    text: string;
    value: unknown;
}

@Component
export default class HgDateDropdownComponent extends Vue {
    @Model("change", { type: String }) public model!: string;
    @Prop() state?: boolean;
    @Prop({ required: false, default: false }) allowFuture!: boolean;
    @Prop({ required: false, default: true }) allowPast!: boolean;
    @Prop({ required: false, default: null }) minYear?: number;
    @Prop({ required: false, default: false }) disabled!: boolean;

    private day: number | null = null;
    private month: number | null = null;
    private year: number | null = null;
    private value: string | null = "";
    private currentYear = new DateWrapper().year();
    private currentMonth = new DateWrapper().month();
    private currentDay = new DateWrapper().day();

    private monthValues = [
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

    private mounted(): void {
        this.value = this.model;
    }

    private get getYears(): ISelectOption[] {
        let minYear = this.minYear;
        let maxYear = 0;

        if (!minYear) {
            minYear = this.allowPast ? 1900 : this.currentYear;
        } else {
            minYear = this.allowPast ? minYear : this.currentYear;
        }

        if (!maxYear) {
            maxYear = this.currentYear + 20;
        }

        if (!this.allowFuture) {
            maxYear = this.currentYear;
        }

        let yearOptions: ISelectOption[] = [{ value: null, text: "Year" }];
        for (let i = maxYear; i >= minYear; i--) {
            yearOptions.push({ value: i, text: i.toString() });
        }

        return yearOptions;
    }

    private get getMonths(): ISelectOption[] {
        let start = 1;
        let end = 12;
        if (!this.allowPast && this.year === this.currentYear) {
            start = this.currentMonth;
        }
        if (!this.allowFuture && this.year === this.currentYear) {
            end = this.currentMonth;
        }

        let monthOptions: ISelectOption[] = [{ value: null, text: "Month" }];
        for (let monthNo = start; monthNo <= end; monthNo++) {
            monthOptions.push({
                value: monthNo,
                text: this.monthValues[monthNo - 1],
            });
        }

        return monthOptions;
    }

    private get getDays(): ISelectOption[] {
        let day;
        let start = 1;
        let end = 31;

        if (
            !this.allowPast &&
            this.year === this.currentYear &&
            this.month === this.currentMonth &&
            start < this.currentDay
        ) {
            start = this.currentDay;
        }

        let numDaysInMonth = new Date(
            this.year ?? this.currentYear,
            this.month ?? this.currentMonth,
            0
        ).getDate();
        if (end > numDaysInMonth) {
            end = numDaysInMonth;
        }

        if (
            !this.allowFuture &&
            this.year === this.currentYear &&
            this.month === this.currentMonth &&
            end > this.currentDay
        ) {
            end = this.currentDay;
        }

        let dayOptions: ISelectOption[] = [{ value: null, text: "Day" }];

        for (let j = start; j <= end; j++) {
            day = j;

            dayOptions.push({ value: day, text: day.toString() });
        }

        return dayOptions;
    }

    private onChange(): void {
        if (this.year && this.month && this.day) {
            this.value = DateWrapper.fromNumerical(
                this.year,
                this.month,
                this.day
            ).toISODate();
        } else {
            this.value = null;
        }

        this.updateModel();
    }

    @Emit("change")
    private updateModel(): string | null {
        return this.value;
    }

    @Watch("model")
    private onModelChanged(): void {
        this.value = this.model;
    }

    @Emit("blur")
    private onBlur(): void {
        return;
    }

    private getClass(): string {
        if (this.state === true) {
            return "valid";
        } else if (this.state === false) {
            return "invalid";
        } else {
            return "";
        }
    }
}
</script>

<template>
    <b-row no-gutters>
        <b-col cols="auto">
            <b-form-select
                v-model="year"
                data-testid="formSelectYear"
                :class="getClass(state)"
                :options="getYears"
                aria-label="Year"
                :disabled="disabled"
                @change="onChange"
            />
        </b-col>
        <b-col class="px-2">
            <b-form-select
                v-model="month"
                data-testid="formSelectMonth"
                :class="getClass(state)"
                :options="getMonths"
                aria-label="Month"
                :disabled="disabled"
                @change="onChange"
            />
        </b-col>
        <b-col cols="auto">
            <b-form-select
                v-model="day"
                data-testid="formSelectDay"
                :class="getClass(state)"
                :options="getDays"
                aria-label="Day"
                :disabled="disabled"
                @change="onChange"
                @blur="onBlur"
            />
        </b-col>
    </b-row>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

.valid {
    border-color: $success;
}

.invalid {
    border-color: $danger;
}
</style>
