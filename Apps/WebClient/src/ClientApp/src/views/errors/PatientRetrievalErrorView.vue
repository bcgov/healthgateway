<script setup lang="ts">
import PageErrorComponent from "@/components/PageErrorComponent.vue";
</script>

<template>
    <PageErrorComponent title="Error retrieving user information">
        <p>
            There may be an issue in our Client Registry. Please contact
            <strong
                ><a href="mailto:HealthGateway@gov.bc.ca"
                    >HealthGateway@gov.bc.ca</a
                ></strong
            >
        </p>
    </PageErrorComponent>
</template>
