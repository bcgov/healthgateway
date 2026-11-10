<script setup lang="ts">
import DependentManagementView from "@/components/private/dependent/DependentManagementView.vue";
import LegacyDependentsView from "@/components/private/dependent/legacy/LegacyDependentsView.vue";
import { useConfigStore } from "@/stores/config";

const configStore = useConfigStore();
</script>

<template>
    <DependentManagementView
        v-if="
            configStore.webConfig.featureToggleConfiguration.dependents
                .timelineEnabled
        "
    />
    <LegacyDependentsView v-else />
</template>
