<script setup lang="ts">
import useVuelidate from "@vuelidate/core";
import { minLength, required } from "@vuelidate/validators";
import { computed, ref } from "vue";

import { EntryType, entryTypeMap } from "@/constants/entryType";
import { useDelegationStore } from "@/stores/delegation";
import ConfigUtil from "@/utility/configUtil";
import ValidationUtil from "@/utility/validationUtil";

defineExpose({ saveStep });

const delegationStore = useDelegationStore();

const selectedRecordTypes = ref<EntryType[]>([]);

let entryTypeOptions: Array<{ title: string; value: EntryType }> = [];

const validations = computed(() => ({
    selectedRecordTypes: {
        required,
        minLength: minLength(1),
    },
}));

const recordTypesErrorMessages = computed(() =>
    ValidationUtil.getErrorMessages(v$.value.selectedRecordTypes)
);

const v$ = useVuelidate(validations, {
    selectedRecordTypes,
});

function saveStep(): void {
    v$.value.$touch();
    if (v$.value.$invalid) {
        return;
    }
    delegationStore.captureDelegationRecordTypes(selectedRecordTypes.value);
}

// INIT
entryTypeOptions = [...entryTypeMap]
    .filter(([_, entryDetails]) =>
        ConfigUtil.isDatasetEnabled(entryDetails.type)
    )
    .map(([_, entryDetails]) => {
        return entryDetails === undefined
            ? undefined
            : {
                  title: entryDetails.name,
                  value: entryDetails.type,
              };
    })
    .filter((option) => option !== undefined) as Array<{
    title: string;
    value: EntryType;
}>;

if (delegationStore.delegationWizardState?.recordTypes !== undefined) {
    selectedRecordTypes.value =
        delegationStore.delegationWizardState.recordTypes;
}
</script>

<template>
    <v-row data-testid="delegation-record-types-step">
        <v-col cols="12">
            <h5 class="text-h6 font-weight-bold mb-4">Health Records:</h5>
            <p class="text-body-1">
                You can always change what's shared or stop sharing. If you stop
                sharing, they won't be able to access your health records.
            </p>
        </v-col>
        <v-col cols="12">
            <v-select
                v-model="selectedRecordTypes"
                placeholder="Health Records"
                multiple
                chips
                eager
                :items="entryTypeOptions"
                data-testid="delegation-record-types-select"
                :error-messages="recordTypesErrorMessages"
                @blur="v$.selectedRecordTypes.$touch()"
            />
        </v-col>
    </v-row>
</template>
