<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faChevronDown, faChevronUp } from "@fortawesome/free-solid-svg-icons";
import Vue from "vue";
import { Component, Prop } from "vue-property-decorator";
import { Getter } from "vuex-class";

import Image00 from "@/assets/images/registration/000_Logo-Dark.png";
import Image01 from "@/assets/images/registration/001_BC-Services-Card.png";
import Image02 from "@/assets/images/registration/002_App-Store.png";
import Image03 from "@/assets/images/registration/003_Mobile-App.png";
import Image04 from "@/assets/images/registration/004_USB-Card-Reader.png";
import Image05 from "@/assets/images/registration/005_Mobile-Card.png";
import { RegistrationStatus } from "@/constants/registrationStatus";
import type { WebClientConfiguration } from "@/models/configData";

library.add(faChevronDown, faChevronUp);

@Component
export default class RegistrationInfoView extends Vue {
    @Prop() inviteKey?: string;
    @Prop() email?: string;
    @Getter("webClient", { namespace: "config" })
    webClientConfig!: WebClientConfiguration;

    private logoImg: string = Image00;
    private bcServicesCardImg: string = Image01;
    private appStoreImg: string = Image02;
    private mobileAppImg: string = Image03;
    private cardReaderImg: string = Image04;
    private mobileCardImg: string = Image05;
    private signupProcessVisible = false;
    private dongleVisible = false;
    private registrationLink = "/registration/";
    private isRegistrationClosed = false;

    private mounted() {
        this.isRegistrationClosed =
            this.webClientConfig.registrationStatus ==
            RegistrationStatus.Closed;
    }
}
</script>

<template>
    <b-container>
        <b-row class="pt-5">
            <b-col class="text-center mb-5">
                <b-alert :show="isRegistrationClosed" variant="info">
                    <h4>Closed Registration</h4>
                    <span class="text-justify d-flex p-2">
                        Registration for the Health Gateway is closed at this
                        time. Please check back for updates. Thank you.
                    </span>
                </b-alert>
                <h4 class="title">
                    Register securely to access your health records using BC
                    Services Card
                </h4>
            </b-col>
            <b-container class="register-with-bc-card">
                <b-row>
                    <b-col>
                        <img
                            class="img-fluid d-none d-md-block logo"
                            :src="logoImg"
                            width="auto"
                            height="auto"
                            alt="B.C. Government Logo"
                        />
                    </b-col>
                </b-row>
                <b-row>
                    <b-col fluid>
                        <img
                            width="350"
                            src="@/assets/images/landing/002_Devices.png"
                            alt="Different Devices (Laptop, Tablet, Phone)"
                    /></b-col>

                    <b-col fluid>
                        <b-row class="mt-5">
                            <b-col>
                                <router-link :to="registrationLink">
                                    <hg-button
                                        size="lg"
                                        variant="primary"
                                        data-testid="registerBtn"
                                    >
                                        <img
                                            class="mr-3"
                                            :src="mobileCardImg"
                                            height="40"
                                            alt="BC Services Card App Icon"
                                        />Register for Health Gateway
                                    </hg-button>
                                </router-link>
                            </b-col>
                        </b-row>
                    </b-col>
                </b-row>
                <b-row>
                    <b-col class="text-left mt-5">
                        <hg-button
                            id="servicesCardBtn"
                            data-testid="servicesCardBtn"
                            :class="signupProcessVisible ? 'collapsed' : null"
                            :aria-expanded="
                                signupProcessVisible ? 'true' : 'false'
                            "
                            aria-controls="collapse-1"
                            variant="secondary"
                            @click="
                                signupProcessVisible = !signupProcessVisible
                            "
                        >
                            <span v-show="!signupProcessVisible">
                                <hg-icon
                                    icon="chevron-down"
                                    size="medium"
                                    aria-hidden="true"
                                />
                            </span>
                            <span v-show="signupProcessVisible">
                                <hg-icon
                                    icon="chevron-up"
                                    size="medium"
                                    aria-hidden="true"
                                />
                            </span>
                            <span class="ml-2"
                                >I Have a BC Services Card, but How Do I Use
                                It?</span
                            >
                        </hg-button>
                        <b-collapse
                            id="collapse-1"
                            v-model="signupProcessVisible"
                        >
                            <b-row class="m-5">
                                <b-col>
                                    <b-row>
                                        <b-col>
                                            <span
                                                class="
                                                    align-top
                                                    bullet
                                                    rounded-circle
                                                    py-1
                                                    px-2
                                                "
                                                >1</span
                                            >
                                            <img
                                                class="image-step"
                                                :src="bcServicesCardImg"
                                                alt="B.C. Services Card"
                                            />
                                        </b-col>
                                    </b-row>
                                    <b-row>
                                        <b-col>
                                            <span class="pl-4"
                                                >Grab your card</span
                                            >
                                        </b-col>
                                    </b-row>
                                </b-col>
                                <b-col>
                                    <b-row>
                                        <b-col>
                                            <span
                                                class="
                                                    align-top
                                                    bullet
                                                    rounded-circle
                                                    py-1
                                                    px-2
                                                "
                                                >2</span
                                            >
                                            <img
                                                class="image-step"
                                                :src="appStoreImg"
                                                alt="App Store, Google Play"
                                            />
                                        </b-col>
                                    </b-row>
                                    <b-row>
                                        <b-col>
                                            <span class="pl-4"
                                                >Download the BC Services Card
                                                app</span
                                            >
                                        </b-col>
                                    </b-row>
                                </b-col>
                                <b-col>
                                    <b-row>
                                        <b-col>
                                            <span
                                                class="
                                                    align-top
                                                    bullet
                                                    rounded-circle
                                                    py-1
                                                    px-2
                                                "
                                                >3</span
                                            >
                                            <img
                                                class="image-step"
                                                :src="mobileAppImg"
                                                alt="Mobile App"
                                            />
                                            <b-row>
                                                <b-col>
                                                    <span class="pl-4"
                                                        >Follow instructions
                                                        in-app</span
                                                    >
                                                </b-col>
                                            </b-row>
                                            <b-row>
                                                <b-col>
                                                    <span class="pl-4"
                                                        >to set up your mobile
                                                        card</span
                                                    >
                                                </b-col>
                                            </b-row>
                                        </b-col>
                                    </b-row>
                                </b-col>
                            </b-row>
                        </b-collapse>
                    </b-col>
                </b-row>
                <b-row class="mb-5">
                    <b-col>
                        <hg-button
                            id="moreOptionsBtn"
                            data-testid="moreOptionsBtn"
                            class="my-3"
                            :class="dongleVisible ? 'collapsed' : null"
                            :aria-expanded="dongleVisible ? 'true' : 'false'"
                            aria-controls="collapse-4"
                            variant="secondary"
                            @click="dongleVisible = !dongleVisible"
                        >
                            <span v-show="!dongleVisible">
                                <hg-icon
                                    icon="chevron-down"
                                    size="medium"
                                    aria-hidden="true"
                                />
                            </span>
                            <span v-show="dongleVisible">
                                <hg-icon
                                    icon="chevron-up"
                                    size="medium"
                                    aria-hidden="true"
                                />
                            </span>
                            <span class="ml-2">Is There Another Option?</span>
                        </hg-button>
                        <b-collapse id="collapse-4" v-model="dongleVisible">
                            <div class="m-5 px-4">
                                <b-row>
                                    <b-col>
                                        <img
                                            class="image-step"
                                            :src="cardReaderImg"
                                            alt="Card Reader"
                                        />
                                    </b-col>
                                </b-row>
                                <b-row>
                                    <b-col>USB card reader</b-col>
                                    <br />
                                    <br />
                                </b-row>
                                <b-row>
                                    <b-col
                                        >If you are using a computer with a USB
                                        port, you can choose to use a goverment
                                        issued USB card reader. The USB card
                                        reader will read the chip embedded in
                                        your BC Services Card to identify you
                                        when you log in.</b-col
                                    >
                                </b-row>
                                <b-row>
                                    <b-col
                                        ><a
                                            rel="noopener"
                                            target="_blank"
                                            href="https://www2.gov.bc.ca/gov/content/governments/government-id/bc-services-card/login-with-card/card-readers-passcodes"
                                            >Learn more about using your BC
                                            Services Card</a
                                        ></b-col
                                    >
                                </b-row>
                            </div>
                        </b-collapse>
                    </b-col>
                </b-row>
            </b-container>
        </b-row>
    </b-container>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";
.title {
    color: $primary;
    font-size: 2.1em;
}
.logo {
    width: 300px;
    margin-bottom: 1em;
}
.image-step {
    height: 100px;
    margin-bottom: 1em;
}
.bullet {
    background-color: $bcgold;
    color: white;
}
.btn-light {
    color: $primary;
}
</style>
