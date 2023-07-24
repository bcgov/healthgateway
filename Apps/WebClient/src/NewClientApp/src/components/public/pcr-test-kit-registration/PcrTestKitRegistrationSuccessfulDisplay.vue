<script setup lang="ts">
import HgButtonComponent from "@/components/common/HgButtonComponent.vue";

interface Props {
    oidcIsAuthenticated: boolean;
}

defineProps<Props>();
</script>
<template>
    <div class="text-center py-12" data-testid="registration-success-banner">
        <v-icon
            icon="circle-check"
            color="success"
            size="48"
            class="mb-4"
        ></v-icon>
        <h4
            data-testid="registration-success-banner-title"
            class="text-h4 mb-4"
        >
            Success!
        </h4>
        <p class="text-body-1">Your kit has been registered to your profile.</p>
        <div v-if="oidcIsAuthenticated" class="pt-2">
            <router-link to="/logout">
                <HgButtonComponent
                    id="logoutBtn"
                    aria-label="Logout"
                    data-testid="logoutBtn"
                    variant="secondary"
                >
                    <span>Log Out</span>
                </HgButtonComponent>
            </router-link>
        </div>
        <div v-else class="pt-2">
            <router-link to="/">
                <HgButtonComponent
                    id="btn-continue"
                    aria-label="Continue"
                    data-testid="btn-continue"
                    variant="secondary"
                >
                    Back to Home
                </HgButtonComponent>
            </router-link>
        </div>
    </div>
</template>
