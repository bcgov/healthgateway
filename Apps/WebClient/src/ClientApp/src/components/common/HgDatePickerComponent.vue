<script setup lang="ts">
import "@vuepic/vue-datepicker/dist/main.css";

import { useVuelidate } from "@vuelidate/core";
import { helpers } from "@vuelidate/validators";
import { VueDatePicker } from "@vuepic/vue-datepicker";
import { vMaska } from "maska/vue";
import { computed, ref, watch } from "vue";

import HgIconButtonComponent from "@/components/common/HgIconButtonComponent.vue";
import { DateWrapper, IDateWrapper } from "@/models/dateWrapper";
import { useLayoutStore } from "@/stores/layout";
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

const layoutStore = useLayoutStore();

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
const minJsDate = computed(() => props.minDate.toJSDate());
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
        v-maska="maskOptions"
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
                :time-config="{ enableTimePicker: false }"
                model-type="format"
                :formats="{ month: 'LLLL', input: 'yyyy-MM-dd' }"
                :min-date="minJsDate"
                :max-date="maxJsDate"
                auto-apply
                six-weeks="append"
                :teleport="true"
                :centered="layoutStore.isMobile"
                :floating="{ offset: 16 }"
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
