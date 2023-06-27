<script setup lang="ts">
import PageErrorComponent from "@/components/shared/PageErrorComponent.vue";
</script>

<template>
    <PageErrorComponent title="Error retrieving user information">
        <p class="text-body-1">
            There may be an issue in our Client Registry. Please contact
            <a class="font-weight-bold" href="mailto:HealthGateway@gov.bc.ca"
                >HealthGateway@gov.bc.ca
            </a>
        </p>
    </PageErrorComponent>
</template>
