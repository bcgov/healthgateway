<script setup lang="ts">
import { computed } from "vue";

import { Path } from "@/constants/path";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import {
    Action,
    Destination,
    EmailUrl,
    ExternalUrl,
    Origin,
    Text,
    Type,
} from "@/plugins/extensions";
import { ITrackingService } from "@/services/interfaces";
import { useAuthStore } from "@/stores/auth";
import { useConfigStore } from "@/stores/config";
import { useLayoutStore } from "@/stores/layout";
import { useUserStore } from "@/stores/user";

const authStore = useAuthStore();
const configStore = useConfigStore();
const userStore = useUserStore();
const layoutStore = useLayoutStore();

const trackingService = container.get<ITrackingService>(
    SERVICE_IDENTIFIER.TrackingService
);

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
                <a
                    href="https://www2.gov.bc.ca/gov/content?id=FE8BA7F9F1F0416CB2D24CF71C4BAF80"
                    target="_blank"
                    rel="noopener"
                    data-testid="footer-faq-link"
                    class="text-primary"
                    @click="
                        trackingService.trackEvent({
                            action: Action.ExternalLink,
                            text: Text.SupportGuide,
                            origin: Origin.Footer,
                            destination: Destination.SupportGuide,
                            type: Type.Footer,
                            url: ExternalUrl.SupportGuide,
                        })
                    "
                >
                    Support Guide
                </a>
            </v-col>
            <v-col class="flex-grow-0" cols="12" md="auto">
                <a
                    href="https://www2.gov.bc.ca/gov/content?id=D5206BCE6B3F46279080D9CC5B977D3E"
                    target="_blank"
                    rel="noopener"
                    data-testid="footer-release-notes-link"
                    class="text-primary"
                    @click="
                        trackingService.trackEvent({
                            action: Action.ExternalLink,
                            text: Text.ReleaseNotes,
                            origin: Origin.Footer,
                            destination: Destination.SupportGuide,
                            type: Type.Footer,
                            url: ExternalUrl.ReleaseNotes,
                        })
                    "
                >
                    Release Notes
                </a>
            </v-col>
            <v-col class="flex-grow-0" cols="12" md="auto">
                <router-link
                    :to="Path.TermsOfService"
                    data-testid="footer-terms-of-service-link"
                    class="text-primary"
                    @click="
                        trackingService.trackEvent({
                            action: Action.ExternalLink,
                            text: Text.TermsOfService,
                            origin: Origin.Footer,
                            destination: Destination.TermsOfService,
                            type: Type.Footer,
                            url: ExternalUrl.TermsOfService,
                        })
                    "
                >
                    Terms of Service
                </router-link>
            </v-col>
            <v-col class="flex-grow-0" cols="12" md="auto">
                <a
                    href="https://www2.gov.bc.ca/gov/content/health/managing-your-health/health-gateway"
                    target="_blank"
                    rel="noopener"
                    data-testid="footer-faq-link"
                    class="text-primary"
                    @click="
                        trackingService.trackEvent({
                            action: Action.ExternalLink,
                            text: Text.AboutUs,
                            origin: Origin.Footer,
                            destination: Destination.SupportGuide,
                            type: Type.Footer,
                            url: ExternalUrl.AbuutUs,
                        })
                    "
                >
                    About Us
                </a>
            </v-col>
            <v-spacer />
            <v-col class="flex-grow-0" cols="12" md="auto">
                <a
                    href="mailto:healthgateway@gov.bc.ca"
                    data-testid="footer-email-link"
                    class="text-primary"
                    @click="
                        trackingService.trackEvent({
                            action: Action.Email,
                            text: Text.EmailHealthGateway,
                            origin: Origin.Footer,
                            destination: Destination.SupportEmail,
                            type: Type.Footer,
                            url: EmailUrl.HealthGatewayEmail,
                        })
                    "
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
