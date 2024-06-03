<script setup lang="ts">
import { useRoute, useRouter } from "vue-router";

import { Path } from "@/constants/path";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { ILogger } from "@/services/interfaces";
import { useConfigStore } from "@/stores/config";

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const route = useRoute();
const router = useRouter();
const configStore = useConfigStore();

function getRedirectTimeout() {
    const configValue = Number(configStore.webConfig?.timeouts.logoutRedirect);
    if (isNaN(configValue) || configValue <= 0) {
        logger.warn(
            `Logout redirect timeout is invalid: ${configValue}, using default value of 10000ms`
        );
        return 10000;
    }
    return configValue;
}

setTimeout(() => {
    if (route.path == Path.LogoutComplete) {
        router.push({ path: Path.Root });
    }
}, getRedirectTimeout());
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
