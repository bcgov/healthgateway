<script setup lang="ts">
import { computed } from "vue";

import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import { useAuthStore } from "@/stores/auth";
import { useConfigStore } from "@/stores/config";
import { useUserStore } from "@/stores/user";

const authStore = useAuthStore();
const configStore = useConfigStore();
const userStore = useUserStore();

const isFooterShown = computed<boolean>(
    () =>
        !configStore.isOffline &&
        (!authStore.oidcIsAuthenticated ||
            !userStore.isValidIdentityProvider ||
            userStore.userIsRegistered)
);
</script>

<template>
    <v-footer v-show="isFooterShown" data-testid="footer" color="primary" app>
        <v-row>
            <v-col class="flex-grow-0" cols="12" md="auto">
                <HgButtonComponent
                    variant="secondary"
                    inverse
                    data-testid="footer-terms-of-service-link"
                    :to="Path.TermsOfService"
                    text="Terms of Service"
                />
            </v-col>
            <v-col class="flex-grow-0" cols="12" md="auto">
                <HgButtonComponent
                    variant="secondary"
                    inverse
                    data-testid="footer-release-notes-link"
                    to="/release-notes"
                    text="Release Notes"
                />
            </v-col>
            <v-col class="flex-grow-0" cols="12" md="auto">
                <HgButtonComponent
                    variant="secondary"
                    inverse
                    data-testid="footer-about-us-link"
                    href="https://www2.gov.bc.ca/gov/content/health/managing-your-health/health-gateway"
                    target="_blank"
                    text="About Us"
                />
            </v-col>
            <v-col class="flex-grow-0" cols="12" md="auto">
                <HgButtonComponent
                    variant="secondary"
                    inverse
                    data-testid="footer-faq-link"
                    href="https://www2.gov.bc.ca/gov/content?id=FE8BA7F9F1F0416CB2D24CF71C4BAF80"
                    target="_blank"
                    text="FAQ"
                />
            </v-col>
            <v-spacer />
            <v-col class="flex-grow-0" cols="12" md="auto">
                <HgButtonComponent
                    variant="secondary"
                    inverse
                    data-testid="footer-email-link"
                    href="mailto:healthgateway@gov.bc.ca"
                    text="Email: HealthGateway@gov.bc.ca"
                />
            </v-col>
        </v-row>
    </v-footer>
</template>
