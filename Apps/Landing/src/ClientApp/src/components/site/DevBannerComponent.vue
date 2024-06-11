<script setup lang="ts">
import { computed, ref } from "vue";

import Process, { EnvironmentType } from "@/constants/process";
import { useLayoutStore } from "@/stores/layout";

const layoutStore = useLayoutStore();

const host = ref(window.location.hostname.toLocaleUpperCase());

const isProduction = computed(
    () =>
        Process.NODE_ENV == EnvironmentType.production &&
        (host.value.startsWith("HEALTHGATEWAY") ||
            host.value.startsWith("WWW.HEALTHGATEWAY") ||
            host.value.startsWith("CLASSIC.HEALTHGATEWAY") ||
            host.value.startsWith("LEGACY.HEALTHGATEWAY"))
);
</script>

<template>
    <v-system-bar
        v-if="!isProduction"
        class="d-flex justify-center bg-accent small d-print-none"
    >
        <p>
            <span v-if="!layoutStore.isMobile"
                >Non-production environment: </span
            ><strong>{{ host }}</strong>
        </p>
    </v-system-bar>
</template>
