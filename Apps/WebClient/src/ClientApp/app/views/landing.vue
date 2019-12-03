<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

.landing {
  .btn-secondary-landing {
    box-shadow: 0 4px 8px 0 rgba(0, 0, 0, 0.2), 0 6px 20px 0 rgba(0, 0, 0, 0.19);
    border-width: 0px;
    margin: 0.8em;
    width: 150px;
    font-weight: 600;
    color: $primary;
    background-color: $soft_background;
  }

  .btn-primary-landing {
    box-shadow: 0 4px 8px 0 rgba(0, 0, 0, 0.2), 0 6px 20px 0 rgba(0, 0, 0, 0.19);
    border-width: 0px;
    margin: 0.8em;
    width: 150px;
    font-weight: 600;
    color: $primary;
    background-color: $bcgold;
  }

  .background-tint-light {
    background-color: rgba(0, 51, 102, 0.1); // Tint color
    background-blend-mode: multiply;
  }

  .background-tint-dark {
    background-color: rgba(0, 51, 102, 0.3); // Tint color
    background-blend-mode: multiply;
  }

  .intro {
    height: 750px;
    background-size: cover;

    .title-wrapper {
      max-width: 390px;
      margin-top: -100px;
      padding: 1em;
      margin-bottom: auto;
      margin-left: auto;
      margin-right: auto;
    }

    .logo {
      width: 300px;
      margin-bottom: 1em;
    }

    .title-text {
      color: white;
      font-size: 2.2em;
    }
  }

  .devices-section {
    .devices-image {
      margin-left: auto;
      margin-right: auto;
      padding: 2em;
      margin-top: -75px;
      margin-bottom: 20px;
    }

    .devices-text {
      color: $primary;
      margin-left: auto;
      margin-right: auto;
      margin-top: -40px;
      padding: 3em;
    }
  }

  .circle-icons {
    background-color: $primary;
    color: white;
    display: flex;
    justify-content: center;

    .icon-wrapper {
      margin: 70px 65px 70px 65px;
    }

    .icon {
      color: $primary;
      height: 110px;
      width: 110px;
      padding: 25px 25px;
      margin-left: auto;
      margin-right: auto;
      margin-bottom: 10px;
    }
  }

  .tile-section {
    margin-top: 50px;
    margin-bottom: 50px;
    margin-left: auto;
    margin-right: auto;
    max-width: 70%;

    .row {
      max-width: 100%;
      margin-top: 40px;
      margin-left: 0px;
      margin-right: 0px;
    }

    .col {
      padding-left: 0px;
      padding-right: 0px;
    }

    .text-wrapper {
      margin-top: 5em;
      color: $primary;

      .title {
        font-size: 2.2rem;
      }
    }
  }

  .secure {
    height: 750px;
    background-size: cover;

    .secure-wrapper {
      max-width: 600px;
      margin-top: 130px;
      margin-bottom: auto;
      margin-left: auto;
      margin-right: auto;
    }

    .title {
      color: $primary;
      font-size: 2.2em;
    }

    .description {
      color: $primary;
    }

    .secure-buttons {
      margin-top: 1em;
    }
  }
}
</style>
<template>
  <div class="landing">
    <b-row
      class="intro align-items-center"
      :style="{ backgroundImage: 'url(\'' + introBackground + '\')' }"
    >
      <b-col>
        <div class="title-wrapper">
          <img
            class="img-fluid d-none d-md-block logo"
            :src="logo"
            width="auto"
            height="auto"
            alt="B.C. Government Logo"
          />

          <div class="title-text">
            <div>
              Empowering you
            </div>
            <div>
              to manage your health
            </div>
          </div>
          <b-row class="justify-content-around">
            <b-button
              id="btnStart"
              to="registrationInfo"
              class="btn btn-primary-landing"
              role="button"
              >Sign Up</b-button
            >
            <b-button id="btnLogin" to="login" class="btn btn-secondary-landing"
              >Log In</b-button
            >
          </b-row>
        </div>
      </b-col>
      <b-col />
    </b-row>
    <b-row class="devices-section align-items-center mx-3">
      <b-col>
        <img
          class="img-fluid d-none d-md-block devices-image"
          :src="devices"
          width="auto"
          height="auto"
          alt="B.C. Government Logo"
      /></b-col>
      <b-col>
        <div class="devices-text ">
          <h2>Browse your health records</h2>
          <p>
            Visits to clinics, lab test results, dispensed medications and
            vaccinations.
          </p>
        </div>
      </b-col>
    </b-row>
    <b-row class="circle-icons">
      <div
        v-for="icon in icons"
        :key="icon.label"
        class="icon-wrapper text-center"
      >
        <div class="icon rounded-circle bg-white">
          <h3>
            <span class="fa fa-2x" :class="icon.classIcon"></span>
          </h3>
        </div>
        <span>
          {{ icon.label }}
        </span>
      </div>
    </b-row>
    <div>
      <div class="tile-section">
        <div v-for="(tile, index) in tiles" :key="tile.title" class="tile-row">
          <b-row v-if="index % 2 === 0">
            <b-col>
              <div class="text-wrapper mr-5">
                <div class="title">{{ tile.title }}</div>
                <div class="small-text">{{ tile.description }}</div>
              </div>
            </b-col>
            <b-col>
              <img
                class="img-fluid d-md-block"
                :src="tile.imageSrc"
                width="auto"
                height="auto"
                alt="B.C. Government Logo"
            /></b-col>
          </b-row>
          <b-row v-else>
            <b-col>
              <img
                class="img-fluid d-none d-md-block"
                :src="tile.imageSrc"
                width="auto"
                height="auto"
                alt="B.C. Government Logo"
            /></b-col>
            <b-col>
              <div class="text-wrapper ml-5">
                <div class="title">{{ tile.title }}</div>
                <div class="small-text">{{ tile.description }}</div>
              </div>
            </b-col>
          </b-row>
        </div>
      </div>
      <b-row
        class="secure"
        :style="{ backgroundImage: 'url(\'' + bottomImage + '\')' }"
      >
        <b-col class="col-8">
          <div class="secure-wrapper">
            <div class="title">
              Securely access your information
            </div>
            <div class="description">
              Use your BC Services Card with two-factor authentication
            </div>
            <div class="secure-buttons">
              <b-button
                id="btnSecureStart"
                to="registrationInfo"
                class="btn btn-primary-landing"
                role="button"
                >Sign Up</b-button
              >
              <b-button
                id="btnLearnMore"
                href="https://www2.gov.bc.ca/gov/content/governments/government-id/bc-services-card"
                class="btn btn-secondary-landing"
                >Learn More</b-button
              >
            </div>
          </div>
        </b-col>
        <b-col class="col-2" />
      </b-row>
    </div>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import { Component } from "vue-property-decorator";
import Image00 from "@/assets/images/landing/000_Logo-Overlay.png";
import Image01 from "@/assets/images/landing/001_Hero-01_Duotone.png";
import Image02 from "@/assets/images/landing/002_Devices.png";
import Image03 from "@/assets/images/landing/003_AdobeStock_143856492-edited_shoes.jpg";
import Image04 from "@/assets/images/landing/004_AdobeStock_216356596.jpeg";
import Image05 from "@/assets/images/landing/005_AdobeStock_243861557.jpeg";
import Image06 from "@/assets/images/landing/006_AdobeStock_223963895.jpeg";
import Image07 from "@/assets/images/landing/007_Hero-02_Duotone.png";

interface Icon {
  label: string;
  classIcon: string;
}

interface Tile {
  title: string;
  description: string;
  imageSrc: string;
}

@Component
export default class LandingComponent extends Vue {
  private icons: Icon[] = [
    {
      classIcon: "fa-pills",
      label: "Medications"
    },
    {
      classIcon: "fa-user-md",
      label: "Consultations"
    },
    {
      classIcon: "fa-flask",
      label: "Lab Tests"
    },
    {
      classIcon: "fa-syringe",
      label: "Vaccinations"
    }
  ];

  private tiles: Tile[] = [
    {
      title: "All in one place",
      description:
        "Conveniently access your data on a computer, tablet or smartphone",
      imageSrc: Image03
    },
    {
      title: "Take control of your health",
      description: "Look at historical information captured over time.",
      imageSrc: Image04
    },
    {
      title: "Manage family records",
      description:
        "Care for the needs of your children and those who depend on you.",
      imageSrc: Image05
    },
    {
      title: "Collaborate with others",
      description:
        "Become an active participant by sharing and discussing your data with health care providers.",
      imageSrc: Image06
    }
  ];

  private logo: string = Image00;
  private introBackground: string = Image01;
  private devices: string = Image02;
  private bottomImage: string = Image07;
}
</script>
