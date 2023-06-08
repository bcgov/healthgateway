<script setup lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faCalendar } from "@fortawesome/free-solid-svg-icons";
import { useVuelidate } from "@vuelidate/core";
import { BFormDatepicker } from "bootstrap-vue";
import { computed } from "vue";

import { DateWrapper } from "@/models/dateWrapper";

library.add(faCalendar);

interface Props {
    value?: string;
    state?: boolean;
    maxDate?: DateWrapper;
}
const props = withDefaults(defineProps<Props>(), {
    value: "",
    state: undefined,
    maxDate: () => new DateWrapper("2100-01-01"),
});

const emit = defineEmits<{
    (e: "update:value", value: string): void;
    (e: "is-date-valid", value: boolean): void;
    (e: "blur"): void;
}>();

const inputValue = computed<string>({
    get() {
        return fromIsoDate(props.value);
    },
    set(value: string) {
        let isoDate = "";
        if (value && isValid.value !== false) {
            isoDate = DateWrapper.fromStringFormat(value).toISODate();
        }

        emit("update:value", isoDate);
        emit("is-date-valid", isValid.value ?? true);
    },
});
const isValid = computed(() => {
    const param = v$.value.inputValue;
    return param.$dirty === false
        ? undefined
        : !param.$invalid && !param.$pending;
});
const internalState = computed(() =>
    isValid.value === false ? false : props.state
);
const validations = computed(() => ({
    inputValue: {
        minValue: (value: string) =>
            !value ||
            DateWrapper.fromStringFormat(value).isAfter(
                new DateWrapper("1900-01-01")
            ),
        maxValue: (value: string) =>
            !value ||
            DateWrapper.fromStringFormat(value).isBefore(props.maxDate),
    },
}));

const v$ = useVuelidate(validations, { inputValue });

function fromIsoDate(isoDate: string): string {
    return isoDate ? new DateWrapper(isoDate).format().toUpperCase() : "";
}
</script>

<template>
    <b-input-group>
        <b-form-input
            v-model="inputValue"
            v-mask="'####-AAA-##'"
            type="text"
            placeholder="YYYY-MMM-DD"
            autocomplete="off"
            :state="internalState"
            @blur.native="() => emit('blur')"
            @click.native.capture.stop
        />
        <b-input-group-append>
            <b-form-datepicker
                :value="value"
                :max="maxDate.toJSDate()"
                menu-class="datepicker-style"
                button-only
                right
                locale="en-CA"
                @input="(date) => (inputValue = fromIsoDate(date))"
            >
                <template #button-content>
                    <hg-icon icon="calendar" size="small" />
                </template>
            </b-form-datepicker>
        </b-input-group-append>
    </b-input-group>
</template>

<style lang="scss">
@import "@/assets/scss/_variables.scss";
// Fixes datepicker displaying under site header on mobile
.datepicker-style {
    z-index: $z_datepicker;
}
</style>
