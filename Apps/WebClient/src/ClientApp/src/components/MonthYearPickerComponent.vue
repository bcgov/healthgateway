<script setup lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import {
    faChevronLeft,
    faChevronRight,
} from "@fortawesome/free-solid-svg-icons";
import { computed, ref, watch } from "vue";

import { DateWrapper, IDateWrapper } from "@/models/dateWrapper";

library.add(faChevronLeft, faChevronRight);

class MonthToDisplay {
    public title = "";
    public hasData = false;
}

interface Props {
    currentMonth: DateWrapper;
    availableMonths: DateWrapper[];
}
const props = defineProps<Props>();

const isYearOpen = ref(false);
const isMonthOpen = ref(false);
const selectedYear = ref(new DateWrapper().year());
const selectedMonth = ref(new DateWrapper().month());
const selectedDate = ref<IDateWrapper>(new DateWrapper());
const years = ref<number[]>([]);

watch(props.currentMonth, () => {
    onCurrentMonthChange(props.currentMonth);
});

function onCurrentMonthChange(currentMonth: DateWrapper): void {
    selectedDate.value = currentMonth;
    close();
}

watch(props.availableMonths, () => {
    onAvailableMonths();
});

function onAvailableMonths(): void {
    props.availableMonths.forEach((date) => {
        const year = date.year();
        if (!years.value.some((y) => y == year)) {
            years.value.push(year);
        }
    });
    const currentYear = selectedDate.value.year();
    if (!years.value.some((y) => y == currentYear)) {
        years.value.push(currentYear);
    }
    // Sort years by descending
    years.value.sort((a, b) => b - a);
}

const monthsToDisplay = computed<MonthToDisplay[]>(() => {
    const availableMonthsOfSelectedYear = props.availableMonths.filter(
        (m) => m.year() === selectedYear.value
    );

    const monthsToDisplay = [
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
        const monthIndex = date.month() - 1;
        monthsToDisplay[monthIndex].hasData = true;
    });
    return monthsToDisplay;
});

const dateText = computed<string>(() => {
    return selectedDate.value.format("MMMM yyyy");
});

function isCurrentYear(year: number): boolean {
    return props.currentMonth.year() === year;
}

function isCurrentMonth(displayMonth: MonthToDisplay): boolean {
    const monthIndex = props.currentMonth.month() - 1;
    return (
        monthIndex === monthsToDisplay.value.indexOf(displayMonth) &&
        props.currentMonth.year() === selectedYear.value
    );
}

function selectYear(year: number): void {
    selectedYear.value = year;
    open();
}

function previousYear(): void {
    selectedYear.value =
        years.value[years.value.indexOf(selectedYear.value) + 1];
}

function nextYear(): void {
    selectedYear.value =
        years.value[years.value.indexOf(selectedYear.value) - 1];
}

function selectMonth(monthIndex: number): void {
    if (monthsToDisplay.value[monthIndex].hasData) {
        selectedMonth.value = monthIndex + 1;
        selectedDate.value = DateWrapper.fromNumerical(
            selectedYear.value,
            selectedMonth.value,
            1
        );
        dateChanged();
        close();
    }
}

const emit = defineEmits<{
    (e: "date-changed", newValue: IDateWrapper): void;
}>();

function dateChanged(): void {
    emit("date-changed", selectedDate.value);
}

function close(): void {
    isYearOpen.value = false;
    isMonthOpen.value = false;
    selectedMonth.value = selectedDate.value.month();
    selectedYear.value = selectedDate.value.year();
}

function open(): void {
    isMonthOpen.value = !isMonthOpen.value;
    isYearOpen.value = !isYearOpen.value;
}

defineExpose({
    isYearOpen,
    isMonthOpen,
    selectedYear,
    selectedMonth,
});
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
