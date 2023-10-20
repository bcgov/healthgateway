<script setup lang="ts">
import "@vuepic/vue-datepicker/dist/main.css";

import { useVuelidate } from "@vuelidate/core";
import { helpers } from "@vuelidate/validators";
import VueDatePicker from "@vuepic/vue-datepicker";
import { vMaska } from "maska";
import { computed, ref, watch } from "vue";

import HgIconButtonComponent from "@/components/common/HgIconButtonComponent.vue";
import { DateWrapper, IDateWrapper } from "@/models/dateWrapper";
import { useAppStore } from "@/stores/app";
import ValidationUtil from "@/utility/validationUtil";

interface Props {
    modelValue?: string;
    label?: string;
    state?: boolean;
    errorMessages?: string[];
    minDate?: IDateWrapper;
    maxDate?: IDateWrapper;
}
const props = withDefaults(defineProps<Props>(), {
    modelValue: "",
    label: "Date",
    state: undefined,
    errorMessages: () => [],
    minDate: () => DateWrapper.fromNumerical(1900, 1, 1),
    maxDate: () => DateWrapper.fromNumerical(2099, 12, 31),
});

const emit = defineEmits<{
    (e: "update:model-value", value: string): void;
    (e: "validity-updated", value: boolean): void;
    (e: "blur"): void;
}>();

const maskOptions = {
    mask: "####-@@@-##",
    eager: true,
    postProcess: (value: string) => value.toUpperCase(),
};

const appStore = useAppStore();

const internalValue = ref(fromIsoFormat(props.modelValue));

const datePickerValue = computed<string>({
    get() {
        return props.modelValue;
    },
    set(value: string) {
        emit("update:model-value", value);
        notifyTouched(true);
    },
});
const textFieldValue = computed<string>({
    get() {
        return internalValue.value;
    },
    set(value: string) {
        internalValue.value = value ?? "";
        const convertedValue =
            internalState.value === false || !internalValue.value
                ? ""
                : toIsoFormat(internalValue.value);

        if (convertedValue !== props.modelValue) {
            emit("update:model-value", convertedValue);
        }
    },
});
const internalState = computed(() =>
    ValidationUtil.isValid(v$.value.textFieldValue)
);
const internalErrorMessages = computed(() =>
    ValidationUtil.getErrorMessages(v$.value.textFieldValue)
);
const validations = computed(() => ({
    textFieldValue: {
        date: helpers.withMessage("Invalid date", validateDateFormat),
    },
}));
const maxJsDate = computed(() => props.maxDate.toJSDate());

const v$ = useVuelidate(validations, { textFieldValue });

function fromIsoFormat(value: string): string {
    return DateWrapper.fromIsoDate(value).format().toUpperCase();
}

function toIsoFormat(value: string): string {
    return DateWrapper.fromStringFormat(value).toISODate();
}

function validateDateFormat(value: string): boolean {
    return value ? DateWrapper.fromStringFormat(value).isValid() : true;
}

function handleBlur(): void {
    v$.value.textFieldValue.$touch();
    notifyTouched(false);
}

function notifyTouched(fromDatePicker: boolean): void {
    const valid = fromDatePicker ? true : internalState.value === true;

    emit("blur");
    emit("validity-updated", valid);
}

watch(
    () => props.modelValue,
    (value) => (internalValue.value = fromIsoFormat(value))
);
</script>

<template>
    <v-text-field
        v-model="textFieldValue"
        v-maska:[maskOptions]
        clearable
        type="text"
        :label="label"
        :placeholder="DateWrapper.defaultFormat.toUpperCase()"
        :error="internalState === false || state === false"
        :error-messages="internalErrorMessages.concat(errorMessages)"
        @blur="handleBlur"
    >
        <template #append>
            <VueDatePicker
                v-model="datePickerValue"
                :max-date="maxJsDate"
                :enable-time-picker="false"
                model-type="format"
                format="yyyy-MM-dd"
                auto-apply
                month-name-format="long"
                six-weeks="append"
                :offset="16"
                calendar-cell-class-name="rounded-circle"
                :teleport="true"
                :teleport-center="appStore.isMobile"
            >
                <template #trigger>
                    <HgIconButtonComponent icon="fas fa-calendar" />
                </template>
            </VueDatePicker>
        </template>
    </v-text-field>
</template>

<style lang="scss">
.dp__theme_light {
    --dp-text-color: rgba(
        var(--v-theme-on-background),
        var(--v-high-emphasis-opacity)
    );
    --dp-primary-color: rgb(var(--v-theme-focus));
    --dp-primary-text-color: rgb(var(--v-theme-on-focus));
}
</style>
