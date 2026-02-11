<script setup lang="ts">
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

const trackingService = container.get<ITrackingService>(
    SERVICE_IDENTIFIER.TrackingService
);

function openAccessMyHealth() {
    trackNavigationClick(
        Action.ExternalLink,
        Text.BackToAccessMyHealth,
        Destination.AccessMyHealth,
        ExternalUrl.AccessMyHealth
    );

    window.location.assign(ExternalUrl.AccessMyHealth);
}

function openHealthGatewayInfo(): void {
    trackNavigationClick(
        Action.ExternalLink,
        Text.HealthGatewayInfo,
        Destination.HealthGateway,
        ExternalUrl.HealthGateway
    );
    window.location.assign(ExternalUrl.HealthGateway);
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
                    <strong>Do you want to open Health Gateway?</strong>
                </p>
                <p class="text-center mb-4">
                    You are currently signed into AccessMyHealth. If you wish to
                    continue, you will open Health Gateway in this window.
                </p>
                <p class="text-center">
                    Health Gateway may contain more health records and
                    information.
                    <a
                        :href="ExternalUrl.HealthGateway"
                        class="text-link"
                        data-testid="click-hgw-link"
                        @click.prevent="openHealthGatewayInfo"
                        >Learn more.</a
                    >
                </p>
                <div class="d-flex flex-column flex-sm-row ga-2 justify-center">
                    <HgButtonComponent
                        variant="link"
                        text="Cancel"
                        :uppercase="false"
                        data-testid="go-back-button"
                        :data-url="ExternalUrl.AccessMyHealth"
                        @click="openAccessMyHealth()"
                    />
                    <HgButtonComponent
                        variant="link"
                        text="Continue to Health Gateway"
                        :uppercase="false"
                        data-testid="sign-in-button"
                        @click="openHgHome()"
                    />
                </div>
                <p class="text-center mt-8 text-body-2 font-italic">
                    Please remember to sign out and close both windows when you
                    are done.
                </p>
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
