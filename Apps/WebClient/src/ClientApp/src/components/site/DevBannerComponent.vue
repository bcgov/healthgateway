<script setup lang="ts">
import { computed, ref } from "vue";

import Process, { EnvironmentType } from "@/constants/process";
import { useAppStore } from "@/stores/app";

const appStore = useAppStore();

const host = ref(window.location.hostname.toLocaleUpperCase());

const isProduction = computed(
    () =>
        Process.NODE_ENV == EnvironmentType.production &&
        (host.value.startsWith("HEALTHGATEWAY") ||
            host.value.startsWith("WWW.HEALTHGATEWAY"))
);
</script>

<template>
    <v-system-bar
        v-if="!isProduction"
        class="d-flex justify-center bg-accent small d-print-none"
    >
        <p>
            <span v-if="!appStore.isMobile">Non-production environment: </span
            ><strong>{{ host }}</strong>
        </p>
    </v-system-bar>
</template>
