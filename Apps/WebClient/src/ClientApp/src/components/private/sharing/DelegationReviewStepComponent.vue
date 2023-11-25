<script setup lang="ts">
import { computed } from "vue";

import DisplayFieldComponent from "@/components/common/DisplayFieldComponent.vue";
import { EntryType, entryTypeMap } from "@/constants/entryType";
import { useDelegationStore } from "@/stores/delegation";

defineExpose({ saveStep });

const delegationStore = useDelegationStore();

const wizardState = computed(() => delegationStore.delegationWizardState);
const delegationExpiryDescription = computed(() => {
    if (wizardState.value?.expiryDate === undefined) {
        return "Never";
    }
    return `${wizardState.value.expiryDateLabel} (${delegationStore.expiryDateDisplay})`;
});

function saveStep() {
    delegationStore.submitDelegationDialog();
}

function recordTypeName(entryType: EntryType): string {
    return entryTypeMap.get(entryType)?.name ?? "Unknown Record Type";
}
</script>

<template>
    <v-row data-testid="delegation-review-step">
        <v-col cols="12">
            <h5 class="text-h6 font-weight-bold">
                Review the following information:
            </h5>
        </v-col>
        <v-col v-if="wizardState === undefined">
            <p class="text-body-1 text-red">
                You have reached this step in error, please close this dialog
                and try creating this invitation again.
            </p>
        </v-col>
        <v-col v-else cols="12" class="d-flex flex-column justify-space-evenly">
            <DisplayFieldComponent
                name-class="font-weight-bold"
                name="Nickname"
                data-testid="review-nickname"
                :value="wizardState.nickname"
            />
            <DisplayFieldComponent
                name-class="font-weight-bold"
                name="Inviting"
                data-testid="review-email"
                :value="wizardState.email"
            />
            <DisplayFieldComponent
                name-class="font-weight-bold"
                name="Health Records Type"
            >
                <template #value>
                    <v-chip
                        v-for="entryType in wizardState.recordTypes"
                        :key="entryType"
                        class="ml-1"
                        color="primary"
                        text-color="white"
                        density="compact"
                        :data-testid="`review-record-type-${entryType
                            .toLowerCase()
                            .trim()}`"
                    >
                        {{ recordTypeName(entryType) }}
                    </v-chip>
                </template>
            </DisplayFieldComponent>
            <DisplayFieldComponent
                name-class="font-weight-bold"
                name="Expiry Date"
                data-testid="review-expiry-date"
                :value="delegationExpiryDescription"
            />
        </v-col>
        <v-col>
            <p class="text-body-1">
                After this step, an invite will be sent but the
                <b>other user won't be able to access your health records</b>
                unless you provided them with a <b>sharing code</b> that gets
                generated once you complete this step.
            </p>
        </v-col>
    </v-row>
</template>
