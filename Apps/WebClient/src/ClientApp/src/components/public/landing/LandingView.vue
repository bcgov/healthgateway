<script setup lang="ts">
import { computed, ref } from "vue";
import { useRouter } from "vue-router";
import { useDisplay } from "vuetify";

import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import HgIconButtonComponent from "@/components/common/HgIconButtonComponent.vue";
import TileComponent from "@/components/public/landing/TileComponent.vue";
import {
    getAccessLinks,
    getHealthServicesLinks,
} from "@/constants/accessLinks";
import { EntryType, entryTypeMap } from "@/constants/entryType";
import { ServiceName } from "@/constants/serviceName";
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
import ConfigUtil from "@/utility/configUtil";

enum PreviewDevice {
    laptop,
    tablet,
    smartphone,
}

const datasetEntryTypes: EntryType[] = [
    EntryType.Medication,
    EntryType.LabResult,
    EntryType.BcCancerScreening,
    EntryType.HealthVisit,
    EntryType.Immunization,
    EntryType.SpecialAuthorityRequest,
    EntryType.ClinicalDocument,
    EntryType.HospitalVisit,
    EntryType.DiagnosticImaging,
];

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const trackingService = container.get<ITrackingService>(
    SERVICE_IDENTIFIER.TrackingService
);

const configStore = useConfigStore();
const authStore = useAuthStore();
const router = useRouter();

const selectedPreviewDevice = ref(PreviewDevice.laptop);
const { mdAndUp, smAndDown, width, xs } = useDisplay();

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

const showLaptopTooltip = ref(false);
const showTabletTooltip = ref(false);
const showSmartphoneTooltip = ref(false);

const isOffline = computed(() => configStore.isOffline);
const oidcIsAuthenticated = computed(() => authStore.oidcIsAuthenticated);

const offlineMessage = computed(
    () => configStore.webConfig.offlineMode?.message ?? ""
);

const accessLinks = computed(() => getAccessLinks());
const healthServicesLinks = computed(() => getHealthServicesLinks());

const proofOfVaccinationTile = computed<InfoTile>(() => ({
    type: "ProofOfVaccination",
    icon: "check-circle",
    logoUri: new URL("@/assets/images/gov/canada-gov-logo.svg", import.meta.url)
        .href,
    name: "Proof of Vaccination",
    description: "View and download your proof of vaccination.",
    active: configStore.webConfig.featureToggleConfiguration.covid19
        .publicCovid19.showFederalProofOfVaccination,
}));
const organDonorRegistrationTile = computed<InfoTile>(() => ({
    type: "OrganDonorRegistration",
    icon: "check-circle",
    logoUri: new URL("@/assets/images/services/odr-logo.svg", import.meta.url)
        .href,
    name: "Organ Donor Registration",
    description:
        "Check your Organ Donor Registration status with BC Transplant.",
    active: ConfigUtil.isServiceEnabled(ServiceName.OrganDonorRegistration),
}));

const servicesTiles = computed(() =>
    [proofOfVaccinationTile.value, organDonorRegistrationTile.value].filter(
        (tile) => tile.active
    )
);
const shouldDisplayServices = computed(() => servicesTiles.value.length > 0);
const datasetTiles = computed(() =>
    datasetEntryTypes.map<InfoTile>(mapEntryTypeToTile)
);
const activeDatasetTiles = computed(() =>
    datasetTiles.value.filter((tile) => tile.active)
);
const shouldDisplayDatasets = computed(
    () => activeDatasetTiles.value.length > 0
);

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

function mapEntryTypeToTile(type: EntryType): InfoTile {
    const details = entryTypeMap.get(type);
    return {
        type,
        logoUri: details?.logoUri,
        icon: details?.icon ?? "",
        name: details?.name ?? "",
        description: details?.description ?? "",
        active: ConfigUtil.isDatasetEnabled(type) && details !== undefined,
    };
}

function selectPreviewDevice(previewDevice: PreviewDevice): void {
    selectedPreviewDevice.value = previewDevice;
    switch (previewDevice) {
        case PreviewDevice.laptop:
            showLaptopTooltip.value = false;
            break;
        case PreviewDevice.tablet:
            showTabletTooltip.value = false;
            break;
        case PreviewDevice.smartphone:
            showSmartphoneTooltip.value = false;
            break;
    }
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
        <div v-if="shouldDisplayDatasets && false" class="mt-6 mt-md-12">
            <h2
                class="text-primary text-h4 font-weight-bold mb-2 mb-md-4 mb-lg-6"
                data-testid="active-dataset-tiles-header"
            >
                What you can access
            </h2>
            <v-row>
                <v-col
                    v-for="tile in activeDatasetTiles"
                    :key="tile.name"
                    class="text-center px-6 px-md-12 pb-6 pb-md-12"
                    :data-testid="`active-dataset-tile-${tile.type}`"
                    cols="12"
                    md="6"
                    lg="4"
                >
                    <TileComponent :tile="tile" />
                </v-col>
            </v-row>
        </div>
        <div v-if="shouldDisplayServices && false" class="mt-6 mt-md-12">
            <h2
                class="text-primary text-h4 font-weight-bold mb-2 mb-md-4 mb-lg-6"
                data-testid="active-service-tiles-header"
            >
                Services you have access to
            </h2>
            <v-row>
                <v-col
                    v-for="tile in servicesTiles"
                    :key="tile.name"
                    class="text-center px-6 px-md-12 pb-6 pb-md-12"
                    :data-testid="`active-service-tile-${tile.type}`"
                    cols="12"
                    md="6"
                    lg="4"
                >
                    <TileComponent :tile="tile" />
                </v-col>
            </v-row>
        </div>
        <div v-if="false" class="mt-6 mt-md-12">
            <v-row no-gutters>
                <v-col cols="12" lg="6">
                    <h2 class="mb-4 text-primary text-h4 font-weight-bold">
                        What you can do
                    </h2>
                </v-col>
                <v-col cols="12" lg="6">
                    <p class="text-body-1 mb-0">
                        View your B.C. health records in one place, including
                        lab test results, medications, health visits,
                        immunizations and more.
                    </p>
                </v-col>
            </v-row>
            <div class="mt-6 mt-lg-12 text-center">
                <v-tooltip
                    v-model="showLaptopTooltip"
                    text="Show Laptop Preview"
                    location="top"
                >
                    <template #activator="{ props }">
                        <HgIconButtonComponent
                            icon="desktop"
                            data-testid="preview-device-button-laptop"
                            :class="{
                                'bg-grey-lighten-3':
                                    selectedPreviewDevice ===
                                    PreviewDevice.laptop,
                            }"
                            v-bind="props"
                            class="mx-4"
                            @click="selectPreviewDevice(PreviewDevice.laptop)"
                        />
                    </template>
                </v-tooltip>
                <v-tooltip
                    v-model="showTabletTooltip"
                    text="Show Tablet Preview"
                    location="top"
                >
                    <template #activator="{ props }">
                        <HgIconButtonComponent
                            icon="fas fa-tablet-screen-button"
                            v-bind="props"
                            data-testid="preview-device-button-tablet"
                            :class="{
                                'bg-grey-lighten-3':
                                    selectedPreviewDevice ===
                                    PreviewDevice.tablet,
                            }"
                            class="mx-4"
                            @click="selectPreviewDevice(PreviewDevice.tablet)"
                        />
                    </template>
                </v-tooltip>
                <v-tooltip
                    v-model="showSmartphoneTooltip"
                    text="Show Smartphone Preview"
                    location="top"
                >
                    <template #activator="{ props }">
                        <HgIconButtonComponent
                            icon="fas fa-mobile-screen-button"
                            v-bind="props"
                            data-testid="preview-device-button-smartphone"
                            :class="{
                                'bg-grey-lighten-3':
                                    selectedPreviewDevice ===
                                    PreviewDevice.smartphone,
                            }"
                            class="mx-4"
                            @click="
                                selectPreviewDevice(PreviewDevice.smartphone)
                            "
                        />
                    </template>
                </v-tooltip>
            </div>
            <div
                class="device-preview d-flex"
                :class="{ 'device-preview-md': mdAndUp }"
            >
                <v-img
                    v-show="selectedPreviewDevice === PreviewDevice.laptop"
                    src="@/assets/images/landing/preview-laptop.png"
                    data-testid="preview-image-laptop"
                    alt="Preview of Health Gateway on a Laptop"
                />
                <v-img
                    v-show="selectedPreviewDevice === PreviewDevice.tablet"
                    src="@/assets/images/landing/preview-tablet.png"
                    data-testid="preview-image-tablet"
                    alt="Preview of Health Gateway on a Tablet"
                />
                <v-img
                    v-show="selectedPreviewDevice === PreviewDevice.smartphone"
                    src="@/assets/images/landing/preview-smartphone.png"
                    data-testid="preview-image-smartphone"
                    alt="Preview of Health Gateway on a Smartphone"
                />
            </div>
            <v-row class="mt-4">
                <v-col cols="12" lg="4" class="pa-4 d-flex">
                    <v-card class="text-center flex-grow-1" variant="outlined">
                        <template #title>
                            <v-icon
                                class="mt-2 text-primary"
                                icon="fas fa-clock-rotate-left"
                                size="large"
                            />
                            <h4
                                class="mt-2 text-h6 font-weight-bold text-primary"
                            >
                                Stay up to date
                            </h4>
                        </template>
                        <template #text>
                            <p class="text-body-1">
                                View your health information in chronological
                                order, so you can easily see what’s new.
                            </p>
                        </template>
                    </v-card>
                </v-col>
                <v-col cols="12" lg="4" class="pa-4 d-flex">
                    <v-card class="text-center flex-grow-1" variant="outlined">
                        <template #title>
                            <v-icon
                                class="mt-2 text-primary"
                                icon="fas fa-cloud-arrow-down"
                                size="large"
                            />
                            <h4
                                class="mt-2 text-h6 font-weight-bold text-primary"
                            >
                                Manage your information
                            </h4>
                        </template>
                        <template #text>
                            <p class="text-body-1">
                                Download your health records, organize, and
                                print them. Make your own notes on records to
                                track health events.
                            </p>
                        </template>
                    </v-card>
                </v-col>
                <v-col cols="12" lg="4" class="pa-4 d-flex">
                    <v-card class="text-center flex-grow-1" variant="outlined">
                        <template #title>
                            <v-icon
                                class="mt-2 text-primary"
                                icon="fas fa-magnifying-glass"
                                size="large"
                            />
                            <h4
                                class="mt-2 text-h6 font-weight-bold text-primary"
                            >
                                Find what you need
                            </h4>
                        </template>
                        <template #text>
                            <p class="text-body-1">
                                Filter by category or search your health records
                                to find what you need.
                            </p>
                        </template>
                    </v-card>
                </v-col>
            </v-row>
        </div>
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
