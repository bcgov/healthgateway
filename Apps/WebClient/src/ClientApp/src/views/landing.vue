<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import {
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

import Image00 from "@/assets/images/landing/000_Logo-Overlay.png";
import Image02 from "@/assets/images/landing/002_Devices.png";
import Image03 from "@/assets/images/landing/003_reduced-3.jpeg";
import Image04 from "@/assets/images/landing/004_reduced-living-room.jpeg";
import Image05 from "@/assets/images/landing/005_reduced-family.jpeg";
import { RegistrationStatus } from "@/constants/registrationStatus";
import type { WebClientConfiguration } from "@/models/configData";

library.add(
    faClipboardCheck,
    faEdit,
    faMicroscope,
    faPills,
    faSyringe,
    faUserMd,
    faVial
);

interface Icon {
    name: string;
    label: string;
    definition: string;
    active: boolean;
}

interface Tile {
    title: string;
    description: string;
    bullets?: string[];
    imageSrc: string;
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

    private logo: string = Image00;
    private devices: string = Image02;
    private isOpenRegistration = false;

    private icons: Icon[] = [
        {
            name: "Medication",
            definition: "pills",
            label: "Prescription Medications",
            active: false,
        },
        {
            name: "Note",
            definition: "edit",
            label: "Add Notes to Records",
            active: false,
        },
        {
            name: "Laboratory",
            definition: "vial",
            label: "COVID-19 Test Results",
            active: true,
        },
        {
            name: "Immunization",
            definition: "syringe",
            label: "Immunization Records",
            active: false,
        },
        {
            name: "Encounter",
            definition: "user-md",
            label: "Health Visits",
            active: false,
        },
        {
            name: "VaccinationStatus",
            definition: "clipboard-check",
            label: "BC Vaccine Card",
            active: false,
        },
        {
            name: "AllLaboratory",
            definition: "microscope",
            label: "Lab Results",
            active: false,
        },
    ];

    private tiles: Tile[] = [
        {
            title: "One card, many services",
            description:
                "Securely access your data using your BC Services Card on a mobile device.",

            imageSrc: Image03,
        },
        {
            title: "Take control of your health",
            description: "Look at historical information.",
            bullets: [
                "Dispensed Medications",
                "COVID-19 Test Results",
                "COVID-19 Immunization Records",
            ],

            imageSrc: Image04,
        },
        {
            title: "Guardian access",
            description:
                "Access COVID-19 test results for children eleven and under.",
            imageSrc: Image05,
        },
    ];

    private get offlineMessage(): string {
        if (this.isOffline) {
            return this.config.offlineMode?.message || "";
        } else {
            return "";
        }
    }

    private mounted() {
        this.isOpenRegistration =
            this.config.registrationStatus == RegistrationStatus.Open;

        for (const moduleName in this.config.modules) {
            var icon = this.icons.find(
                (iconEntry) => iconEntry.name === moduleName
            );
            if (icon) {
                icon.active = this.config.modules[moduleName];
            }
        }
    }

    private getTileClass(index: number): string {
        return index % 2 == 0 ? "order-md-1" : "order-md-2";
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
        <h3 class="text-center font-weight-normal my-4 mx-1">
            BC residents can access all their health records in a single place
        </h3>
        <b-row
            v-if="!isOffline"
            class="devices-section justify-content-center justify-content-lg-around align-items-center mx-0 mx-md-5"
        >
            <b-col class="d-none d-lg-block text-center col-6 col-xl-4">
                <img
                    class="img-fluid devices-image"
                    :src="devices"
                    width="auto"
                    height="auto"
                    alt="Devices"
                />
            </b-col>
            <b-col class="col-12 col-sm-7 col-lg-5 devices-text ml-md-4">
                <b-row
                    v-for="icon in icons"
                    :key="icon.label"
                    class="my-4 align-items-center"
                    :class="icon.active ? 'status-active' : 'status-inactive'"
                    no-gutters
                >
                    <b-col cols="auto">
                        <hg-icon
                            :icon="icon.definition"
                            size="large"
                            square
                            class="mr-2"
                        />
                    </b-col>
                    <b-col>
                        <span>{{ icon.label }}</span>
                        <span v-if="!icon.active"> (Coming soon)</span>
                    </b-col>
                </b-row>
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
        </b-row>
        <b-row v-else class="align-items-center pt-2 pb-5 align-middle">
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
        <b-row
            v-if="isPublicLaboratoryResultEnabled"
            class="covid19-section mb-3 mb-md-0 py-4 mx-n2"
            data-testid="covidRecordLandingPage"
        >
            <b-col class="col-12 col-md-5 offset-md-1">
                <h2>Get your COVID‑19 test result</h2>
                <hg-button
                    variant="secondary"
                    class="my-2 text-center"
                    data-testid="covid-test-button"
                    to="/covidtest"
                >
                    Get Result
                </hg-button>
            </b-col>
        </b-row>
        <b-row class="tile-section my-0 my-md-1">
            <b-col>
                <b-row
                    v-for="(tile, index) in tiles"
                    :key="tile.title"
                    class="d-flex justify-content-center align-content-around tile-row my-md-5 my-0"
                >
                    <b-col
                        class="col-12 col-md-5"
                        :class="getTileClass(index + 1)"
                    >
                        <div class="background-tint"></div>
                        <img
                            class="img-fluid d-md-block"
                            :src="tile.imageSrc"
                            width="auto"
                            height="auto"
                            alt="B.C. Government Logo"
                        />
                    </b-col>
                    <b-col class="col-12 col-md-5" :class="getTileClass(index)">
                        <div class="text-wrapper mx-4 position-absolute">
                            <h2 class="font-weight-normal">{{ tile.title }}</h2>
                            <div class="small-text">{{ tile.description }}</div>
                            <ul>
                                <li
                                    v-for="bullet in tile.bullets"
                                    :key="bullet"
                                    class="small-text"
                                >
                                    {{ bullet }}
                                </li>
                            </ul>
                        </div>
                    </b-col>
                </b-row>
            </b-col>
        </b-row>
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

    .devices-section {
        .devices-image {
            margin-left: auto;
            margin-right: auto;
            margin-bottom: 20px;
        }

        .devices-text {
            color: $primary;
            max-width: 500px !important;

            .status-active {
                color: $primary;
            }

            .status-inactive {
                color: darkgray;
            }
        }
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

    .tile-section {
        margin-left: 0px;
        margin-right: 0px;
        padding-left: 0px;
        padding-right: 0px;

        .col {
            padding-left: 0px;
            padding-right: 0px;
        }

        .text-wrapper {
            color: $primary;
            z-index: 1;

            .title {
                font-size: 2.2rem;
            }
        }
        /* Small Devices*/
        @media (max-width: 767px) {
            .text-wrapper {
                color: white;
                bottom: 0;

                .title {
                    font-size: 1.8rem;
                    color: white;
                }

                .small-text {
                    font-size: 1rem;
                }
            }

            .background-tint {
                background: -moz-linear-gradient(
                    top,
                    rgba(255, 255, 255, 0) 0%,
                    rgba(0, 0, 0, 0.8) 100%
                ); /* FF3.6+ */
                background: -webkit-gradient(
                    linear,
                    left top,
                    left bottom,
                    color-stop(0%, rgba(255, 255, 255, 0)),
                    color-stop(100%, rgba(0, 0, 0, 0.65))
                ); /* Chrome,Safari4+ */
                background: -webkit-linear-gradient(
                    top,
                    rgba(255, 255, 255, 0) 0%,
                    rgba(0, 0, 0, 0.8) 100%
                ); /* Chrome10+,Safari5.1+ */
                background: -o-linear-gradient(
                    top,
                    rgba(255, 255, 255, 0) 0%,
                    rgba(0, 0, 0, 0.8) 100%
                ); /* Opera 11.10+ */
                background: -ms-linear-gradient(
                    top,
                    rgba(255, 255, 255, 0) 0%,
                    rgba(0, 0, 0, 0.8) 100%
                ); /* IE10+ */
                background: linear-gradient(
                    to bottom,
                    rgba(255, 255, 255, 0) 0%,
                    rgba(0, 0, 0, 0.8) 100%
                ); /* W3C */

                background-blend-mode: multiply;
                width: 100%;
                height: 100%;
                position: absolute;
            }
        }
    }
    .covid19-section {
        background-color: #e8eef5;
    }
}
</style>
