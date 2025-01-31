<script setup lang="ts">
import { computed, ref } from "vue";
import { useDisplay } from "vuetify";

import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import HgIconButtonComponent from "@/components/common/HgIconButtonComponent.vue";
import TileComponent from "@/components/public/landing/TileComponent.vue";
import { EntryType, entryTypeMap } from "@/constants/entryType";
import { ServiceName } from "@/constants/serviceName";
import { InfoTile } from "@/models/infoTile";
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
    EntryType.Covid19TestResult,
    EntryType.HealthVisit,
    EntryType.Immunization,
    EntryType.SpecialAuthorityRequest,
    EntryType.ClinicalDocument,
    EntryType.HospitalVisit,
    EntryType.DiagnosticImaging,
];

const serviceEntryTypes: EntryType[] = [EntryType.BcCancerScreening];

const configStore = useConfigStore();

const selectedPreviewDevice = ref(PreviewDevice.laptop);
const { mdAndUp } = useDisplay();

const showLaptopTooltip = ref(false);
const showTabletTooltip = ref(false);
const showSmartphoneTooltip = ref(false);

const isOffline = computed(() => configStore.isOffline);

const offlineMessage = computed(
    () => configStore.webConfig.offlineMode?.message ?? ""
);
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
    [
        proofOfVaccinationTile.value,
        organDonorRegistrationTile.value,
        ...serviceEntryTypes.map<InfoTile>(mapEntryTypeToTile),
    ].filter((tile) => tile.active)
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
                <div class="mb-4">
                    <HgButtonComponent
                        id="btnLogin"
                        class="btn-auth-landing"
                        text="Log in with BC Services Card"
                        variant="primary"
                        to="/login"
                        data-testid="btnLogin"
                    />
                    <div class="mt-4 d-flex align-center">
                        <span class="text-body-1 mr-2">Need an account?</span>
                        <HgButtonComponent
                            id="btnStart"
                            text="Register"
                            variant="link"
                            to="/registration"
                            data-testid="btnStart"
                        />
                    </div>
                </div>
            </v-col>
            <v-col
                xs="false"
                lg="7"
                class="d-none d-lg-block text-center col-7 pt-0"
            >
                <v-img
                    src="@/assets/images/landing/landing-top.png"
                    alt="Health Gateway Preview"
                    data-testid="landing-top-image-id"
                />
            </v-col>
        </v-row>
        <div v-if="shouldDisplayDatasets" class="mt-6 mt-md-12">
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
        <div v-if="shouldDisplayServices" class="mt-6 mt-md-12">
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
        <div class="mt-6 mt-md-12">
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
                                View your health information in a list or
                                calendar view, so you can easily see what’s new.
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
                                Add a quick link to the records you use the
                                most. Filter or search to find what you need.
                            </p>
                        </template>
                    </v-card>
                </v-col>
            </v-row>
        </div>
    </v-container>
</template>

<style lang="scss" scoped>
.device-preview {
    max-height: 212px;

    &.device-preview-md {
        max-height: 412px;
    }
}
</style>
