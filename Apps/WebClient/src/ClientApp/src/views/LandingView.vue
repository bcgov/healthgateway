<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import {
    faCheckCircle,
    faClipboardCheck,
    faClockRotateLeft,
    faCloudArrowDown,
    faDesktop,
    faEdit,
    faFileMedical,
    faHome,
    faMicroscope,
    faMobileScreenButton,
    faPills,
    faStethoscope,
    faSyringe,
    faTabletScreenButton,
    faUserGroup,
    faVial,
} from "@fortawesome/free-solid-svg-icons";
import Vue from "vue";
import { Component } from "vue-property-decorator";
import { Getter } from "vuex-class";

import { EntryType, entryTypeMap } from "@/constants/entryType";
import { RegistrationStatus } from "@/constants/registrationStatus";
import type { WebClientConfiguration } from "@/models/configData";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger } from "@/services/interfaces";

library.add(
    faCheckCircle,
    faClipboardCheck,
    faClockRotateLeft,
    faCloudArrowDown,
    faDesktop,
    faEdit,
    faFileMedical,
    faHome,
    faMicroscope,
    faMobileScreenButton,
    faPills,
    faStethoscope,
    faSyringe,
    faTabletScreenButton,
    faUserGroup,
    faVial
);

interface Tile {
    type: string;
    name: string;
    description: string;
    icon: string;
    active: boolean;
}

@Component
export default class LandingView extends Vue {
    @Getter("webClient", { namespace: "config" })
    config!: WebClientConfiguration;

    @Getter("isOffline", { namespace: "config" })
    isOffline!: boolean;

    @Getter("oidcIsAuthenticated", { namespace: "auth" })
    oidcIsAuthenticated!: boolean;

    @Getter("isSidebarAvailable", { namespace: "navbar" })
    isSidebarAvailable!: boolean;

    @Getter("userIsRegistered", { namespace: "user" })
    userIsRegistered!: boolean;

    private get isVaccinationBannerEnabled(): boolean {
        return false;
    }

    private get isPublicLaboratoryResultEnabled(): boolean {
        return this.config.modules["PublicLaboratoryResult"];
    }

    private logger!: ILogger;
    private isOpenRegistration = false;
    private selectedPreviewDevice = "laptop";

    private entryTypes: EntryType[] = [
        EntryType.Medication,
        EntryType.LaboratoryOrder,
        EntryType.Covid19LaboratoryOrder,
        EntryType.Encounter,
        EntryType.Immunization,
        EntryType.MedicationRequest,
    ];

    private tiles: Tile[] = [];

    private get offlineMessage(): string {
        if (this.isOffline) {
            return this.config.offlineMode?.message || "";
        } else {
            return "";
        }
    }

    private created() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
    }

    private mounted() {
        this.isOpenRegistration =
            this.config.registrationStatus == RegistrationStatus.Open;

        // Get core tiles from entry type constants
        this.loadCoreTiles();

        // Add Proof of Vaccination tile to tiles
        this.addProofOfVaccinationTile();

        // Filter out only active tiles to display
        this.filterActiveTiles();
    }

    private getProofOfVaccinationTile(): Tile {
        const proofOfVaccinationTile: Tile = {
            type: "ProofOfVaccination",
            icon: "check-circle",
            name: "Proof of Vaccination",
            description: "View and download your proof of vaccination",
            active: true,
        };

        this.logger.debug(
            `Proof of Vaccination Tile:  ${JSON.stringify(
                proofOfVaccinationTile
            )}`
        );

        return proofOfVaccinationTile;
    }

    private getTile(entryType: EntryType): Tile | undefined {
        const entry = entryTypeMap.get(entryType);
        if (entry) {
            return {
                type: entryType.toString(),
                icon: entry.icon,
                name: entry.name,
                description: entry.description,
                active: false,
            };
        }
        return undefined;
    }

    private addProofOfVaccinationTile(): void {
        this.tiles.splice(2, 0, this.getProofOfVaccinationTile());
        this.tiles.forEach((tile) =>
            this.logger.debug(`Tile:  ${JSON.stringify(tile)}`)
        );
    }

    private filterActiveTiles(): void {
        for (const moduleName in this.config.modules) {
            const tile = this.tiles.find(
                (tileEntry) => tileEntry.type === moduleName
            );
            if (tile) {
                tile.active = this.config.modules[moduleName];
            }
        }
        this.tiles.forEach((tile) =>
            this.logger.debug(`Active Tile:  ${JSON.stringify(tile)}`)
        );
    }

    private loadCoreTiles(): void {
        // Get core tiles from entry type constants
        this.tiles = this.entryTypes.map((type) => {
            const details = entryTypeMap.get(type);
            const tile: Tile = {
                type: type,
                icon: details?.icon ?? "",
                name: details?.name ?? "",
                description: details?.description ?? "",
                active: false,
            };
            return tile;
        });

        this.tiles.forEach((tile) =>
            this.logger.debug(`Core Tile:  ${JSON.stringify(tile)}`)
        );
    }

    private selectPreviewDevice(deviceName: string): void {
        this.selectedPreviewDevice = deviceName;
        this.$root.$emit(
            "bv::hide::tooltip",
            `preview-device-button-${deviceName}`
        );
    }
}
</script>

<template>
    <div class="landing">
        <b-row
            v-if="isVaccinationBannerEnabled"
            no-gutters
            class="vaccine-card-banner small-banner d-flex justify-content-center"
            :class="{ 'd-lg-none': !isSidebarAvailable }"
        >
            <b-col cols="auto">
                <img
                    src="@/assets/images/landing/vaccine-card-banner-icon-sm.svg"
                    alt="Vaccine Card Logo"
                />
            </b-col>
            <b-col cols="auto" class="text-center p-2 mb-4">
                <h2 class="h4 mt-3">Proof of Vaccination</h2>
                <div class="mb-3">
                    <hg-button
                        variant="primary"
                        to="/vaccinecard"
                        data-testid="btnVaccineCard"
                        class="w-75 text-center"
                    >
                        Get Proof
                    </hg-button>
                </div>
            </b-col>
        </b-row>
        <b-row
            v-if="isVaccinationBannerEnabled"
            class="vaccine-card-banner large-banner d-none justify-content-end"
            :class="{ 'd-lg-flex': !isSidebarAvailable }"
        >
            <b-col cols="auto">
                <img
                    src="@/assets/images/landing/vaccine-card-banner-icon-lg.svg"
                    alt="Vaccine Card Logo"
                />
            </b-col>
            <b-col cols="auto" class="text-center">
                <h2 class="h1 mt-4">Proof of Vaccination</h2>
                <div>
                    Confidential access to your BC and Canada proof of
                    vaccination
                </div>
                <div>
                    <hg-button
                        variant="primary"
                        to="/vaccinecard"
                        class="w-50 my-4 text-center"
                    >
                        Get Proof
                    </hg-button>
                </div>
            </b-col>
        </b-row>
        <b-container v-if="isOffline">
            <b-row class="align-items-center pt-2 pb-5 align-middle">
                <b-col class="cols-12 text-center">
                    <hr class="py-4" />
                    <b-row class="py-2">
                        <b-col class="title">
                            The site is offline for maintenance
                        </b-col>
                    </b-row>
                    <b-row class="py-3">
                        <b-col data-testid="offlineMessage" class="sub-title">
                            {{ offlineMessage }}
                        </b-col>
                    </b-row>
                    <b-row class="pt-5">
                        <b-col>
                            <hr class="pt-5" />
                        </b-col>
                    </b-row>
                </b-col>
            </b-row>
        </b-container>
        <b-container v-else>
            <b-row class="mt-4 mt-md-5">
                <b-col class="col-12 col-lg-5">
                    <h1 class="mb-4">Access your health information online</h1>
                    <p class="mb-4">
                        Health Gateway provides secure and convenient access to
                        your health records in British Columbia
                    </p>
                    <div v-if="!oidcIsAuthenticated" class="mb-3">
                        <router-link to="/login">
                            <hg-button
                                id="btnLogin"
                                data-testid="btnLogin"
                                variant="primary"
                                class="btn-auth-landing"
                            >
                                <span>Log In with BC Services Card</span>
                            </hg-button>
                        </router-link>
                        <div class="mt-3">
                            <span class="mr-2">Need an account?</span>
                            <router-link
                                id="btnStart"
                                data-testid="btnStart"
                                :to="
                                    isOpenRegistration
                                        ? 'registration'
                                        : 'registrationInfo'
                                "
                                >Register</router-link
                            >
                        </div>
                    </div>
                </b-col>
                <b-col class="d-none d-lg-block text-center col-7">
                    <img
                        class="img-fluid"
                        src="@/assets/images/landing/landing-top.png"
                        alt="Health Gateway Preview"
                        data-testid="landing-top-image-id"
                    />
                </b-col>
            </b-row>
            <div class="mt-4 mt-md-5">
                <h1 class="mb-2 mb-md-3 mb-lg-4">What you can access</h1>
                <b-row>
                    <b-col
                        v-for="tile in tiles"
                        :key="tile.name"
                        class="text-center px-4 px-md-5 pb-4 pb-md-5"
                        cols="12"
                        md="6"
                        lg="4"
                    >
                        <div class="icon-header">
                            <hg-icon
                                :icon="tile.icon"
                                size="extra-extra-large"
                                square
                                class="m-3"
                            />
                        </div>
                        <h4>{{ tile.name }}</h4>
                        <p class="mb-0">{{ tile.description }}</p>
                    </b-col>
                </b-row>
            </div>
            <div class="my-4 mb-md-5">
                <b-row>
                    <b-col cols="12" lg="6">
                        <h1 class="mb-4">What you can do</h1>
                    </b-col>
                    <b-col cols="12" lg="6">
                        <p class="mb-0">
                            Health Gateway brings together your information from
                            BC’s health databases. View your medications, health
                            visits, immunizations and more, all in one place.
                            New features and information are regularly added.
                        </p>
                    </b-col>
                </b-row>
                <div class="mt-4 mt-lg-5 text-center">
                    <button
                        id="preview-device-button-laptop"
                        v-b-tooltip.nofade.d0="{
                            customClass: 'd-none d-xl-block',
                        }"
                        title="Show Laptop Preview"
                        :disabled="selectedPreviewDevice === 'laptop'"
                        data-testid="preview-device-button-laptop"
                        class="preview-device-button bg-transparent border-0 shadow-none mx-3 p-2"
                        @click="selectPreviewDevice('laptop')"
                    >
                        <hg-icon icon="desktop" size="extra-large" square />
                    </button>
                    <button
                        id="preview-device-button-tablet"
                        v-b-tooltip.nofade.d0="{
                            customClass: 'd-none d-xl-block',
                        }"
                        title="Show Tablet Preview"
                        :disabled="selectedPreviewDevice === 'tablet'"
                        data-testid="preview-device-button-tablet"
                        class="preview-device-button bg-transparent border-0 shadow-none mx-3 p-2"
                        @click="selectPreviewDevice('tablet')"
                    >
                        <hg-icon
                            icon="tablet-screen-button"
                            size="extra-large"
                            square
                        />
                    </button>
                    <button
                        id="preview-device-button-smartphone"
                        v-b-tooltip.nofade.d0="{
                            customClass: 'd-none d-xl-block',
                        }"
                        title="Show Smartphone Preview"
                        :disabled="selectedPreviewDevice === 'smartphone'"
                        data-testid="preview-device-button-smartphone"
                        class="preview-device-button bg-transparent border-0 shadow-none mx-3 p-2"
                        @click="selectPreviewDevice('smartphone')"
                    >
                        <hg-icon
                            icon="mobile-screen-button"
                            size="extra-large"
                            square
                        />
                    </button>
                </div>
                <div class="text-center">
                    <img
                        v-show="selectedPreviewDevice === 'laptop'"
                        src="@/assets/images/landing/preview-laptop.png"
                        class="img-fluid"
                        data-testid="preview-image-laptop"
                        alt="Preview of Health Gateway on a Laptop"
                    />
                    <img
                        v-show="selectedPreviewDevice === 'tablet'"
                        src="@/assets/images/landing/preview-tablet.png"
                        class="img-fluid"
                        data-testid="preview-image-tablet"
                        alt="Preview of Health Gateway on a Tablet"
                    />
                    <img
                        v-show="selectedPreviewDevice === 'smartphone'"
                        src="@/assets/images/landing/preview-smartphone.png"
                        class="img-fluid"
                        data-testid="preview-image-smartphone"
                        alt="Preview of Health Gateway on a Smartphone"
                    />
                </div>
                <b-row>
                    <b-col cols="12" lg="4" class="p-3">
                        <b-card class="h-100 text-center">
                            <div class="icon-header">
                                <hg-icon
                                    icon="clock-rotate-left"
                                    size="extra-large"
                                />
                            </div>
                            <h4 class="my-3">Stay up-to-date</h4>
                            <p class="mb-0 h-100">
                                View your health information in a list or
                                calendar view, so you can easily find your most
                                recent records. Filter or search to narrow your
                                results.
                            </p>
                        </b-card>
                    </b-col>
                    <b-col cols="12" lg="4" class="p-3">
                        <b-card class="h-100 text-center">
                            <div class="icon-header">
                                <hg-icon
                                    icon="cloud-arrow-down"
                                    size="extra-large"
                                />
                            </div>
                            <h4 class="my-3">Manage your information</h4>
                            <p class="mb-0">
                                Download records so you can organize, print and
                                use them as needed. Make your own notes on
                                records to track important health events.
                            </p>
                        </b-card>
                    </b-col>
                    <b-col cols="12" lg="4" class="p-3">
                        <b-card class="h-100 text-center">
                            <div class="icon-header">
                                <hg-icon icon="home" size="extra-large" />
                            </div>
                            <h4 class="my-3">Quick and easy</h4>
                            <p class="mb-0">
                                Add a shortcut to your preferred health records
                                on the home screen.
                            </p>
                        </b-card>
                    </b-col>
                </b-row>
            </div>
        </b-container>
    </div>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

.landing {
    h1,
    h2,
    h3,
    h4,
    h5,
    h6,
    .icon-header {
        color: $primary;
    }

    .title {
        font-size: 2.2rem;
    }

    .sub-title {
        font-size: 1.5rem;
    }

    .btn-auth-landing {
        background-color: #1a5a95;
        border-color: #1a5a95;
    }

    .vaccine-card-banner {
        color: #212529;
        background-color: $hg-vaccine-card-header;
        background-repeat: no-repeat;
        background-position: right bottom;

        &.small-banner {
            background-image: url("~@/assets/images/landing/vaccine-card-banner-bg-sm.svg");
        }

        &.large-banner {
            background-image: url("~@/assets/images/landing/vaccine-card-banner-bg-lg.svg");
            background-size: 731px;
            height: 186px;
            padding-right: 175px;

            img {
                width: 250px;
                margin-right: 15px;
            }
        }
    }

    .preview-device-button {
        color: $hg-brand-secondary;

        &[disabled] {
            color: $hg-brand-primary;
        }
    }
}
</style>
