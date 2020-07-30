<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

.landing {
    h1 {
        color: $primary;
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
    }

    .title-section h2 {
        font-weight: 200;
        font-size: x-large;
        /* Small Devices*/
        @media (max-width: 767px) {
            text-align: left !important;
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
            margin-left: auto;
            margin-right: auto;
            margin-top: -40px;
            /* Small Devices*/
            @media (max-width: 767px) {
                font-size: small;
            }
            .icon-row {
                display: block;
                line-height: 40px;
                height: 40px;
                .icon {
                    text-align: center;
                    border-radius: 50%;
                    width: 40px;
                }
                .edit {
                    background-color: #fcba19 !important;
                }
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

            .btn {
                width: 150px;
            }

            #btnLogin {
                color: $primary;
            }

            .mobile-only {
                /* Tablet & Desktop*/
                @media (min-width: 768px) {
                    display: none;
                }
                /* Mobile */
                @media (max-width: 767px) {
                    display: flex;
                }
            }

            .covid-container {
                color: white;
                background-color: $danger;
                border-radius: 15px;
                height: 25px;
                line-height: 28px;
                margin-top: 5px;
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
                    font-size: 2rem;
                    color: white;
                }
                .small-text {
                    font-size: 1.2rem;
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
<template>
    <div class="landing">
        <CommunicationComponent />
        <b-row
            class="title-section justify-content-center align-items-center mx-1 mx-md-5 my-2"
        >
            <b-col class="col-12">
                <div class="title-text">
                    <b-row>
                        <h1 class="text-center w-100 p-3">Health Gateway</h1>
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
            class="devices-section justify-content-center align-items-center mx-1 mx-md-5"
        >
            <b-col class="d-none d-lg-block text-center col-6 col-xl-4 m-auto">
                <img
                    class="img-fluid devices-image"
                    :src="devices"
                    width="auto"
                    height="auto"
                    alt="Devices"
            /></b-col>
            <b-col class="col-12 col-md-6">
                <div class="devices-text my-3 my-md-3 ml-md-4">
                    <b-row>
                        <b-col cols="0" class="icon-wrapper mr-4 px-0">
                            <div
                                v-for="icon in icons"
                                :key="icon.label"
                                class="mb-2 icon-row"
                                :class="
                                    icon.active
                                        ? 'status-active'
                                        : 'status-inactive'
                                "
                                align-content="center"
                            >
                                <div class="icon" :class="icon.definition">
                                    <font-awesome-icon
                                        :icon="icon.definition"
                                        size="lg"
                                    ></font-awesome-icon>
                                </div>
                            </div>
                        </b-col>
                        <b-col class="px-0">
                            <div
                                v-for="icon in icons"
                                :key="icon.label"
                                class="mb-2 d-flex icon-row"
                                :class="
                                    icon.active
                                        ? 'status-active'
                                        : 'status-inactive'
                                "
                                align-v="center"
                            >
                                <b-col cols="0" class="px-0">
                                    <span>{{ icon.label }}</span>
                                </b-col>
                                <b-col
                                    v-if="false && icon.label === 'Lab Tests'"
                                    cols="0"
                                    class="covid-container ml-2 px-2"
                                >
                                    <font-awesome-icon
                                        class="px-1"
                                        icon="exclamation-triangle"
                                        size="1x"
                                    ></font-awesome-icon>
                                    <span class="pr-1"
                                        >COVID-19 Test Result</span
                                    >
                                </b-col>
                            </div>
                        </b-col>
                    </b-row>
                    <b-row class="py-3">
                        <b-col>
                            <b-button
                                id="btnStart"
                                :to="
                                    isOpenRegistration
                                        ? 'registration'
                                        : 'registrationInfo'
                                "
                                role="button"
                                class="m-2"
                                >Register</b-button
                            >
                            <div class="row mobile-only"></div>
                            <b-button
                                id="btnLogin"
                                to="login"
                                variant="outline-secondary"
                                class="m-2"
                                >Log in</b-button
                            >
                        </b-col>
                    </b-row>
                </div>
            </b-col>
        </b-row>
        <b-row class="tile-section my-0 my-md-1">
            <div class="w-100 p-3">
                <b-row
                    class="d-flex justify-content-center align-content-around tile-row my-md-5 my-0"
                >
                    <b-col class="col-12 col-md-6">
                        <h1 class="text-center">
                            Future Goals
                        </h1>
                    </b-col>
                </b-row>
            </div>
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
                        <div class="text-wrapper m-4 position-absolute">
                            <div class="title">{{ tile.title }}</div>
                            <div class="small-text">{{ tile.description }}</div>
                        </div>
                    </b-col>
                </b-row>
            </div>
        </b-row>
    </div>
</template>

<script lang="ts">
import Vue from "vue";
import { Component } from "vue-property-decorator";
import container from "@/plugins/inversify.config";
import { Getter } from "vuex-class";
import CommunicationComponent from "@/components/communication.vue";
import { WebClientConfiguration } from "@/models/configData";
import { RegistrationStatus } from "@/constants/registrationStatus";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import {
    IconDefinition,
    faClipboard,
    faExclamationTriangle,
} from "@fortawesome/free-solid-svg-icons";

// @ts-ignore-start
import Image00 from "@/assets/images/landing/000_Logo-Overlay.png";
import Image02 from "@/assets/images/landing/002_Devices.png";
import Image03 from "@/assets/images/landing/003_AdobeStock_143856492-edited_shoes.jpg";
import Image04 from "@/assets/images/landing/004_AdobeStock_216356596.jpeg";
import Image05 from "@/assets/images/landing/005_AdobeStock_243861557.jpeg";
import Image06 from "@/assets/images/landing/006_AdobeStock_223963895.jpeg";
import Image07 from "@/assets/images/landing/007_Hero-02_Duotone.png";
// @ts-ignore-end

interface Icon {
    label: string;
    definition: string;
    active: boolean;
}

interface Tile {
    title: string;
    description: string;
    imageSrc: string;
}

@Component({
    components: {
        CommunicationComponent,
    },
})
export default class LandingView extends Vue {
    private icons: Icon[] = [
        {
            definition: "pills",
            label: "Prescription Medication(Dec 2019)",
            active: true,
        },
        {
            definition: "edit",
            label: "Add Notes to Records(Mar 2020)",
            active: true,
        },
        {
            definition: "syringe",
            label: "Immunization(Coming soon)",
            active: false,
        },
        {
            definition: "flask",
            label: "Lab Results(Coming soon)",
            active: false,
        },
        {
            definition: "user-md",
            label: "Health Visits(Coming soon)",
            active: false,
        },
    ];

    private tiles: Tile[] = [
        {
            title: "All in one place",
            description:
                "Conveniently access your data on a computer, tablet or smartphone",
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

    private logo: string = Image00;
    private devices: string = Image02;
    private bottomImage: string = Image07;
    private isOpenRegistration: boolean = false;
    private getTileClass(index: number): string {
        return index % 2 == 0 ? "order-md-1" : "order-md-2";
    }
    @Getter("webClient", { namespace: "config" })
    webClientConfig!: WebClientConfiguration;

    private mounted() {
        this.isOpenRegistration =
            this.webClientConfig.registrationStatus == RegistrationStatus.Open;
    }
}
</script>
