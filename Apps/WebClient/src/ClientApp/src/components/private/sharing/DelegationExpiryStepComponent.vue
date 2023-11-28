<script setup lang="ts">
import useVuelidate from "@vuelidate/core";
import { required } from "@vuelidate/validators";
import { computed, ref } from "vue";

import { useDelegationStore } from "@/stores/delegation";
import ValidationUtil from "@/utility/validationUtil";

const expiryOptions = [
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

const delegationStore = useDelegationStore();

const selectedExpiry = ref<number>();

const expectedExpiryDate = computed(() => {
    if (selectedExpiry.value === undefined) {
        throw new Error(
            "User must seclect expiry date range before calculating expiry date"
        );
    }
    if (selectedExpiry.value === 0) {
        return undefined;
    }
    const currentDate = new Date();
    currentDate.setMonth(currentDate.getMonth() + selectedExpiry.value);
    return currentDate;
});

const selectedDescription = computed(() => {
    const selectedOption = expiryOptions.find(
        (option) => option.value === selectedExpiry.value
    );
    return selectedOption === undefined ? "" : selectedOption.title;
});

const validations = computed(() => ({
    selectedExpiry: {
        required,
    },
}));

const expiryErrorMessages = computed(() =>
    ValidationUtil.getErrorMessages(v$.value.selectedExpiry)
);

const v$ = useVuelidate(validations, { selectedExpiry });

function saveStep() {
    v$.value.$touch();
    if (v$.value.$invalid || selectedExpiry.value === undefined) {
        return;
    }
    delegationStore.captureDelegationExpiry(
        selectedDescription.value,
        expectedExpiryDate.value
    );
}
</script>

<template>
    <v-row data-testid="delegation-expiry-step">
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
                v-model="selectedExpiry"
                placeholder="Set expiry date"
                eager
                :items="expiryOptions"
                data-testid="delegation-expiry-select"
                :error-messages="expiryErrorMessages"
                @blur="v$.selectedExpiry.$touch()"
            />
        </v-col>
    </v-row>
</template>
