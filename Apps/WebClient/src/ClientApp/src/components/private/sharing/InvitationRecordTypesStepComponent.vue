<script setup lang="ts">
import useVuelidate from "@vuelidate/core";
import { minLength, required } from "@vuelidate/validators";
import { computed, ref } from "vue";

import { EntryType, entryTypeMap } from "@/constants/entryType";
import { useDelegateStore } from "@/stores/delegate";
import ConfigUtil from "@/utility/configUtil";
import ValidationUtil from "@/utility/validationUtil";

defineExpose({ saveStep });

const delegateStore = useDelegateStore();

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
    if (v$.value.$invalid) return;
    delegateStore.captureInvitationRecordTypes(selectedRecordTypes.value);
}

// INIT
entryTypeOptions = ConfigUtil.enabledDatasets()
    .map((dataset: EntryType) => {
        const entryDetails = entryTypeMap.get(dataset);
        return entryDetails === undefined
            ? undefined
            : {
                  title: entryDetails.name,
                  value: dataset,
              };
    })
    .filter((option) => option !== undefined) as Array<{
    title: string;
    value: EntryType;
}>;

if (delegateStore.invitationWizardState?.recordTypes !== undefined) {
    selectedRecordTypes.value =
        delegateStore.invitationWizardState.recordTypes ?? [];
}
</script>

<template>
    <v-row data-testid="invitation-record-types-step">
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
                data-testid="invitation-record-types-select"
                :error-messages="recordTypesErrorMessages"
                @blur="v$.selectedRecordTypes.$touch()"
            />
        </v-col>
    </v-row>
</template>
