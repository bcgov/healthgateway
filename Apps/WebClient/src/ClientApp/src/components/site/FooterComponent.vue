<script setup lang="ts">
import { computed } from "vue";

import { Path } from "@/constants/path";
import { useAuthStore } from "@/stores/auth";
import { useConfigStore } from "@/stores/config";
import { useLayoutStore } from "@/stores/layout";
import { useUserStore } from "@/stores/user";

const authStore = useAuthStore();
const configStore = useConfigStore();
const userStore = useUserStore();
const layoutStore = useLayoutStore();

const isFooterShown = computed(
    () =>
        !configStore.isOffline &&
        (!authStore.oidcIsAuthenticated ||
            !userStore.isValidIdentityProvider ||
            userStore.userIsRegistered)
);
</script>

<template>
    <v-footer
        v-if="isFooterShown"
        data-testid="footer"
        color="white"
        app
        :class="{ 'non-fixed-footer': layoutStore.isMobile }"
    >
        <v-row class="pa-2">
            <v-col class="flex-grow-0" cols="12" md="auto">
                <router-link
                    :to="Path.TermsOfService"
                    data-testid="footer-terms-of-service-link"
                    class="text-primary"
                >
                    Terms of Service
                </router-link>
            </v-col>
            <v-col class="flex-grow-0" cols="12" md="auto">
                <a
                    href="https://www2.gov.bc.ca/gov/content?id=D5206BCE6B3F46279080D9CC5B977D3E"
                    target="_blank"
                    rel="noopener"
                    data-testid="footer-release-notes-link"
                    class="text-primary"
                >
                    Release Notes
                </a>
            </v-col>
            <v-col class="flex-grow-0" cols="12" md="auto">
                <a
                    href="https://www2.gov.bc.ca/gov/content/health/managing-your-health/health-gateway"
                    target="_blank"
                    rel="noopener"
                    data-testid="footer-faq-link"
                    class="text-primary"
                >
                    About Us
                </a>
            </v-col>
            <v-col class="flex-grow-0" cols="12" md="auto">
                <a
                    href="https://www2.gov.bc.ca/gov/content?id=FE8BA7F9F1F0416CB2D24CF71C4BAF80"
                    target="_blank"
                    rel="noopener"
                    data-testid="footer-faq-link"
                    class="text-primary"
                >
                    FAQ
                </a>
            </v-col>
            <v-spacer />
            <v-col class="flex-grow-0" cols="12" md="auto">
                <a
                    href="mailto:healthgateway@gov.bc.ca"
                    data-testid="footer-email-link"
                    class="text-primary"
                >
                    Email: HealthGateway@gov.bc.ca
                </a>
            </v-col>
        </v-row>
    </v-footer>
</template>

<style lang="scss" scoped>
.v-footer {
    // Light grey line
    border-top: 1px solid #dcdcdc;

    // Ensure the footer appears above overlapping components like drawers or content
    z-index: 100;

    // Prevent transparency issues — ensures consistent white background across viewports
    background-color: white;

    // Sticky positioning keeps the footer visible at the bottom when not using the Vuetify layout system
    position: sticky;
    bottom: 0;
}

// On mobile, let it flow with content
.non-fixed-footer {
    position: static !important;
}
</style>
