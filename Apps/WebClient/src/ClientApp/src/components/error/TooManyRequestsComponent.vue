<script setup lang="ts">
import { computed } from "vue";

import { useErrorStore } from "@/stores/error";

interface Props {
    location?: string;
}
const props = withDefaults(defineProps<Props>(), {
    location: "page",
});

const errorStore = useErrorStore();

const showWarning = computed(
    () => errorStore.tooManyRequestsWarning == props.location
);

const showError = computed(
    () => errorStore.tooManyRequestsError == props.location
);

function clearTooManyRequestsWarning(): void {
    errorStore.clearTooManyRequestsWarning();
}

function clearTooManyRequestsError(): void {
    errorStore.clearTooManyRequestsError();
}
</script>

<template>
    <v-alert
        v-if="showError"
        data-testid="too-many-requests-error"
        type="error"
        closable
        class="d-print-none mb-4"
        variant="outlined"
        border
        @click:close="clearTooManyRequestsError"
    >
        Unable to complete action as the site is too busy. Please try again
        later.
    </v-alert>
    <v-alert
        v-else-if="showWarning"
        data-testid="too-many-requests-warning"
        type="warning"
        icon="circle-exclamation"
        closable
        class="d-print-none mb-4"
        variant="outlined"
        border
        @click:close="clearTooManyRequestsWarning"
    >
        We are unable to complete all actions because the site is too busy.
        Please try again later.
    </v-alert>
</template>
