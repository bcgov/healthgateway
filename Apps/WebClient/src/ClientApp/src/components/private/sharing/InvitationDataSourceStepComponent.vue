<script setup lang="ts">
import useVuelidate from "@vuelidate/core";
import { minLength, required } from "@vuelidate/validators";
import { computed, ref } from "vue";

import { DataSource } from "@/constants/dataSource";
import { EntryType, entryTypeMap } from "@/constants/entryType";
import { useDelegateStore } from "@/stores/delegate";
import ConfigUtil from "@/utility/configUtil";
import DataSourceUtil from "@/utility/dataSourceUtil";
import ValidationUtil from "@/utility/validationUtil";

defineExpose({ saveStep });

const delegateStore = useDelegateStore();

const selectedDataSources = ref<DataSource[]>([]);

let dataSourceOptions: Array<{ title: string; value: DataSource }> = [];

const validations = computed(() => ({
    selectedDataSources: {
        required,
        minLength: minLength(1),
    },
}));

const dataSourcesErrorMessages = computed(() =>
    ValidationUtil.getErrorMessages(v$.value.selectedDataSources)
);

const v$ = useVuelidate(validations, { selectedDataSources });

function saveStep(): void {
    v$.value.$touch();
    if (v$.value.$invalid) return;
    delegateStore.captureInvitationDataSources(selectedDataSources.value);
}

// INIT
dataSourceOptions = ConfigUtil.enabledDatasets()
    .map((dataset: EntryType) => {
        const entryDetails = entryTypeMap.get(dataset);
        return entryDetails === undefined
            ? undefined
            : {
                  title: entryDetails.name,
                  value: DataSourceUtil.getDataSource(dataset),
              };
    })
    .filter((option) => option !== undefined) as Array<{
    title: string;
    value: DataSource;
}>;

if (delegateStore.invitationWizardState?.dataSources !== undefined) {
    selectedDataSources.value = delegateStore.invitationWizardState.dataSources;
}
</script>

<template>
    <v-row>
        <v-col cols="12">
            <h5 class="text-h6 font-weight-bold mb-4">Health Records:</h5>
            <p class="text-body-1">
                You can always change what's shared or stop sharing. If you stop
                sharing, they won't be able to access your health records.
            </p>
        </v-col>
        <v-col cols="12">
            <v-select
                v-model="selectedDataSources"
                placeholder="Health Records"
                multiple
                chips
                eager
                :items="dataSourceOptions"
                data-testid="invitation-datasources"
                :error-messages="dataSourcesErrorMessages"
                @blur="v$.selectedDataSources.$touch()"
            />
        </v-col>
    </v-row>
</template>
