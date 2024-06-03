<script setup lang="ts">
import HgButtonComponent from "@/components/common/HgButtonComponent.vue";

interface Props {
    oidcIsAuthenticated: boolean;
}
defineProps<Props>();
</script>

<template>
    <div class="text-center py-12" data-testid="registration-success-banner">
        <v-icon icon="circle-check" color="success" size="48" class="mb-4" />
        <h4
            data-testid="registration-success-banner-title"
            class="text-h4 mb-4"
        >
            Success!
        </h4>
        <p class="text-body-1 mt-2">
            Your kit has been registered to your profile.
        </p>
        <div class="text-center">
            <HgButtonComponent
                v-if="oidcIsAuthenticated"
                id="logoutBtn"
                aria-label="Logout"
                data-testid="logoutBtn"
                variant="secondary"
                to="/logout"
                text="Log Out"
            />
            <HgButtonComponent
                v-else
                id="btn-continue"
                aria-label="Continue"
                data-testid="btn-continue"
                variant="secondary"
                to="/"
                text="Back to Home"
            />
        </div>
    </div>
</template>
