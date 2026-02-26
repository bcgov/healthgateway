<script setup lang="ts">
import { computed } from "vue";

import HgAlertComponent from "@/components/common/HgAlertComponent.vue";
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
    <HgAlertComponent
        v-if="showError"
        data-testid="too-many-requests-error"
        type="error"
        closable
        variant="outlined"
        :center-content="true"
        @click:close="clearTooManyRequestsError"
    >
        Unable to complete action as the site is too busy. Please try again
        later.
    </HgAlertComponent>
    <HgAlertComponent
        v-else-if="showWarning"
        data-testid="too-many-requests-warning"
        type="warning"
        closable
        variant="outlined"
        :center-content="true"
        @click:close="clearTooManyRequestsWarning"
    >
        We are unable to complete all actions because the site is too busy.
        Please try again later.
    </HgAlertComponent>
</template>
