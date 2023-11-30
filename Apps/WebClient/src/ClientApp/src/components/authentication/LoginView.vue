<script setup lang="ts">
import { computed, ref } from "vue";
import { useRouter } from "vue-router";

import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import LoadingComponent from "@/components/common/LoadingComponent.vue";
import { useAuthStore } from "@/stores/auth";
import { useConfigStore } from "@/stores/config";
import { useUserStore } from "@/stores/user";

interface Props {
    isRetry: boolean;
    redirectPath: string;
}
const props = defineProps<Props>();

const router = useRouter();
const userStore = useUserStore();
const authStore = useAuthStore();
const configStore = useConfigStore();

const isLoading = ref(true);
const showError = ref(props.isRetry);

const oidcIsAuthenticated = computed(() => authStore.oidcIsAuthenticated);
const identityProviders = computed(() => configStore.identityProviders);
const hasMultipleProviders = computed(() => identityProviders.value.length > 1);

function signIn(idpHint?: string): Promise<void> {
    return authStore.signIn(props.redirectPath, idpHint);
}

function redirect(): void {
    if (userStore.userIsRegistered) {
        router.push(props.redirectPath);
    } else {
        router.push({ path: "/registration" });
    }
}

function signInAndRedirect(idpHint: string): void {
    signIn(idpHint).then(() => {
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
                variant="outlined"
                border
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
