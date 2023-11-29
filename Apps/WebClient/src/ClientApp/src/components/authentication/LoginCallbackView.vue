<script setup lang="ts">
import { useRouter } from "vue-router";

import LoadingComponent from "@/components/common/LoadingComponent.vue";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { ILogger } from "@/services/interfaces";
import { useAuthStore } from "@/stores/auth";
import { useNotificationStore } from "@/stores/notification";
import { useUserStore } from "@/stores/user";

interface Props {
    redirectPath: string;
}
const props = defineProps<Props>();

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const router = useRouter();
const userStore = useUserStore();
const authStore = useAuthStore();
const notificationStore = useNotificationStore();

function signIn(idpHint?: string): Promise<void> {
    return authStore.signIn(props.redirectPath, idpHint);
}

function clearStorage(): void {
    authStore.clearStorage();
}

function retrieveNotifications(): void {
    notificationStore.retrieve();
}

function redirect() {
    router
        .push({ path: props.redirectPath })
        .catch((error) => logger.warn(error.message));
}

signIn()
    .then(() => {
        logger.debug(`signIn for user: ${JSON.stringify(userStore.user)}`);

        // If the identity provider is invalid, the router will redirect to the appropriate error page.
        if (!userStore.isValidIdentityProvider) {
            redirect();
            return;
        }

        return userStore.retrieveEssentialData().then(() => {
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
