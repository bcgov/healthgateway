<script setup lang="ts">
import PageErrorComponent from "@/components/error/PageErrorComponent.vue";
</script>

<template>
    <PageErrorComponent title="401" subtitle="Unauthorized">
        <p class="text-body-1">You do not have permission to view this page.</p>
    </PageErrorComponent>
</template>
