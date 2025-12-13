<script setup lang="ts">
import VaccineCardDisabledView from "@/components/public/vaccine-card/VaccineCardDisabledView.vue";
import VaccineCardView from "@/components/public/vaccine-card/VaccineCardView.vue";
import { useConfigStore } from "@/stores/config";

const configStore = useConfigStore();
</script>

<template>
    <VaccineCardView
        v-if="
            configStore.webConfig.featureToggleConfiguration.covid19
                .publicCovid19.vaccineCardEnabled
        "
    />
    <VaccineCardDisabledView v-else />
</template>
