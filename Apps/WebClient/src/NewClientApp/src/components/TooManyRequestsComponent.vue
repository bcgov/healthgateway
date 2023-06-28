<script setup lang="ts">
import { useErrorStore } from "@/stores/error";
import { computed } from "vue";

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
    <div>
        <v-alert
            :model-value="showWarning"
            data-testid="too-many-requests-warning"
            color="warning"
            dismissible
            class="no-print my-1"
            @dismissed="clearTooManyRequestsWarning"
        >
            We are unable to complete all actions because the site is too busy.
            Please try again later.
        </v-alert>
        <v-alert
            :model-value="showError"
            data-testid="too-many-requests-error"
            color="error"
            dismissible
            class="no-print my-1"
            @dismissed="clearTooManyRequestsError"
        >
            Unable to complete action as the site is too busy. Please try again
            later.
        </v-alert>
    </div>
</template>
