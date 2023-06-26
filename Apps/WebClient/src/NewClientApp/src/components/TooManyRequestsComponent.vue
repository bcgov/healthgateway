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

const warning = computed(() => errorStore.tooManyRequestsWarning);

const error = computed(() => errorStore.tooManyRequestsError);

function clearTooManyRequestsWarning(): void {
    errorStore.clearTooManyRequestsWarning();
}

function clearTooManyRequestsError(): void {
    errorStore.clearTooManyRequestsError();
}

const showWarning = computed(() => warning.value === props.location);
const showError = computed(() => error.value === props.location);
</script>

<template>
    <div>
        <v-alert
            :show="showWarning"
            data-testid="too-many-requests-warning"
            color="warning"
            dismissible
            class="no-print"
            @dismissed="clearTooManyRequestsWarning"
        >
            We are unable to complete all actions because the site is too busy.
            Please try again later.
        </v-alert>
        <v-alert
            :show="showError"
            data-testid="too-many-requests-error"
            color="error"
            dismissible
            class="no-print"
            @dismissed="clearTooManyRequestsError"
        >
            Unable to complete action as the site is too busy. Please try again
            later.
        </v-alert>
    </div>
</template>
