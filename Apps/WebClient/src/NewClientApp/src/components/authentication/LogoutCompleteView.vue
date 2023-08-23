<script setup lang="ts">
import { useRoute, useRouter } from "vue-router";

import { useConfigStore } from "@/stores/config";

const route = useRoute();
const router = useRouter();
const configStore = useConfigStore();

setTimeout(() => {
    if (route.path == "/logoutComplete") {
        router.push({ path: "/" });
    }
}, Number(configStore.webConfig.timeouts.logoutRedirect));
</script>

<template>
    <div class="text-center">
        <h1
            data-testid="logout-complete-msg"
            class="mt-12 text-h4 text-primary font-weight-bold mb-6"
        >
            You signed out of your account
        </h1>
        <p class="text-body-1">
            It's a good idea to close all browser windows.
        </p>
    </div>
</template>
