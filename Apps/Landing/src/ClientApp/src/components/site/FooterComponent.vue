<script setup lang="ts">
import { computed } from "vue";

import { Path } from "@/constants/path";
import { useConfigStore } from "@/stores/config";
import { useLayoutStore } from "@/stores/layout";

const configStore = useConfigStore();
const layoutStore = useLayoutStore();

const isFooterShown = computed(() => !configStore.isOffline);
const isFooterFixed = computed(() => !layoutStore.isMobile);
</script>

<template>
    <v-footer
        v-if="isFooterShown"
        data-testid="footer"
        color="white"
        class="justify-center"
        elevation="4"
        :app="isFooterFixed"
    >
        <v-row class="pa-2">
            <v-col class="flex-grow-0" cols="12" md="auto">
                <router-link
                    :to="Path.TermsOfService"
                    data-testid="footer-terms-of-service-link"
                    class="text-secondary"
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
                    class="text-secondary"
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
                    class="text-secondary"
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
                    class="text-secondary"
                >
                    FAQ
                </a>
            </v-col>
            <v-spacer />
            <v-col class="flex-grow-0" cols="12" md="auto">
                <a
                    href="mailto:healthgateway@gov.bc.ca"
                    data-testid="footer-email-link"
                    class="text-secondary"
                >
                    Email: HealthGateway@gov.bc.ca
                </a>
            </v-col>
        </v-row>
    </v-footer>
</template>

<style lang="scss" scoped>
.v-footer > .v-row {
    max-width: var(--site-max-width);
}
</style>
