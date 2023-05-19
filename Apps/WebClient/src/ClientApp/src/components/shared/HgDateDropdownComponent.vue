<script setup lang="ts">
import { computed, ref } from "vue";

import { DateWrapper } from "@/models/dateWrapper";

interface Props {
    value: string;
    state?: boolean;
    allowFuture?: boolean;
    allowPast?: boolean;
    minYear?: number;
    disabled?: boolean;
}
const props = withDefaults(defineProps<Props>(), {
    state: undefined,
    allowFuture: false,
    allowPast: true,
    minYear: undefined,
    disabled: false,
});

const emit = defineEmits<{
    (e: "input", newValue: string): void;
    (e: "blur"): void;
}>();

interface ISelectOption {
    text: string | null;
    value: unknown;
}

const currentYear = new DateWrapper().year();
const currentMonth = new DateWrapper().month();
const currentDay = new DateWrapper().day();
const monthNames = [
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

const day = ref<number | null>(null);
const month = ref<number | null>(null);
const year = ref<number | null>(null);

const classes = computed(() => {
    switch (props.state) {
        case true:
            return "valid";
        case false:
            return "invalid";
        default:
            return "";
    }
});
const daysInMonth = computed(() =>
    DateWrapper.daysInMonth(
        year.value ?? currentYear,
        month.value ?? currentMonth
    )
);
const yearOptions = computed(() => {
    const minYear = props.minYear ?? 1900;
    const start = props.allowPast ? minYear : currentYear;
    const end = props.allowFuture ? currentYear + 20 : currentYear;

    const yearOptions: ISelectOption[] = [{ value: null, text: "Year" }];
    for (let i = end; i >= start; i--) {
        yearOptions.push({ value: i, text: i.toString() });
    }

    return yearOptions;
});
const monthOptions = computed(() => {
    const isCurrent = year.value === currentYear;
    const start = !props.allowPast && isCurrent ? currentMonth : 1;
    const end = !props.allowFuture && isCurrent ? currentMonth : 12;
    const monthOptions: ISelectOption[] = [{ value: null, text: "Month" }];

    for (let i = start; i <= end; i++) {
        monthOptions.push({
            value: i,
            text: monthNames[i - 1],
        });
    }

    return monthOptions;
});
const dayOptions = computed(() => {
    const isCurrent =
        year.value === currentYear && month.value === currentMonth;
    const start = !props.allowPast && isCurrent ? currentDay : 1;
    const end =
        !props.allowFuture && isCurrent ? currentDay : daysInMonth.value;
    const dayOptions: ISelectOption[] = [{ value: null, text: "Day" }];

    for (let i = start; i <= end; i++) {
        dayOptions.push({ value: i, text: i.toString() });
    }

    return dayOptions;
});

function onChange(): void {
    if (day.value !== null && day.value > daysInMonth.value) {
        day.value = null;
    }

    if (year.value && month.value && day.value) {
        const date = DateWrapper.fromNumerical(
            year.value,
            month.value,
            day.value
        );
        emit("input", date.toISODate() || "");
    } else {
        emit("input", "");
    }
}
</script>

<template>
    <b-row no-gutters>
        <b-col cols="auto">
            <b-form-select
                v-model="year"
                data-testid="formSelectYear"
                :class="classes"
                :options="yearOptions"
                aria-label="Year"
                :disabled="disabled"
                @change="onChange"
            />
        </b-col>
        <b-col class="px-2">
            <b-form-select
                v-model="month"
                data-testid="formSelectMonth"
                :class="classes"
                :options="monthOptions"
                aria-label="Month"
                :disabled="disabled"
                @change="onChange"
            />
        </b-col>
        <b-col cols="auto">
            <b-form-select
                v-model="day"
                data-testid="formSelectDay"
                :class="classes"
                :options="dayOptions"
                aria-label="Day"
                :disabled="disabled"
                @change="onChange"
                @blur="emit('blur')"
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
