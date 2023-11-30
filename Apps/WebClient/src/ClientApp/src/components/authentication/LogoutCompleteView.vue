<script setup lang="ts">
import { computed, onUnmounted, ref } from "vue";
import { useRouter } from "vue-router";

import { Path } from "@/constants/path";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { ILogger } from "@/services/interfaces";
import { useConfigStore } from "@/stores/config";

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const router = useRouter();
const configStore = useConfigStore();

const redirectTimeoutId = ref<ReturnType<typeof setTimeout>>();

const redirectTimeoutLength = computed(() => {
    const configValue = Number(configStore.webConfig?.timeouts.logoutRedirect);
    if (isNaN(configValue) || configValue <= 0) {
        logger.warn(
            `Logout redirect timeout is invalid: ${configValue}, using default value of 10000ms`
        );
        return 10000;
    }
    return configValue;
});

onUnmounted(() => clearTimeout(redirectTimeoutId.value));

redirectTimeoutId.value = setTimeout(() => {
    router.push(Path.Root);
}, redirectTimeoutLength.value);
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
