<script setup lang="ts">
import { computed, ref } from "vue";

import { DateWrapper } from "@/models/dateWrapper";
import SelectOption from "@/models/selectOption";

interface Props {
    modelValue: string;
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
    (e: "update:model-value", newValue: string): void;
    (e: "blur"): void;
}>();

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

const daysInMonth = computed(
    () =>
        DateWrapper.daysInMonth(
            year.value ?? currentYear,
            month.value ?? currentMonth
        ) ?? 31
);
const yearOptions = computed(() => {
    const minYear = props.minYear ?? 1900;
    const start = props.allowPast ? minYear : currentYear;
    const end = props.allowFuture ? currentYear + 20 : currentYear;

    const yearOptions: SelectOption[] = [{ value: null, title: "Year" }];
    for (let i = end; i >= start; i--) {
        yearOptions.push({ value: i, title: i.toString() });
    }

    return yearOptions;
});
const monthOptions = computed(() => {
    const isCurrent = year.value === currentYear;
    const start = !props.allowPast && isCurrent ? currentMonth : 1;
    const end = !props.allowFuture && isCurrent ? currentMonth : 12;
    const monthOptions: SelectOption[] = [{ value: null, title: "Month" }];

    for (let i = start; i <= end; i++) {
        monthOptions.push({
            value: i,
            title: monthNames[i - 1],
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
    const dayOptions: SelectOption[] = [{ value: null, title: "Day" }];

    for (let i = start; i <= end; i++) {
        dayOptions.push({ value: i, title: i.toString() });
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
        emit("update:model-value", date.toISODate());
    } else {
        emit("update:model-value", "");
    }
}
</script>

<template>
    <v-row dense>
        <v-col class="date-dropdown-column flex-grow-0">
            <v-select
                v-model="year"
                data-testid="formSelectYear"
                :error="state === false"
                :items="yearOptions"
                label="Year"
                :disabled="disabled"
                @update:model-value="onChange"
                @blur="emit('blur')"
            />
        </v-col>
        <v-col class="date-dropdown-column">
            <v-select
                v-model="month"
                data-testid="formSelectMonth"
                :error="state === false"
                :items="monthOptions"
                label="Month"
                :disabled="disabled"
                @update:model-value="onChange"
                @blur="emit('blur')"
            />
        </v-col>
        <v-col class="date-dropdown-column flex-grow-0">
            <v-select
                v-model="day"
                data-testid="formSelectDay"
                :error="state === false"
                :items="dayOptions"
                label="Day"
                :disabled="disabled"
                @update:model-value="onChange"
                @blur="emit('blur')"
            />
        </v-col>
    </v-row>
</template>

<style scoped>
.date-dropdown-column {
    flex-basis: 120px;
}
</style>
