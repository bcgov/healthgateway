<script setup lang="ts">
import { computed } from "vue";

import { ILogger } from "@/services/interfaces";
import { useRoute, useRouter } from "vue-router";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { useUserStore } from "@/stores/user";
import { useAuthStore } from "@/stores/auth";
import { useNotificationStore } from "@/stores/notification";
import LoadingComponent from "@/components/shared/LoadingComponent.vue";

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const route = useRoute();
const router = useRouter();
const userStore = useUserStore();
const authStore = useAuthStore();
const notificationStore = useNotificationStore();

const defaultPath = computed(() =>
    route.query.redirect ? route.query.redirect.toString() : "/home"
);

function signIn(redirectPath: string, idpHint?: string): Promise<void> {
    return authStore.signIn(redirectPath, idpHint);
}

function clearStorage(): void {
    authStore.clearStorage();
}

function retrieveEssentialData(): Promise<void> {
    return userStore.retrieveEssentialData();
}

function retrieveNotifications(): void {
    notificationStore.retrieve();
}

function redirect() {
    router
        .push({ path: defaultPath.value })
        .catch((error) => logger.warn(error.message));
}

signIn(defaultPath.value)
    .then(() => {
        logger.debug(`signIn for user: ${JSON.stringify(userStore.user)}`);

        // If the identity provider is invalid, the router will redirect to the appropriate error page.
        if (!userStore.isValidIdentityProvider) {
            redirect();
            return;
        }

        return retrieveEssentialData().then(() => {
            retrieveNotifications();
            redirect();
        });
    })
    .catch((error) => {
        logger.error(`LoginCallback Error: ${JSON.stringify(error)}`);

        clearStorage();
        router.push({
            path: "/login",
            query: { isRetry: "true" },
        });
    });
</script>

<template>
    <LoadingComponent is-loading text="Logging In..." />
</template>
