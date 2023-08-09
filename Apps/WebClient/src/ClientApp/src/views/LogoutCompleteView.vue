<script setup lang="ts">
import { computed } from "vue";
import { useRoute, useRouter, useStore } from "vue-composition-wrapper";

import type { WebClientConfiguration } from "@/models/configData";

const route = useRoute();
const router = useRouter();
const store = useStore();

const config = computed<WebClientConfiguration>(
    () => store.getters["config/webClient"]
);

setTimeout(() => {
    if (route.value.path == "/logoutComplete") {
        router.push({ path: "/" });
    }
}, Number(config.value.timeouts.logoutRedirect));
</script>

<template>
    <b-row class="justify-content-center h-100 pt-5">
        <b-col md="6">
            <div class="shadow-lg p-3 mb-5 bg-white rounded border">
                <h3 data-testid="logout-complete-msg">
                    <strong>You signed out of your account</strong>
                </h3>
                <p>It's a good idea to close all browser windows.</p>
            </div>
        </b-col>
    </b-row>
</template>
