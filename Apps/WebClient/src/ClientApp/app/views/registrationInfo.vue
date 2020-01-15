<style lang="scss">
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
<template>
  <b-container>
    <b-row class="pt-5">
      <b-col class="text-center mb-5">
        <b-alert
          :show="regInviteOnly"
          variant="info"
        >
          <h4>Invite Only Registration</h4>
          <span>Hi, thank you for your interest in the Health Gateway.
            Registration is invite only at this time, however, you can enter your email address to be added to the waitlist.
            We will send you registration link in the near future to participate as an early user of the application.
          </span>
        </b-alert>
        <b-alert
          :show="regClosed"
          variant="info"
        >
          <h4>Closed Registration</h4>
          <span>
            Registration for the Health Gateway is closed at this time. Please check back for updates. Thank you.
          </span>
        </b-alert>
        <h4 class="title">
          Register securely to access your health records using BC Services Card
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
            <img width="350" src="@/assets/images/landing/002_Devices.png"
          /></b-col>

          <b-col fluid>
            <b-row class="mt-5">
              <b-col>
                <router-link :to="registrationLink">
                  <b-button size="lg" variant="primary">
                    <img
                      class="mr-3"
                      :src="mobileCardImg"
                      height="40"
                    />Register for Health Gateway
                  </b-button>
                </router-link>
              </b-col>
            </b-row>
          </b-col>
        </b-row>
        <b-row>
          <b-col class="text-left mt-5">
            <b-button
              id="servicesCardBtn"
              :class="signupProcessVisible ? 'collapsed' : null"
              :aria-expanded="signupProcessVisible ? 'true' : 'false'"
              aria-controls="collapse-1"
              size="lg"
              variant="light"
              @click="signupProcessVisible = !signupProcessVisible"
            >
              <span v-show="!signupProcessVisible">
                <font-awesome-icon
                  icon="chevron-down"
                  aria-hidden="true"
                ></font-awesome-icon>
              </span>
              <span v-show="signupProcessVisible">
                <font-awesome-icon
                  icon="chevron-up"
                  aria-hidden="true"
                ></font-awesome-icon>
              </span>

              I have a BC Services Card, but how do I use it?
            </b-button>
            <b-collapse
              id="collapse-1"
              v-model="signupProcessVisible"
              class="mt-2"
            >
              <b-row class="m-5">
                <b-col>
                  <b-row>
                    <b-col>
                      <span class="align-top bullet rounded-circle py-1 px-2"
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
                      <span class="pl-4">Grab your card</span>
                    </b-col>
                  </b-row>
                </b-col>
                <b-col>
                  <b-row>
                    <b-col>
                      <span class="align-top bullet rounded-circle py-1 px-2"
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
                        >Download the BC Services Card app</span
                      >
                    </b-col>
                  </b-row>
                </b-col>
                <b-col>
                  <b-row>
                    <b-col>
                      <span class="align-top bullet rounded-circle py-1 px-2"
                        >3</span
                      >
                      <img
                        class="image-step"
                        :src="mobileAppImg"
                        alt="Mobile App"
                      />
                      <b-row>
                        <b-col>
                          <span class="pl-4">Follow instructions in-app</span>
                        </b-col>
                      </b-row>
                      <b-row>
                        <b-col>
                          <span class="pl-4">to set up your mobile card</span>
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
            <b-button
              id="moreOptionsBtn"
              class="my-3"
              :class="dongleVisible ? 'collapsed' : null"
              :aria-expanded="dongleVisible ? 'true' : 'false'"
              aria-controls="collapse-4"
              size="lg"
              variant="light"
              @click="dongleVisible = !dongleVisible"
            >
              <span v-show="!dongleVisible">
                <font-awesome-icon
                  icon="chevron-down"
                  aria-hidden="true"
                ></font-awesome-icon>
              </span>
              <span v-show="dongleVisible">
                <font-awesome-icon
                  icon="chevron-up"
                  aria-hidden="true"
                ></font-awesome-icon>
              </span>

              Is there another option?
            </b-button>
            <b-collapse id="collapse-4" v-model="dongleVisible" class="mt-2">
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
                    >If you are using a computer with a USB port, you can choose
                    to use a goverment issued USB card reader. The USB card
                    reader will read the chip embedded in your BC Services Card
                    to identify you when you log in.</b-col
                  >
                </b-row>
                <b-row>
                  <b-col
                    ><a
                      target="_blank"
                      href="https://www2.gov.bc.ca/gov/content/governments/government-id/bc-services-card/login-with-card/card-readers-passcodes"
                      >Learn more about using your BC Services Card</a
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
<script lang="ts">
import Image00 from "@/assets/images/registration/000_Logo-Dark.png";
import Image01 from "@/assets/images/registration/001_BC-Services-Card.png";
import Image02 from "@/assets/images/registration/002_App-Store.png";
import Image03 from "@/assets/images/registration/003_Mobile-App.png";
import Image04 from "@/assets/images/registration/004_USB-Card-Reader.png";
import Image05 from "@/assets/images/registration/005_Mobile-Card.png";
import Vue from "vue";
import { Getter } from "vuex-class";
import { Component, Prop } from "vue-property-decorator";
import { WebClientConfiguration } from "@/models/configData";
import { RegistrationStatus } from "@/constants/registrationStatus";

@Component
export default class RegistrationInfoComponent extends Vue {
  private logoImg: string = Image00;
  private bcServicesCardImg: string = Image01;
  private appStoreImg: string = Image02;
  private mobileAppImg: string = Image03;
  private cardReaderImg: string = Image04;
  private mobileCardImg: string = Image05;
  private signupProcessVisible: boolean = false;
  private dongleVisible: boolean = false;
  private registrationLink: string = "/registration/";
  private regInviteOnly: boolean = false;
  private regClosed: boolean = false;
  @Prop() inviteKey?: string;
  @Prop() email?: string;
  @Getter("webClient", { namespace: "config" })
  webClientConfig: WebClientConfiguration;

  mounted() {
    this.regInviteOnly = this.webClientConfig.registrationStatus == RegistrationStatus.InviteOnly;
    this.regClosed = this.webClientConfig.registrationStatus == RegistrationStatus.Closed;

    if (
      this.regInviteOnly &&
      this.inviteKey
    ) {
      this.registrationLink += `?inviteKey=${this.inviteKey}`;
      this.registrationLink += this.email ? `&email=${this.email}` : "";
    }
  }
}
</script>
