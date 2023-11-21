<script setup lang="ts">
import { computed } from "vue";

import DisplayFieldComponent from "@/components/common/DisplayFieldComponent.vue";
import { useDelegateStore } from "@/stores/delegate";

defineExpose({ saveStep });

const delegateStore = useDelegateStore();

const currentInvitation = computed(() => delegateStore.invitationWizardState);

function saveStep() {
    console.log("saveStep");
}
</script>

<template>
    <v-row>
        <v-col cols="12">
            <h5 class="text-h6 font-weight-bold mb-4">
                Review the following information:
            </h5>
        </v-col>
        <v-col v-if="currentInvitation === undefined">
            <p class="text-body-1 text-red">
                You have reached this step in error, please close this dialog
                and try creating this invitation again.
            </p>
        </v-col>
        <v-col v-else cols="12">
            <DisplayFieldComponent
                name-class="font-weight-bold"
                name="Nickname"
                :value="currentInvitation.nickname"
            />
            <DisplayFieldComponent
                name-class="font-weight-bold"
                name="Inviting"
                :value="currentInvitation.email"
            />
            <DisplayFieldComponent
                name-class="font-weight-bold"
                name="Health Records Type"
            >
                <template #value>
                    <v-chip
                        v-for="dataSource in currentInvitation.dataSources"
                        :key="dataSource"
                        class="ma-1"
                        color="primary"
                        text-color="white"
                    >
                        {{ dataSource }}
                    </v-chip>
                </template>
            </DisplayFieldComponent>
            <DisplayFieldComponent
                name-class="font-weight-bold"
                name="Expiry Date"
                :value="`${currentInvitation.expiryDateRange} (${currentInvitation.expiryDate})`"
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
