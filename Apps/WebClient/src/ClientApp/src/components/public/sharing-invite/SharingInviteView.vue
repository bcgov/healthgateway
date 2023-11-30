<script setup lang="ts">
import { computed } from "vue";
import { RouteLocationRaw, useRouter } from "vue-router";

import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import { useAuthStore } from "@/stores/auth";
import { useUserStore } from "@/stores/user";

interface Props {
    inviteId: string;
}
const props = defineProps<Props>();

const router = useRouter();
const authStore = useAuthStore();
const userStore = useUserStore();

const oidcIsAuthenticated = computed(() => authStore.oidcIsAuthenticated);
const invitePath = computed(() => `/sharing?invite=${props.inviteId}`);
const loginRoute = computed<RouteLocationRaw>(() => ({
    path: "/login",
    query: { redirect: invitePath.value },
}));
const registrationRoute = computed<RouteLocationRaw>(() => ({
    path: "/registration",
    query: { redirect: invitePath.value },
}));

if (userStore.userIsRegistered) {
    router.push(invitePath.value);
}
</script>

<template>
    <v-container>
        <v-row justify="start" align="start">
            <v-col lg="5">
                <h1 class="mb-6 text-primary text-h4 font-weight-bold">
                    Welcome to Health Gateway
                </h1>
                <p class="mb-6 text-body-1">
                    You've been invited to view health records on Health
                    Gateway. Please log in or register to access this shared
                    information and join us in managing health more effectively.
                </p>
                <div v-if="!oidcIsAuthenticated" class="mb-4">
                    <HgButtonComponent
                        class="btn-auth-landing"
                        text="Log in with BC Services Card"
                        variant="primary"
                        :to="loginRoute"
                        data-testid="login-button"
                    />
                    <div class="mt-4 d-flex align-center">
                        <span class="text-body-1 mr-2">Need an account?</span>
                        <HgButtonComponent
                            text="Register"
                            variant="link"
                            :to="registrationRoute"
                            data-testid="register-button"
                        />
                    </div>
                </div>
            </v-col>
            <v-col
                xs="false"
                lg="7"
                class="d-none d-lg-block text-center col-7"
            >
                <v-img
                    src="@/assets/images/landing/landing-top.png"
                    alt="Health Gateway Preview"
                />
            </v-col>
        </v-row>
    </v-container>
</template>
