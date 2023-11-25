<script setup lang="ts">
import { computed } from "vue";

import { useDelegationStore } from "@/stores/delegation";

const delegationStore = useDelegationStore();

const currentDelegation = computed(() => delegationStore.delegationWizardState);
</script>

<template>
    <v-row v-if="currentDelegation" data-testid="delegation-sharing-code-step">
        <v-col cols="12" justify="center">
            <v-icon
                class="d-block mb-2"
                icon="check-circle"
                color="success"
                size="48"
            />
            <h5 class="text-body-1 text-center">
                An invite has been set to <b>{{ currentDelegation.email }}</b>
            </h5>
        </v-col>
        <v-col>
            <p class="text-body-1 mb-4">
                To complete the sharing process, you need to share the code
                below with the recipient.
            </p>
            <v-row justify="space-evenly" align-content="center" class="mb-4">
                <v-col cols="6" sm="4" class="d-flex align-center">
                    <h5 class="text-h5 pa-2 font-weight-bold text-center w-100">
                        Sharing Code
                    </h5>
                </v-col>
                <v-col cols="6" sm="4" class="d-flex align-center">
                    <h5
                        class="text-h5 pa-2 font-weight-bold border text-center w-100"
                        data-testid="sharing-code"
                    >
                        {{ currentDelegation.sharingCode }}
                    </h5>
                </v-col>
            </v-row>
            <p class="text-body-1 text-red mb-4">
                The code is <b>not included</b> in the invite for additional
                security. You have to share this separately.
            </p>
            <p class="text-body-1">
                You can always change acccess to your records by going to the
                sharing page in your Health Gateway account.
            </p>
        </v-col>
    </v-row>
</template>
