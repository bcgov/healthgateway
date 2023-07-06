<script setup lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import {
    faAddressCard,
    faUser,
    faUserSecret,
} from "@fortawesome/free-solid-svg-icons";
import { computed, ref } from "vue";
import { useRoute, useRouter } from "vue-router";

import HgButtonComponent from "@/components/shared/HgButtonComponent.vue";
import LoadingComponent from "@/components/shared/LoadingComponent.vue";
import { useAuthStore } from "@/stores/auth";
import { useConfigStore } from "@/stores/config";
import { useUserStore } from "@/stores/user";

library.add(faAddressCard, faUser, faUserSecret); // icons listed in config

interface Props {
    isRetry?: boolean;
}
const props = withDefaults(defineProps<Props>(), {
    isRetry: false,
});

const route = useRoute();
const router = useRouter();
const userStore = useUserStore();
const authStore = useAuthStore();
const configStore = useConfigStore();

const isLoading = ref(true);
const showError = ref(props.isRetry);

const oidcIsAuthenticated = computed(() => authStore.oidcIsAuthenticated);
const identityProviders = computed(() => configStore.identityProviders);
const defaultPath = computed(() =>
    route.query.redirect ? route.query.redirect.toString() : "/home"
);
const hasMultipleProviders = computed(() => identityProviders.value.length > 1);

function signIn(redirectPath: string, idpHint?: string): Promise<void> {
    return authStore.signIn(redirectPath, idpHint);
}

function redirect(): void {
    if (userStore.userIsRegistered) {
        router.push({ path: defaultPath.value });
    } else {
        router.push({ path: "/registration" });
    }
}

function signInAndRedirect(idpHint: string): void {
    signIn(defaultPath.value, idpHint).then(() => {
        // if this code is reached, the user was already signed in
        redirect();
    });
}

if (oidcIsAuthenticated.value) {
    redirect();
} else if (identityProviders.value.length === 1) {
    signInAndRedirect(identityProviders.value[0].hint);
} else {
    isLoading.value = false;
}
</script>

<template>
    <LoadingComponent :is-loading="isLoading" />
    <v-row align="center" justify="center" class="mb-5">
        <v-col md="5">
            <v-alert
                v-model="showError"
                closable
                close-label="Close"
                type="error"
                title="Error"
                text="An unexpected error occured while processing the request, please try again."
            />
        </v-col>
    </v-row>
    <v-card
        v-if="identityProviders && identityProviders.length > 0"
        id="loginPicker"
        class="shadow-lg bg-white mx-auto"
        style="max-width: 25rem"
        elevation="7"
    >
        <v-card-title>
            <h2 class="text-h5 font-weight-bold pt-3 text-center">Log In</h2>
        </v-card-title>
        <v-card-text v-if="hasMultipleProviders || isRetry" class="px-10 py-8">
            <div v-for="provider in identityProviders" :key="provider.id">
                <HgButtonComponent
                    :id="`${provider.id}Btn`"
                    :data-testid="`${provider.id}Btn`"
                    :disabled="provider.disabled"
                    variant="primary"
                    block
                    class="justify-start"
                    :prepend-icon="`${provider.icon}`"
                    :text="provider.name"
                    @click="signInAndRedirect(provider.hint)"
                >
                </HgButtonComponent>
                <div
                    v-if="
                        identityProviders.indexOf(provider) <
                        identityProviders.length - 1
                    "
                    class="text-center text-body-1 my-1"
                >
                    or
                </div>
            </div>
        </v-card-text>
        <v-card-text v-else class="pa-5 text-center text-body-1">
            Redirecting to <strong>{{ identityProviders[0].name }}</strong
            >...
        </v-card-text>
    </v-card>
    <div v-else>No login providers configured</div>
</template>
