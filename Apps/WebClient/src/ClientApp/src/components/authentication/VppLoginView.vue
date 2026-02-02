<script setup lang="ts">
import { computed } from "vue";

import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import {
    Action,
    Destination,
    ExternalUrl,
    InternalUrl,
    Origin,
    Text,
    Type,
} from "@/plugins/extensions";
import { ITrackingService } from "@/services/interfaces";
import { useConfigStore } from "@/stores/config";

const configStore = useConfigStore();
const webClientConfig = computed(() => configStore.webConfig);

const trackingService = container.get<ITrackingService>(
    SERVICE_IDENTIFIER.TrackingService
);

function openAccessMyHealth() {
    const url: string | undefined = webClientConfig.value.accessMyHealthUrl;
    if (!url) return;

    trackNavigationClick(
        Action.ExternalLink,
        Text.BackToAccessMyHealth,
        Destination.AccessMyHealth,
        ExternalUrl.AccessMyHealth
    );

    window.location.assign(url);
}

function openHgHome() {
    const baseUrl = window.location.origin;

    trackNavigationClick(
        Action.InternalLink,
        Text.HealthGatewayHome,
        Destination.HealthGateway,
        InternalUrl.Home
    );

    window.location.assign(`${baseUrl}/login`);
}

function trackNavigationClick(
    action: Action,
    text: string,
    destination: string,
    url: string
) {
    trackingService.trackEvent({
        action: action,
        text: text,
        origin: Origin.VPPLoginPage,
        destination: destination,
        type: Type.VPPLogin,
        url: url,
    });
}
</script>

<template>
    <div class="h-100 bg-grey-lighten-4 flex-grow-1 d-flex flex-column">
        <div class="bg-primary d-print-none">
            <router-link id="homeLink" to="/" aria-label="Return to home page">
                <img
                    class="img-fluid ma-3"
                    src="@/assets/images/gov/bcid-logo-rev-en.svg"
                    width="152"
                    alt="BC Mark"
                />
            </router-link>
        </div>
        <div class="d-flex flex-column">
            <div
                class="vpp-login-form bg-white rounded elevation-6 ma-2 ma-sm-4 py-6 py-sm-16 px-4 px-sm-16 align-self-center"
            >
                <p class="text-center">
                    <strong>You are opening Health Gateway</strong>
                </p>
                <p class="text-center mb-4">
                    As you are already signed into AccessMyHealth, you may go
                    directly to your Health Gateway records.
                </p>
                <p class="text-center">
                    For more information about Health Gateway, click
                    <a
                        :href="ExternalUrl.HealthGateway"
                        class="text-link"
                        @click.prevent="
                            trackNavigationClick(
                                Action.ExternalLink,
                                Text.HealthGatewayInfo,
                                Destination.HealthGateway,
                                ExternalUrl.HealthGateway
                            )
                        "
                        >here</a
                    >.
                </p>

                <div class="d-flex flex-column flex-sm-row ga-2 justify-center">
                    <HgButtonComponent
                        variant="link"
                        text="Go back to AccessMyHealth"
                        :uppercase="false"
                        data-testid="do-not-sign-in-button"
                        @click="openAccessMyHealth()"
                    />
                    <HgButtonComponent
                        variant="link"
                        text="Sign into Healthgateway"
                        :uppercase="false"
                        data-testid="sign-in-button"
                        @click="openHgHome()"
                    />
                </div>
            </div>
        </div>
    </div>
</template>

<style lang="scss" scoped>
.vpp-login-form {
    width: 100%;
    max-width: 600px;
}
</style>
