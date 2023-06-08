<script setup lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import {
    faAddressCard,
    faUser,
    faUserSecret,
} from "@fortawesome/free-solid-svg-icons";
import { computed, ref } from "vue";
import { useRoute, useRouter, useStore } from "vue-composition-wrapper";

import LoadingComponent from "@/components/LoadingComponent.vue";
import { IdentityProviderConfiguration } from "@/models/configData";

library.add(faAddressCard, faUser, faUserSecret); // icons listed in config

interface Props {
    isRetry?: boolean;
}
withDefaults(defineProps<Props>(), {
    isRetry: false,
});

const route = useRoute();
const router = useRouter();
const store = useStore();

const isLoading = ref(true);

const oidcIsAuthenticated = computed<boolean>(() => {
    return store.getters["auth/oidcIsAuthenticated"];
});
const userIsRegistered = computed<boolean>(() => {
    return store.getters["user/userIsRegistered"];
});
const identityProviders = computed<IdentityProviderConfiguration[]>(() => {
    return store.getters["config/identityProviders"];
});

const defaultPath = computed(() =>
    route.value.query.redirect ? route.value.query.redirect.toString() : "/home"
);
const hasMultipleProviders = computed(() => {
    return identityProviders.value.length > 1;
});

function signIn(redirectPath: string, idpHint?: string): Promise<void> {
    return store.dispatch("auth/signIn", { redirectPath, idpHint });
}

function redirect(): void {
    if (userIsRegistered.value) {
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
    <div class="container my-5">
        <LoadingComponent :is-loading="isLoading" />
        <b-alert
            style="max-width: 25rem"
            :show="isRetry"
            dismissible
            variant="danger"
        >
            <h4>Error</h4>
            <span
                >An unexpected error occured while processing the request,
                please try again.</span
            >
        </b-alert>
        <b-card
            v-if="identityProviders && identityProviders.length > 0"
            id="loginPicker"
            class="shadow-lg bg-white mx-auto"
            style="max-width: 25rem"
            align="center"
        >
            <h3 slot="header">Log In</h3>
            <b-card-body v-if="hasMultipleProviders || isRetry">
                <div v-for="provider in identityProviders" :key="provider.id">
                    <hg-button
                        :id="`${provider.id}Btn`"
                        :data-testid="`${provider.id}Btn`"
                        :disabled="provider.disabled"
                        variant="primary"
                        block
                        @click="signInAndRedirect(provider.hint)"
                    >
                        <b-row>
                            <b-col class="col-2">
                                <hg-icon
                                    :icon="`${provider.icon}`"
                                    size="medium"
                                />
                            </b-col>
                            <b-col class="text-left">
                                <span>{{ provider.name }}</span>
                            </b-col>
                        </b-row>
                    </hg-button>
                    <div
                        v-if="
                            identityProviders.indexOf(provider) <
                            identityProviders.length - 1
                        "
                    >
                        or
                    </div>
                </div>
            </b-card-body>
            <b-card-body v-else>
                <span
                    >Redirecting to
                    <strong>{{ identityProviders[0].name }}</strong
                    >...</span
                >
            </b-card-body>
        </b-card>
        <div v-else>No login providers configured</div>
    </div>
</template>
