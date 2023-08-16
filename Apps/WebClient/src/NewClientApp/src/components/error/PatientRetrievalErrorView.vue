<script setup lang="ts">
import { computed } from "vue";

import PageErrorComponent from "@/components/error/PageErrorComponent.vue";
import { AppErrorType } from "@/constants/errorType";
import { useAppStore } from "@/stores/app";
const appStore = useAppStore();
const isTooManyRequestsError = computed(
    () => appStore.appError === AppErrorType.TooManyRequests
);
</script>

<template>
    <PageErrorComponent
        data-testid="patient-retrieval-error"
        title="Error retrieving user information"
    >
        <p
            v-if="isTooManyRequestsError"
            class="text-body-1"
            data-testid="app-warning"
        >
            We are unable to retrieve the patient details from our Client
            Registry as the site is too busy. Please try again later.
        </p>
        <p v-else class="text-body-1">
            There may be an issue in our Client Registry. Please contact
            <a class="font-weight-bold" href="mailto:HealthGateway@gov.bc.ca"
                >HealthGateway@gov.bc.ca
            </a>
        </p>
    </PageErrorComponent>
</template>
