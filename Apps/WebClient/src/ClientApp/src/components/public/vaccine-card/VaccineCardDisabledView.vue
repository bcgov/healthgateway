<script setup lang="ts">
import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import {
    Action,
    Destination,
    InternalUrl,
    Origin,
    Text,
} from "@/plugins/extensions";
import { ITrackingService } from "@/services/interfaces";

const trackingService = container.get<ITrackingService>(
    SERVICE_IDENTIFIER.TrackingService
);

function trackVaccineInfoLink(href: string) {
    trackingService.trackEvent({
        action: Action.ExternalLink,
        text: Text.Covid19VaccineInformation,
        destination: Destination.MohCovid19,
        origin: Origin.VaccineCard,
    });
    window.open(href, "_blank", "noopener");
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
                class="vaccine-card-form bg-white rounded elevation-6 ma-2 ma-sm-4 py-6 py-sm-16 px-4 px-sm-16 align-self-center"
            >
                <p>
                    Federal Proof of Vaccination is no longer provided by Health
                    Gateway.
                </p>
                <p class="mb-16">
                    For more information, see the Ministry of Health
                    <a
                        href="https://www2.gov.bc.ca/gov/content/health/managing-your-health/immunizations/covid-19-immunization#record"
                        class="text-link"
                        @click.prevent="
                            trackVaccineInfoLink(
                                'https://www2.gov.bc.ca/gov/content/health/managing-your-health/immunizations/covid-19-immunization#record'
                            )
                        "
                        >COVID‑19 immunizations</a
                    >
                    page.
                </p>
                <div class="text-center">
                    <p>Already a Health Gateway user?</p>
                    <p>Log in and download your immunization record.</p>
                    <HgButtonComponent
                        aria-label="BC Services Card Login"
                        data-testid="btnLogin"
                        variant="primary"
                        text="Log in with BC Services Card"
                        to="/login"
                        @click="
                            trackingService.trackEvent({
                                action: Action.ButtonClick,
                                text: Text.LoginVaccineCard,
                                destination: Destination.Login,
                                url: InternalUrl.Login,
                            })
                        "
                    />
                </div>
            </div>
        </div>
    </div>
</template>

<style lang="scss" scoped>
.vaccine-card-form {
    max-width: 600px;
}
</style>
