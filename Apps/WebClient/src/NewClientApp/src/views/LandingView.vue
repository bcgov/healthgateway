<script setup lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import {
    faCheckCircle,
    faClipboardCheck,
    faClockRotateLeft,
    faCloudArrowDown,
    faDesktop,
    faEdit,
    faFileMedical,
    faFileWaveform,
    faHouseMedical,
    faMagnifyingGlass,
    faMicroscope,
    faMobileScreenButton,
    faPills,
    faStethoscope,
    faSyringe,
    faTabletScreenButton,
    faUserGroup,
    faVial,
    faXRay,
} from "@fortawesome/free-solid-svg-icons";
import { computed, ref } from "vue";

import { EntryType, entryTypeMap } from "@/constants/entryType";
import ConfigUtil from "@/utility/configUtil";
import { useConfigStore } from "@/stores/config";
import { useAuthStore } from "@/stores/auth";
import HgButtonComponent from "@/components/shared/HgButtonComponent.vue";
import HgIconButtonComponent from "@/components/shared/HgIconButtonComponent.vue";
import { useDisplay } from "vuetify";

library.add(
    faCheckCircle,
    faClipboardCheck,
    faClockRotateLeft,
    faCloudArrowDown,
    faDesktop,
    faEdit,
    faFileMedical,
    faFileWaveform,
    faHouseMedical,
    faMagnifyingGlass,
    faMicroscope,
    faMobileScreenButton,
    faPills,
    faStethoscope,
    faSyringe,
    faTabletScreenButton,
    faUserGroup,
    faVial,
    faXRay
);

interface Tile {
    type: string;
    name: string;
    description: string;
    icon: string;
    active: boolean;
}

enum PreviewDevice {
    laptop = "laptop",
    tablet = "tablet",
    smartphone = "smartphone",
}

const entryTypes: EntryType[] = [
    EntryType.Medication,
    EntryType.LabResult,
    EntryType.Covid19TestResult,
    EntryType.HealthVisit,
    EntryType.Immunization,
    EntryType.SpecialAuthorityRequest,
    EntryType.ClinicalDocument,
    EntryType.HospitalVisit,
    EntryType.DiagnosticImaging,
];

const configStore = useConfigStore();
const authStore = useAuthStore();

const selectedPreviewDevice = ref(PreviewDevice.laptop);
const { smAndDown } = useDisplay();

const showLaptopTooltip = ref(false);
const showTabletTooltip = ref(false);
const showSmartphoneTooltip = ref(false);

const isOffline = computed<boolean>(() => configStore.isOffline);
const oidcIsAuthenticated = computed<boolean>(
    () => authStore.oidcIsAuthenticated
);

const offlineMessage = computed(
    () => configStore.webConfig.offlineMode?.message ?? ""
);
const proofOfVaccinationTile = computed<Tile>(() => ({
    type: "ProofOfVaccination",
    icon: "check-circle",
    name: "Proof of Vaccination",
    description: "View and download your proof of vaccination",
    active: configStore.webConfig.featureToggleConfiguration.covid19
        .publicCovid19.showFederalProofOfVaccination,
}));
const tiles = computed(() => {
    // Get core tiles from entry type constants
    const tiles = entryTypes.map<Tile>((type) => {
        const details = entryTypeMap.get(type);
        return {
            type,
            icon: details?.icon ?? "",
            name: details?.name ?? "",
            description: details?.description ?? "",
            active: ConfigUtil.isDatasetEnabled(type) && details !== undefined,
        };
    });

    // Add Proof of Vaccination tile
    tiles.splice(2, 0, proofOfVaccinationTile.value);

    return tiles;
});
const activeTiles = computed(() => tiles.value.filter((tile) => tile.active));

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
</script>

<template>
    <v-container class="landing-container" v-if="isOffline">
        <v-row align="center" justify="center" class="pt-2 pb-5">
            <v-col class="cols-12 text-center">
                <hr class="py-4" />
                <v-row class="py-2">
                    <v-col class="text-h3">
                        The site is offline for maintenance
                    </v-col>
                </v-row>
                <v-row class="py-3">
                    <v-col data-testid="offlineMessage" class="text-subtitle-1">
                        {{ offlineMessage }}
                    </v-col>
                </v-row>
                <v-row class="pt-5">
                    <v-col>
                        <hr class="pt-5" />
                    </v-col>
                </v-row>
            </v-col>
        </v-row>
    </v-container>
    <v-container class="landing-container" v-else>
        <v-row justify="start" align="start" class="pt-10">
            <v-col lg="5">
                <h1 class="mb-4 text-primary text-h4 font-weight-bold">
                    Access your health information online
                </h1>
                <p class="mb-4">
                    Health Gateway provides secure and convenient access to your
                    health records in British Columbia
                </p>
                <div v-if="!oidcIsAuthenticated" class="mb-3">
                    <router-link to="/login">
                        <HgButtonComponent
                            id="btnLogin"
                            data-testid="btnLogin"
                            variant="primary"
                            class="btn-auth-landing"
                        >
                            <span>Log in with BC Services Card</span>
                        </HgButtonComponent>
                    </router-link>
                    <div class="mt-3">
                        <span class="mr-2">Need an account?</span>
                        <router-link
                            id="btnStart"
                            data-testid="btnStart"
                            to="registration"
                        >
                            Register
                        </router-link>
                    </div>
                </div>
            </v-col>
            <v-col
                xs="false"
                lg="7"
                class="d-none d-lg-block text-center col-7"
            >
                <v-img
                    class="img-fluid"
                    src="@/assets/images/landing/landing-top.png"
                    alt="Health Gateway Preview"
                    data-testid="landing-top-image-id"
                />
            </v-col>
        </v-row>
        <div class="mt-4 mt-md-5">
            <h2 class="mb-4 text-primary text-h4 font-weight-bold">
                What you can access
            </h2>
            <v-row align="center" justify="center">
                <v-col
                    v-for="tile in activeTiles"
                    :key="tile.name"
                    class="text-center px-4 px-md-5 pb-4 pb-md-5"
                    :data-testid="`active-tile-${tile.type}`"
                    cols="12"
                    md="6"
                    lg="4"
                >
                    <v-icon
                        :icon="tile.icon"
                        color="primary"
                        size="x-large"
                        class="ma-3"
                    />
                    <h3 class="text-primary mb-2">{{ tile.name }}</h3>
                    <p class="mb-0">{{ tile.description }}</p>
                </v-col>
            </v-row>
        </div>
        <div class="mt-4 mt-md-5">
            <v-row>
                <v-col cols="12" lg="6">
                    <h2 class="mb-4 text-primary text-h4 font-weight-bold">
                        What you can do
                    </h2>
                </v-col>
                <v-col cols="12" lg="6">
                    <p class="mb-0">
                        View your B.C. health records in one place, including
                        lab test results, medications, health visits,
                        immunizations and more.
                    </p>
                </v-col>
            </v-row>
            <v-row>
                <v-col class="mt-4 mt-lg-5 text-center">
                    <v-tooltip
                        text="Show Laptop Preview"
                        location="top"
                        no-click-animation
                        close-on-content-click
                    >
                        <template #activator="{ props }">
                            <HgIconButtonComponent
                                icon="fas fa-desktop"
                                v-bind="props"
                                :disabled="
                                    selectedPreviewDevice ===
                                    PreviewDevice.laptop
                                "
                                @click="
                                    selectPreviewDevice(PreviewDevice.laptop)
                                "
                                class="mx-3"
                            ></HgIconButtonComponent>
                        </template>
                    </v-tooltip>
                    <v-tooltip
                        text="Show Tablet Preview"
                        location="top"
                        no-click-animation
                        close-on-content-click
                    >
                        <template #activator="{ props }">
                            <HgIconButtonComponent
                                icon="fas fa-tablet-screen-button"
                                v-bind="props"
                                :disabled="
                                    selectedPreviewDevice ===
                                    PreviewDevice.tablet
                                "
                                @click="
                                    selectPreviewDevice(PreviewDevice.tablet)
                                "
                                class="mx-3"
                            ></HgIconButtonComponent>
                        </template>
                    </v-tooltip>
                    <v-tooltip
                        text="Show Smartphone Preview"
                        location="top"
                        no-click-animation
                        close-on-content-click
                    >
                        <template #activator="{ props }">
                            <HgIconButtonComponent
                                icon="fas fa-mobile-screen-button"
                                v-bind="props"
                                :disabled="
                                    selectedPreviewDevice ===
                                    PreviewDevice.smartphone
                                "
                                @click="
                                    selectPreviewDevice(
                                        PreviewDevice.smartphone
                                    )
                                "
                                class="mx-3"
                            ></HgIconButtonComponent>
                        </template>
                    </v-tooltip>
                </v-col>
            </v-row>
            <v-row>
                <v-col class="device-preview" :class="smAndDown ? 'sm' : ''">
                    <v-img
                        v-show="selectedPreviewDevice === 'laptop'"
                        src="@/assets/images/landing/preview-laptop.png"
                        data-testid="preview-image-laptop"
                        alt="Preview of Health Gateway on a Laptop"
                    />
                    <v-img
                        v-show="selectedPreviewDevice === 'tablet'"
                        src="@/assets/images/landing/preview-tablet.png"
                        data-testid="preview-image-tablet"
                        alt="Preview of Health Gateway on a Tablet"
                    />
                    <v-img
                        v-show="selectedPreviewDevice === 'smartphone'"
                        src="@/assets/images/landing/preview-smartphone.png"
                        data-testid="preview-image-smartphone"
                        alt="Preview of Health Gateway on a Smartphone"
                    />
                </v-col>
            </v-row>
            <v-row>
                <v-col cols="12" lg="4" class="p-3">
                    <v-card class="h-100 text-center pa-3" variant="outlined">
                        <v-card-title class="text-primary" variant="outlined">
                            <v-icon
                                icon="fas fa-clock-rotate-left"
                                size="large"
                            />
                            <h4 class="mt-2">Stay up to date</h4>
                        </v-card-title>
                        <v-card-text class="text-body-1">
                            View your health information in a list or calendar
                            view, so you can easily see what’s new.
                        </v-card-text>
                    </v-card>
                </v-col>
                <v-col cols="12" lg="4" class="p-3">
                    <v-card class="h-100 text-center pa-3" variant="outlined">
                        <v-card-title class="text-primary">
                            <v-icon
                                icon="fas fa-cloud-arrow-down"
                                size="large"
                            />
                            <h4 class="mt-2">Manage your information</h4>
                        </v-card-title>
                        <v-card-text class="text-body-1">
                            Download your health records, organize, and print
                            them. Make your own notes on records to track health
                            events.
                        </v-card-text>
                    </v-card>
                </v-col>
                <v-col cols="12" lg="4" class="p-3">
                    <v-card class="h-100 text-center pa-3" variant="outlined">
                        <v-card-title class="text-primary">
                            <v-icon
                                icon="fas fa-magnifying-glass"
                                size="large"
                            />
                            <h4 class="mt-2">Find what you need</h4>
                        </v-card-title>
                        <v-card-text class="text-body-1">
                            Add a quick link to the records you use the most.
                            Filter or search to find what you need.
                        </v-card-text>
                    </v-card>
                </v-col>
            </v-row>
        </div>
        <v-row align="center" justify="center">
            <v-col cols="12" lg="6">
                <div class="text-center">
                    <img
                        class="img-fluid"
                        src="@/assets/images/landing/mobile-app.png"
                        alt="Health Gateway Splash Page App"
                        data-testid="spash-page-app"
                    />
                </div>
            </v-col>
            <v-col cols="12" lg="6" class="text-center mb-4 mb-md-5">
                <h2 class="mb-4 text-primary text-h4 font-weight-bold">
                    Try the mobile app.
                </h2>
                <p class="mb-4">
                    You can download it for free to your phone, tablet or iPad.
                </p>
                <a
                    href="https://play.google.com/store/apps/details?id=ca.bc.gov.myhealth&hl=en_CA&gl=US"
                    rel="noopener"
                    target="_blank"
                >
                    <img
                        class="img-fluid mr-3"
                        src="@/assets/images/landing/google-play-badge.png"
                        alt="Go to Google Play"
                    />
                </a>
                <a
                    href="https://apps.apple.com/ca/app/health-gateway/id1590009068"
                    rel="noopener"
                    target="_blank"
                >
                    <img
                        class="img-fluid"
                        src="@/assets/images/landing/apple-badge.png"
                        alt="Go to App Store"
                    />
                </a>
            </v-col>
        </v-row>
    </v-container>
</template>

<style lang="scss" scoped>
.landing-container {
    max-width: 1140px;
}

.device-preview {
    max-height: 412px;
    &.sm {
        max-height: 212px;
    }
}
</style>
