<script setup lang="ts">
import { computed } from "vue";
import { useStore } from "vue-composition-wrapper";

const store = useStore();

interface Props {
    location?: string;
}
const props = withDefaults(defineProps<Props>(), {
    location: "page",
});

const warning = computed<string | undefined>(
    () => store.getters["errorBanner/tooManyRequestsWarning"]
);

const error = computed<string | undefined>(
    () => store.getters["errorBanner/tooManyRequestsError"]
);

function clearTooManyRequestsWarning(): void {
    store.dispatch("errorBanner/clearTooManyRequestsWarning");
}

function clearTooManyRequestsError(): void {
    store.dispatch("errorBanner/clearTooManyRequestsError");
}

const showWarning = computed(() => warning.value === props.location);
const showError = computed(() => error.value === props.location);
</script>

<template>
    <div>
        <b-alert
            :show="showWarning"
            data-testid="too-many-requests-warning"
            variant="warning"
            dismissible
            class="no-print"
            @dismissed="clearTooManyRequestsWarning"
        >
            We are unable to complete all actions because the site is too busy.
            Please try again later.
        </b-alert>
        <b-alert
            :show="showError"
            data-testid="too-many-requests-error"
            variant="danger"
            dismissible
            class="no-print"
            @dismissed="clearTooManyRequestsError"
        >
            Unable to complete action as the site is too busy. Please try again
            later.
        </b-alert>
    </div>
</template>
