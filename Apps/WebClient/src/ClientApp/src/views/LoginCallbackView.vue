<script setup lang="ts">
import { computed } from "vue";
import { useRoute, useRouter, useStore } from "vue-composition-wrapper";

import User from "@/models/user";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger } from "@/services/interfaces";

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const route = useRoute();
const router = useRouter();
const store = useStore();

const user = computed<User>(() => store.getters["user/user"]);
const isValidIdentityProvider = computed<boolean>(
    () => store.getters["user/isValidIdentityProvider"]
);

const defaultPath = computed(() =>
    route.value.query.redirect ? route.value.query.redirect.toString() : "/home"
);

function signIn(redirectPath: string, idpHint?: string): Promise<void> {
    return store.dispatch("auth/signIn", { redirectPath, idpHint });
}

function clearStorage(): void {
    store.dispatch("auth/clearStorage");
}

function retrieveEssentialData(): Promise<void> {
    return store.dispatch("user/retrieveEssentialData");
}

function retrieveNotifications(): Promise<Notification[]> {
    return store.dispatch("notification/retrieve");
}

function redirect() {
    router
        .push({ path: defaultPath.value })
        .catch((error) => logger.warn(error.message));
}

signIn(defaultPath.value)
    .then(() => {
        logger.debug(`signIn for user: ${JSON.stringify(user.value)}`);

        // If the identity provider is invalid, the router will redirect to the appropriate error page.
        if (!isValidIdentityProvider.value) {
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
    <div>Waiting...</div>
</template>
