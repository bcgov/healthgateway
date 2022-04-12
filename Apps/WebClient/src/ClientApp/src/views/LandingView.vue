<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import {
    faCheckCircle,
    faClipboardCheck,
    faEdit,
    faMicroscope,
    faPills,
    faSyringe,
    faUserMd,
    faVial,
} from "@fortawesome/free-solid-svg-icons";
import Vue from "vue";
import { Component } from "vue-property-decorator";
import { Getter } from "vuex-class";

import LandingTopImage from "@/assets/images/landing/landing-top.png";
import { EntryType, entryTypeMap } from "@/constants/entryType";
import { RegistrationStatus } from "@/constants/registrationStatus";
import type { WebClientConfiguration } from "@/models/configData";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger } from "@/services/interfaces";

library.add(
    faCheckCircle,
    faClipboardCheck,
    faEdit,
    faMicroscope,
    faPills,
    faSyringe,
    faUserMd,
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

    private landingTop: string = LandingTopImage;
    private logger!: ILogger;
    private isOpenRegistration = false;

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
            description:
                "View and download your Federal or Provincial proof of vaccination",
            active: true,
        };

        this.logger.debug(
            `Proof of Vaccination Tile:  ${JSON.stringify(
                proofOfVaccinationTile
            )}`
        );

        return proofOfVaccinationTile;
    }

    private getTileClass(index: number): string {
        return index % 2 == 0 ? "order-md-1" : "order-md-2";
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
            var tile = this.tiles.find(
                (tileEntry) => tileEntry.name === moduleName
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
        const coreTiles: Tile[] = this.entryTypes.map((type) => {
            const details = entryTypeMap.get(type);
            let tile: Tile = {
                type: type,
                icon: details?.icon ?? "",
                name: details?.name ?? "",
                description: details?.description ?? "",
                active: false,
            };
            return tile;
        });

        this.tiles = coreTiles.filter((tile) => {
            return tile !== undefined;
        });

        this.tiles.forEach((tile) =>
            this.logger.debug(`Core Tile:  ${JSON.stringify(tile)}`)
        );
    }
}
</script>

<template>
    <div class="landing mx-2">
        <b-row
            v-if="isVaccinationBannerEnabled"
            no-gutters
            class="vaccine-card-banner small-banner d-flex mx-n2 justify-content-center"
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
            class="vaccine-card-banner large-banner d-none justify-content-end mx-n2"
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
            <b-row class="py-4" no-gutters>
                <b-col class="col-12 col-lg-6">
                    <h1 class="mb-3">Access your health information online</h1>
                    <p>
                        Health Gateway provides secure and convenient access to
                        your health records in British Columbia
                    </p>
                    <div class="my-5">
                        <router-link v-if="!oidcIsAuthenticated" to="/login">
                            <hg-button
                                id="btnLogin"
                                data-testid="btnLogin"
                                variant="primary"
                                class="btn-auth-landing"
                            >
                                <span>Log In with BC Services Card</span>
                            </hg-button>
                        </router-link>
                        <div v-if="!oidcIsAuthenticated" class="my-3">
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
                <b-col class="d-none d-lg-block text-center col-6">
                    <img
                        class="img-fluid"
                        :src="landingTop"
                        width="auto"
                        height="auto"
                        alt="Health Gateway Preview"
                    />
                </b-col>
            </b-row>

            <b-row>
                <b-col>
                    <h1>What you can access</h1>
                    <b-row class="mx-2 my-2">
                        <b-col
                            v-for="tile in tiles"
                            :key="tile.name"
                            class="text-center px-5 py-3"
                            cols="12"
                            md="6"
                            lg="4"
                        >
                            <div>
                                <hg-icon
                                    :icon="tile.icon"
                                    size="extra-extra-large"
                                    square
                                    class="m-3"
                                />
                            </div>
                            <h4>{{ tile.name }}</h4>
                            <p>{{ tile.description }}</p>
                        </b-col>
                    </b-row>
                </b-col>
            </b-row>
        </b-container>
    </div>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

.landing {
    color: $primary;

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

    .title-section {
        color: $primary;
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
}
</style>
