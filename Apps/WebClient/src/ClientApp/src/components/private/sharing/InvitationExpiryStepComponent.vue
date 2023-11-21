<script setup lang="ts">
import useVuelidate from "@vuelidate/core";
import { required } from "@vuelidate/validators";
import { computed, ref } from "vue";

import { DateWrapper } from "@/models/dateWrapper";
import { useDelegateStore } from "@/stores/delegate";
import ValidationUtil from "@/utility/validationUtil";

const dateRangeOptions = [
    {
        title: "3 months",
        value: 3,
    },
    {
        title: "6 months",
        value: 6,
    },
    {
        title: "1 year",
        value: 12,
    },
    {
        title: "Never",
        value: 0,
    },
];

defineExpose({ saveStep });

const delegateStore = useDelegateStore();

const selectedExpiryRange = ref<number>();

const expectedExpiryDate = computed(() => {
    if (
        selectedExpiryRange.value === undefined ||
        selectedExpiryRange.value === 0
    )
        return undefined;
    const currentDate = new Date();
    currentDate.setMonth(currentDate.getMonth() + selectedExpiryRange.value);
    return DateWrapper.fromIsoDate(currentDate.toISOString()).format(
        "dd MMM yyyy"
    );
});

const selectedDescription = computed(() => {
    const selectedOption = dateRangeOptions.find(
        (option) => option.value === selectedExpiryRange.value
    );
    return selectedOption === undefined ? "" : selectedOption.title;
});

const validations = computed(() => ({
    selectedExpiryRange: {
        required,
    },
}));

const expiryDateErrorMessages = computed(() =>
    ValidationUtil.getErrorMessages(v$.value.selectedExpiryRange)
);

const v$ = useVuelidate(validations, { selectedExpiryRange });

function saveStep() {
    v$.value.$touch();
    if (v$.value.$invalid || selectedExpiryRange.value === undefined) return;
    delegateStore.captureExpiryDate(
        selectedDescription.value,
        expectedExpiryDate.value
    );
}
</script>

<template>
    <v-row>
        <v-col cols="12">
            <h5 class="text-h6 font-weight-bold mb-4">Expire Access:</h5>
            <p class="text-body-1">
                You can always update when access should expire or stop sharing.
                If you stop sharing, they won't be able to access your health
                records.
            </p>
        </v-col>
        <v-col cols="12">
            <v-select
                v-model="selectedExpiryRange"
                placeholder="Set expiry date"
                eager
                :items="dateRangeOptions"
                data-testid="invitation-datasources"
                :error-messages="expiryDateErrorMessages"
                @blur="v$.selectedExpiryRange.$touch()"
            />
        </v-col>
        <v-col>
            <p class="text-body-2">
                <span v-if="selectedExpiryRange !== undefined">
                    Expiected expiry date:
                    {{ expectedExpiryDate ?? "Never" }}
                </span>
            </p>
        </v-col>
    </v-row>
</template>
