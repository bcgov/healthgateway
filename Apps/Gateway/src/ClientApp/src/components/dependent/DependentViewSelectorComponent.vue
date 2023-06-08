<script setup lang="ts">
import { computed } from "vue";
import { useStore } from "vue-composition-wrapper";

import type { WebClientConfiguration } from "@/models/configData";
import DependentManagementView from "@/views/DependentManagementView.vue";
import DependentsView from "@/views/DependentsView.vue";

const store = useStore();

const config = computed<WebClientConfiguration>(
    () => store.getters["config/webClient"]
);

const timelineEnabled = computed<boolean>(
    () => config.value.featureToggleConfiguration.dependents.timelineEnabled
);
</script>
<template>
    <DependentManagementView v-if="timelineEnabled" />
    <DependentsView v-else />
</template>
