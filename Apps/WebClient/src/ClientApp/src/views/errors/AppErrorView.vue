<script setup lang="ts">
import { computed } from "vue";
import { useStore } from "vue-composition-wrapper";

import { AppErrorType } from "@/constants/errorType";

const store = useStore();

const isTooManyRequests = computed<boolean>(() => {
    return store.getters["appError"] === AppErrorType.TooManyRequests;
});
</script>

<template>
    <div
        class="flex-grow-1 d-flex align-items-center justify-content-center p-3 p-md-4"
    >
        <b-alert
            v-if="isTooManyRequests"
            show
            variant="warning"
            data-testid="app-warning"
        >
            We are unable to complete all actions because the site is too busy.
            Please try again later.
        </b-alert>
        <b-alert v-else show variant="danger" data-testid="app-error">
            Unable to load application. Please try refreshing the page or come
            back later.
        </b-alert>
    </div>
</template>
