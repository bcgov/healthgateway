<script setup lang="ts">
import { computed } from "vue";
import { useRouter } from "vue-router";
import { useDisplay } from "vuetify";

import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import TileComponent from "@/components/public/landing/TileComponent.vue";
import {
    getAccessLinks,
    getHealthServicesLinks,
} from "@/constants/accessLinks";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { InfoTile } from "@/models/infoTile";
import {
    Action,
    Destination,
    ExternalUrl,
    InternalUrl,
    LandingAccessLinkDestination,
    LandingAccessLinkText,
    LandingAccessLinkType,
    Origin,
    Text,
    Type,
} from "@/plugins/extensions";
import { ILogger, ITrackingService } from "@/services/interfaces";
import { useAuthStore } from "@/stores/auth";
import { useConfigStore } from "@/stores/config";

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const trackingService = container.get<ITrackingService>(
    SERVICE_IDENTIFIER.TrackingService
);

const configStore = useConfigStore();
const authStore = useAuthStore();
const router = useRouter();

const { smAndDown, width, xs } = useDisplay();

// Narrow-phone detection (e.g., Galaxy Z Fold width)
const isVeryNarrowPhone = computed(() => width.value <= 360);

// Tight-phone threshold for managed health cards
const isTightPhone = computed(() => width.value <= 375);

/*
  Carousel heights depend not only on screen width, but also on the
  length of the AccessLink name/description content inside each card.
  These values were tuned to prevent text clipping across all major phones.
*/
const accessLinkCarouselHeight = computed(() =>
    isVeryNarrowPhone.value ? 360 : 340
);
const healthServicesCarouselHeight = computed(() =>
    isTightPhone.value ? 400 : 370
);

const isOffline = computed(() => configStore.isOffline);
const oidcIsAuthenticated = computed(() => authStore.oidcIsAuthenticated);

const offlineMessage = computed(
    () => configStore.webConfig.offlineMode?.message ?? ""
);

const accessLinks = computed(() => getAccessLinks());
const healthServicesLinks = computed(() => getHealthServicesLinks());

const shouldDisplayAccessLinks = computed(() => accessLinks.value.length > 0);
const shouldDisplayHealthServicesLinks = computed(
    () => healthServicesLinks.value.length > 0
);

function handleLoginClick(): void {
    trackingService.trackEvent({
        action: Action.ButtonClick,
        text: Text.LoginBCSC,
        type: Type.Login,
        url: InternalUrl.Login,
    });
    router.push({ path: "/login" });
}

function handleRegisterClick(): void {
    trackingService.trackEvent({
        action: Action.InternalLink,
        text: Text.Register,
        origin: Origin.Landing,
        destination: Destination.Registration,
        type: Type.Landing,
        url: InternalUrl.Registration,
    });
    router.push({ path: "/registration" });
}

function openExternalLink(tile: InfoTile, action: Action) {
    if (!tile.link) return;
    window.open(tile.link, "_blank", "noopener");

    const landingAccessLinkType = tile.type as LandingAccessLinkType;

    if (!(landingAccessLinkType in LandingAccessLinkText)) {
        logger.warn(
            `openExternalLink: unsupported landing access link type: ${tile.type}`
        );
        return;
    }

    trackingService.trackEvent({
        action: action,
        text: LandingAccessLinkText[landingAccessLinkType],
        origin: Origin.Landing,
        destination: LandingAccessLinkDestination[landingAccessLinkType],
        type: Type.Landing,
        url: tile.link,
    });
}
</script>

<template>
    <div v-if="isOffline" class="pa-4 text-center">
        <h1 class="text-primary text-h4 font-weight-bold mb-6">
            The site is offline for maintenance
        </h1>
        <p data-testid="offlineMessage" class="text-body-1">
            {{ offlineMessage }}
        </p>
    </div>
    <v-container v-else>
        <v-row justify="start" align="start">
            <v-col lg="5">
                <h1 class="mb-6 text-primary text-h4 font-weight-bold">
                    Access your health information online
                </h1>
                <p class="mb-6 text-body-1">
                    Health Gateway provides secure and convenient access to your
                    health records in British Columbia
                </p>
                <div v-if="!oidcIsAuthenticated" class="mb-4">
                    <HgButtonComponent
                        id="btnLogin"
                        class="btn-auth-landing"
                        text="Log in with BC Services Card"
                        variant="primary"
                        data-testid="btnLogin"
                        :uppercase="false"
                        @click="handleLoginClick"
                    />
                    <div class="mt-4 d-flex align-center">
                        <span class="text-body-1 mr-2">Need an account?</span>
                        <HgButtonComponent
                            id="btnStart"
                            text="Register"
                            variant="link"
                            data-testid="btnStart"
                            :uppercase="false"
                            @click="handleRegisterClick"
                        />
                    </div>
                </div>
            </v-col>
            <v-col
                xs="false"
                lg="7"
                class="d-none d-lg-block text-center col-7 pa-0"
            >
                <v-img
                    src="@/assets/images/landing/landing-top.png"
                    alt="Health Gateway Preview"
                    data-testid="landing-top-image-id"
                />
            </v-col>
        </v-row>
        <div v-if="shouldDisplayAccessLinks" class="mt-4 mt-md-8">
            <h2
                class="text-primary text-h4 font-weight-bold mb-2 mb-md-4 mb-lg-5"
                data-testid="access-links-header"
            >
                What you can access
            </h2>
            <p class="landing-bc-black text-body-1 mb-4">
                There are many features that help you view, manage and stay
                informed about your health.
            </p>
            <v-carousel
                v-if="xs"
                :height="accessLinkCarouselHeight"
                class="w-100"
                color="primary"
                :show-arrows="false"
                hide-delimiter-background
                data-testid="mobile-access-card-carousel"
            >
                <v-carousel-item v-for="tile in accessLinks" :key="tile.name">
                    <v-card
                        variant="outlined"
                        rounded="md"
                        :class="[
                            'h-100 d-flex flex-column text-start pa-5 pa-lg-4 border-sm',
                            smAndDown ? 'pb-12' : 'pb-6',
                        ]"
                        :data-testid="`mobile-access-card-${tile.type}`"
                        @click="openExternalLink(tile, Action.CardClick)"
                    >
                        <div
                            class="flex-grow-1"
                            :style="{ minHeight: xs ? '120px' : '70px' }"
                        >
                            <TileComponent :tile="tile" />
                        </div>
                        <v-card-actions
                            class="justify-center no-card-press-zone"
                            :class="xs ? 'pb-1' : 'pb-0'"
                        >
                            <HgButtonComponent
                                text="Read more"
                                variant="secondary"
                                :uppercase="false"
                                :aria-label="`Read more about ${tile.name}`"
                                :data-testid="`mobile-read-more-button-${tile.type}`"
                                @click.stop="
                                    openExternalLink(tile, Action.ExternalLink)
                                "
                            />
                        </v-card-actions>
                    </v-card>
                </v-carousel-item>
            </v-carousel>
            <v-row v-else align="stretch" class="mb-6 mb-md-8">
                <v-col
                    v-for="tile in accessLinks"
                    :key="tile.name"
                    cols="12"
                    md="6"
                    lg="3"
                    class="px-2 px-md-4"
                >
                    <v-card
                        variant="outlined"
                        rounded="md"
                        class="h-100 d-flex flex-column text-start pa-5 pa-lg-4 border-sm"
                        :data-testid="`access-card-${tile.type}`"
                        @click="openExternalLink(tile, Action.CardClick)"
                    >
                        <div class="flex-grow-1">
                            <TileComponent :tile="tile" />
                        </div>
                        <v-card-actions
                            class="justify-center pt-4 no-card-press-zone"
                        >
                            <HgButtonComponent
                                text="Read more"
                                variant="secondary"
                                :uppercase="false"
                                :aria-label="`Read more about ${tile.name}`"
                                :data-testid="`read-more-button-${tile.type}`"
                                @click.stop="
                                    openExternalLink(tile, Action.ExternalLink)
                                "
                            />
                        </v-card-actions>
                    </v-card>
                </v-col>
            </v-row>
        </div>
        <div class="mt-4 mt-md-8">
            <v-sheet
                color="sectionBackground"
                variant="tonal"
                class="pa-6 pa-lg-8"
            >
                <v-row align="start" no-gutters>
                    <v-col cols="12" md="7" class="pr-md-8">
                        <h2 class="text-h4 font-weight-bold text-primary mb-2">
                            Find your health records
                        </h2>
                        <p class="text-body-1 mb-4">
                            Health Gateway helps bring your records together in
                            one place. It connects to many record sources, but
                            not all.
                        </p>
                        <a
                            :href="ExternalUrl.YourHealthInformation"
                            target="_blank"
                            rel="noopener"
                            class="text-link"
                            @click="
                                trackingService.trackEvent({
                                    action: Action.ExternalLink,
                                    text: Text.FindYourHealthRecords,
                                    origin: Origin.Landing,
                                    destination: Destination.SupportGuide,
                                    type: Type.Landing,
                                    url: ExternalUrl.YourHealthInformation,
                                })
                            "
                        >
                            Learn more about other health records in B.C.
                        </a>
                    </v-col>
                    <v-col
                        cols="12"
                        md="5"
                        class="d-none d-md-flex justify-center align-center"
                    >
                        <v-img
                            src="@/assets/images/landing/finding-records.png"
                            alt="Illustration for finding records"
                            max-width="300"
                            contain
                        />
                    </v-col>
                </v-row>
            </v-sheet>
        </div>
        <div v-if="shouldDisplayHealthServicesLinks" class="mt-6 mt-md-8">
            <h2
                class="text-primary text-h4 font-weight-bold mb-2 mb-md-4 mb-lg-5"
                data-testid="health-services-links-header"
            >
                Explore other trusted B.C. health services
            </h2>
            <v-carousel
                v-if="xs"
                :height="healthServicesCarouselHeight"
                class="w-100"
                color="primary"
                :show-arrows="false"
                hide-delimiter-background
                data-testid="mobile-health-services-card-carousel"
            >
                <v-carousel-item
                    v-for="tile in healthServicesLinks"
                    :key="tile.name"
                >
                    <v-card
                        variant="outlined"
                        rounded="md"
                        :class="[
                            'h-100 d-flex flex-column text-start pa-5 pa-lg-4 border-sm',
                            smAndDown ? 'pb-12' : 'pb-6',
                        ]"
                        :data-testid="`mobile-health-services-card-${tile.type}`"
                        @click="openExternalLink(tile, Action.CardClick)"
                    >
                        <div
                            class="flex-grow-1"
                            :style="{ minHeight: xs ? '120px' : '70px' }"
                        >
                            <TileComponent :tile="tile" />
                        </div>
                        <v-card-actions
                            class="justify-center no-card-press-zone"
                            :class="xs ? 'pb-1' : 'pb-0'"
                        >
                            <HgButtonComponent
                                text="Read more"
                                variant="secondary"
                                :uppercase="false"
                                :aria-label="`Read more about ${tile.name}`"
                                :data-testid="`mobile-read-more-button-${tile.type}`"
                                @click.stop="
                                    openExternalLink(tile, Action.ExternalLink)
                                "
                            />
                        </v-card-actions>
                    </v-card>
                </v-carousel-item>
            </v-carousel>
            <v-row v-else align="stretch" class="mb-6 mb-md-8">
                <v-col
                    v-for="tile in healthServicesLinks"
                    :key="tile.name"
                    cols="12"
                    md="6"
                    lg="4"
                    class="px-2 px-md-4"
                >
                    <v-card
                        variant="outlined"
                        rounded="md"
                        class="h-100 d-flex flex-column text-start pa-5 pa-lg-4 border-sm"
                        :data-testid="`managed-health-card-${tile.type}`"
                        @click="openExternalLink(tile, Action.CardClick)"
                    >
                        <div class="flex-grow-1">
                            <TileComponent :tile="tile" />
                        </div>
                        <v-card-actions
                            class="justify-center pt-4 no-card-press-zone"
                        >
                            <HgButtonComponent
                                text="Read more"
                                variant="secondary"
                                :uppercase="false"
                                :aria-label="`Read more about ${tile.name}`"
                                :data-testid="`read-more-button-${tile.type}`"
                                @click.stop="
                                    openExternalLink(tile, Action.ExternalLink)
                                "
                            />
                        </v-card-actions>
                    </v-card>
                </v-col>
            </v-row>
        </div>
    </v-container>
</template>

<style lang="scss" scoped>
/* 
  LandingView override:
  - Recolor TileComponent titles to BC Black (#313132)
    instead of the Vuetify theme primary.
*/
:deep(.tile-title) {
    color: #313132 !important;
}

/* Paragraph override: Landing-only text requiring BC Black */
.landing-bc-black {
    color: #313132 !important;
}

.device-preview {
    max-height: 212px;

    &.device-preview-md {
        max-height: 412px;
    }
}

:deep(.v-card:has(.no-card-press-zone :active) .v-card__overlay),
:deep(.v-card:has(.no-card-press-zone :active) .v-card__underlay),
:deep(.v-card:has(.no-card-press-zone :focus-within) .v-card__overlay),
:deep(.v-card:has(.no-card-press-zone :focus-within) .v-card__underlay) {
    opacity: 0 !important;
}
</style>
