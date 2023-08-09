<script setup lang="ts">
import { computed } from "vue";

import { Path } from "@/constants/path";
import { useAppStore } from "@/stores/app";
import { useAuthStore } from "@/stores/auth";
import { useConfigStore } from "@/stores/config";
import { useUserStore } from "@/stores/user";

const authStore = useAuthStore();
const configStore = useConfigStore();
const userStore = useUserStore();
const appStore = useAppStore();

const isFooterShown = computed(
    () =>
        !configStore.isOffline &&
        (!authStore.oidcIsAuthenticated ||
            !userStore.isValidIdentityProvider ||
            userStore.userIsRegistered)
);
const isFooterFixed = computed(() => !appStore.isMobile);
</script>

<template>
    <v-footer
        v-if="isFooterShown"
        data-testid="footer"
        color="primary"
        :app="isFooterFixed"
    >
        <v-row class="pa-2">
            <v-col class="flex-grow-0" cols="12" md="auto">
                <a
                    :href="Path.TermsOfService"
                    data-testid="footer-terms-of-service-link"
                    class="text-white"
                >
                    Terms of Service
                </a>
            </v-col>
            <v-col class="flex-grow-0" cols="12" md="auto">
                <a
                    :href="Path.ReleaseNotes"
                    data-testid="footer-release-notes-link"
                    class="text-white"
                >
                    Release Notes
                </a>
            </v-col>
            <v-col class="flex-grow-0" cols="12" md="auto">
                <a
                    href="https://www2.gov.bc.ca/gov/content/health/managing-your-health/health-gateway"
                    target="_blank"
                    data-testid="footer-faq-link"
                    class="text-white"
                >
                    About Us
                </a>
            </v-col>
            <v-col class="flex-grow-0" cols="12" md="auto">
                <a
                    href="https://www2.gov.bc.ca/gov/content?id=FE8BA7F9F1F0416CB2D24CF71C4BAF80"
                    target="_blank"
                    data-testid="footer-faq-link"
                    class="text-white"
                >
                    FAQ
                </a>
            </v-col>
            <v-spacer />
            <v-col class="flex-grow-0" cols="12" md="auto">
                <a
                    href="mailto:healthgateway@gov.bc.ca"
                    data-testid="footer-email-link"
                    class="text-white"
                >
                    Email: HealthGateway@gov.bc.ca
                </a>
            </v-col>
        </v-row>
    </v-footer>
</template>
