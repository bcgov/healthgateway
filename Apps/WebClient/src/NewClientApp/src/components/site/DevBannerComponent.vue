<script setup lang="ts">
import { computed, ref } from "vue";

import Process, { EnvironmentType } from "@/constants/process";

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
            Non-production environment:
            <strong>{{ host }}</strong>
        </p>
    </v-system-bar>
</template>
