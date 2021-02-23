<script lang="ts">
import Vue from "vue";
import { Component } from "vue-property-decorator";
import { Getter } from "vuex-class";

import Image00 from "@/assets/images/landing/000_Logo-Overlay.png";
import Image02 from "@/assets/images/landing/002_Devices.png";
import Image03 from "@/assets/images/landing/003_AdobeStock_143856492-edited_shoes.jpg";
import Image04 from "@/assets/images/landing/004_AdobeStock_216356596.jpeg";
import Image05 from "@/assets/images/landing/005_AdobeStock_243861557.jpeg";
import Image06 from "@/assets/images/landing/006_AdobeStock_223963895.jpeg";
import Image07 from "@/assets/images/landing/007_Hero-02_Duotone.png";
import CommunicationComponent from "@/components/communication.vue";
import { RegistrationStatus } from "@/constants/registrationStatus";
import type { WebClientConfiguration } from "@/models/configData";

interface Icon {
    name: string;
    label: string;
    definition: string;
    active: boolean;
    color: string;
}

interface Tile {
    title: string;
    description: string;
    bullets?: string[];
    imageSrc: string;
}

@Component({
    components: {
        CommunicationComponent,
    },
})
export default class LandingView extends Vue {
    @Getter("webClient", { namespace: "config" })
    webClientConfig!: WebClientConfiguration;
    @Getter("oidcIsAuthenticated", {
        namespace: "auth",
    })
    oidcIsAuthenticated!: boolean;
    @Getter("isOffline", {
        namespace: "config",
    })
    isOffline!: boolean;

    private logo: string = Image00;
    private devices: string = Image02;
    private bottomImage: string = Image07;
    private isOpenRegistration = false;

    private icons: Icon[] = [
        {
            name: "Medication",
            definition: "pills",
            label: "Prescription Medications (Dec 2019)",
            color: "",
            active: false,
        },
        {
            name: "Note",
            definition: "edit",
            label: "Add Notes to Records (Mar 2020)",
            color: "#fcba19",
            active: false,
        },
        {
            name: "Laboratory",
            definition: "exclamation-triangle",
            label: "COVID-19 Test Results (Sep 2020)",
            color: "#dc3545",
            active: true,
        },
        {
            name: "Immunization",
            definition: "syringe",
            label: "Immunization Records (Dec 2020)",
            color: "",
            active: false,
        },
        {
            name: "Encounter",
            definition: "user-md",
            label: "Health Visits",
            color: "",
            active: false,
        },
        {
            name: "Laboratory-Inactive",
            definition: "flask",
            label: "Lab Results",
            color: "",
            active: false,
        },
    ];

    private tiles: Tile[] = [
        {
            title: "All in one place",
            description:
                "Conveniently access your data on a computer, tablet or smartphone",
            bullets: [
                "Dispensed Medications from Community Pharmacies",
                "COVID-19 Test Results and COVID-19 Immunization Card",
                "Public Health and Community Pharmacy Immunization Records",
            ],
            imageSrc: Image03,
        },
        {
            title: "Take control of your health",
            description: "Look at historical information captured over time.",

            imageSrc: Image04,
        },
        {
            title: "Manage family records",
            description:
                "Care for the needs of your children and those who depend on you.",
            imageSrc: Image05,
        },
        {
            title: "Collaborate with others",
            description:
                "Become an active participant by sharing and discussing your data with health care providers.",
            imageSrc: Image06,
        },
    ];

    private get offlineMessage(): string {
        if (this.isOffline) {
            return this.webClientConfig.offlineMode?.message || "";
        } else {
            return "";
        }
    }

    private mounted() {
        this.isOpenRegistration =
            this.webClientConfig.registrationStatus == RegistrationStatus.Open;

        for (const moduleName in this.webClientConfig.modules) {
            var icon = this.icons.find(
                (iconEntry) => iconEntry.name === moduleName
            );
            if (icon) {
                icon.active = this.webClientConfig.modules[moduleName];
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
        <CommunicationComponent />
        <b-row
            class="title-section justify-content-center align-items-center mx-1 mx-md-5 my-2"
        >
            <b-col class="col-12 mx-0 px-0">
                <div class="title-text">
                    <b-row class="mx-0 px-0">
                        <h1 class="text-center w-100 py-3">Health Gateway</h1>
                    </b-row>
                    <b-row>
                        <h2 class="text-center w-100 p-3">
                            A single place for BC residents to access their
                            health records
                        </h2>
                    </b-row>
                </div>
            </b-col>
        </b-row>
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
            /></b-col>
            <b-col class="col-12 col-sm-7 col-lg-5 devices-text my-3 ml-md-4">
                <b-row
                    v-for="icon in icons"
                    :key="icon.label"
                    class="icon-wrapper my-2 ml-auto justify-content-start"
                    :class="icon.active ? 'status-active' : 'status-inactive'"
                >
                    <b-col
                        class="mr-2 px-0 icon"
                        :style="{ background: icon.color }"
                        cols="0"
                    >
                        <font-awesome-icon
                            :icon="icon.definition"
                            size="lg"
                        ></font-awesome-icon>
                    </b-col>
                    <b-col class="px-0 text-left" cols="auto">
                        <span
                            >{{ icon.label }}
                            <span v-if="!icon.active">
                                (Coming soon)</span
                            ></span
                        >
                    </b-col>
                </b-row>
                <b-row
                    v-if="!oidcIsAuthenticated"
                    class="py-1 justify-content-center justify-content-lg-start"
                >
                    <b-button
                        id="btnStart"
                        data-testid="btnStart"
                        :to="
                            isOpenRegistration
                                ? 'registration'
                                : 'registrationInfo'
                        "
                        role="button"
                        class="col-12 col-sm-5 col-lg-4 my-2 m-sm-2 mx-lg-3"
                        >Register</b-button
                    >
                    <b-button
                        id="btnLogin"
                        data-testid="btnLogin"
                        to="login"
                        variant="outline-secondary"
                        class="col-12 col-sm-5 col-lg-4 my-2 m-sm-2 mx-lg-3"
                        >Log in</b-button
                    >
                </b-row>
            </b-col>
        </b-row>
        <b-row v-else class="align-items-center pt-2 pb-5 align-middle"
            ><b-col class="cols-12 text-center">
                <hr class="py-4" />
                <b-row class="py-2">
                    <b-col class="title"
                        >The site is offline for maintenance</b-col
                    ></b-row
                >
                <b-row class="py-3"
                    ><b-col data-testid="offlineMessage" class="sub-title">
                        {{ offlineMessage }}
                    </b-col></b-row
                >
                <b-row class="pt-5"
                    ><b-col> <hr class="pt-5" /> </b-col>
                </b-row> </b-col
        ></b-row>
        <b-row class="tile-section my-0 my-md-1">
            <div>
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
                    /></b-col>
                    <b-col class="col-12 col-md-5" :class="getTileClass(index)">
                        <div class="text-wrapper mx-4 position-absolute">
                            <div class="title">{{ tile.title }}</div>
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
            </div>
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

    .btn-secondary-landing {
        box-shadow: 0 4px 8px 0 rgba(0, 0, 0, 0.2),
            0 6px 20px 0 rgba(0, 0, 0, 0.19);
        border-width: 0px;
        width: 150px;
        font-weight: 600;
        color: $primary;
        background-color: $soft_background;
    }

    .btn-primary-landing {
        box-shadow: 0 4px 8px 0 rgba(0, 0, 0, 0.2),
            0 6px 20px 0 rgba(0, 0, 0, 0.19);
        border-width: 0px;
        min-width: 150px;
        font-weight: 600;
        color: $primary;
        background-color: $bcgold;
    }

    .title-section {
        color: $primary;
        h2 {
            font-weight: 200;
            font-size: x-large;
        }
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

            .icon-wrapper {
                line-height: 40px;
                height: 40px;
            }

            .icon {
                text-align: center;
                border-radius: 50%;
                height: 40px;
                width: 40px;
            }

            .status-active {
                color: $primary;
                .icon {
                    background-color: $primary;
                    color: white;
                }
            }

            .status-inactive {
                color: darkgray;
                .icon {
                    background-color: darkgray;
                    color: white;
                }
            }

            #btnLogin {
                color: $primary;
            }
        }

        #btnStart {
            background-color: $primary;
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
}
</style>
