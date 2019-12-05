<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

.landing {
  .btn-secondary-landing {
    box-shadow: 0 4px 8px 0 rgba(0, 0, 0, 0.2), 0 6px 20px 0 rgba(0, 0, 0, 0.19);
    border-width: 0px;
    width: 150px;
    font-weight: 600;
    color: $primary;
    background-color: $soft_background;
  }

  .btn-primary-landing {
    box-shadow: 0 4px 8px 0 rgba(0, 0, 0, 0.2), 0 6px 20px 0 rgba(0, 0, 0, 0.19);
    border-width: 0px;
    min-width: 150px;
    font-weight: 600;
    color: $primary;
    background-color: $bcgold;
  }

  .intro {
    height: 750px;
    background-size: cover;

    .title-wrapper {
      margin-bottom: auto;
      margin-left: auto;
      margin-right: auto;
      max-width: 600px;

      border-radius: 25px;
      background-color: rgba(0, 0, 0, 0.5);
    }

    .logo {
      margin-bottom: 1em;
    }

    .title-text {
      color: white;
      font-size: 2.2em;
    }

    .register-link {
      color: white;
    }
  }

  .devices-section {
    .devices-image {
      margin-left: auto;
      margin-right: auto;
      margin-top: -75px;
      margin-bottom: 20px;
    }

    .devices-text {
      color: $primary;
      margin-left: auto;
      margin-right: auto;
      margin-top: -40px;
    }
  }

  .circle-icons {
    background-color: $primary;
    color: white;
    display: flex;
    justify-content: center;

    .icon-wrapper {
      margin: 40px 35px 40px 35px;
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

    /* Large Devices, Wide Screens */
    @media (max-width: 767px) {
      /* your desktop css */
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

  .secure {
    height: 750px;
    background-size: cover;

    .secure-wrapper {
      margin-top: 130px;
      margin-bottom: auto;
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
      <!--Empty Column for  fitting reasons-->
      <b-col class="col-12 col-md-4 col-xl-6 d-none d-md-block" />
      <b-col class="col-12 col-md-8 col-xl-6">
        <div class="title-wrapper m-2 m-sm-5 p-5 p-md-5">
          <img
            class="img-fluid logo py-4"
            :src="logo"
            width="auto"
            height="auto"
            alt="B.C. Government Logo"
          />

          <div class="title-text">
            <div>
              Empowering you to manage your health
            </div>
          </div>
          <b-row class=" p-3 p-md-5">
            <b-col class="justify-content-center">
              <b-button
                id="btnStart"
                to="registrationInfo"
                class="btn btn-primary-landing w-100"
                role="button"
                >Register</b-button
              >
              <b-button
                id="btnLogin"
                to="login"
                class="w-100 register-link"
                variant="link"
                >Already registered? <strong>Login</strong></b-button
              >
            </b-col>
          </b-row>
        </div>
      </b-col>
    </b-row>
    <b-row
      class="devices-section justify-content-center align-items-center mx-5"
    >
      <b-col class="d-none d-md-block text-center col-6 col-xl-4 m-auto">
        <img
          class="img-fluid devices-image"
          :src="devices"
          width="auto"
          height="auto"
          alt="Devices"
      /></b-col>
      <b-col class="col-12 col-md-6">
        <div class="devices-text my-5 my-md-5 mx-md-5">
          <h3>Browse your health records</h3>
          <div>
            Prescription medications
          </div>
          <div>
            Visits to clinics <span class="font-italic">(coming soon)</span>
          </div>
          <div>
            Lab test results <span class="font-italic">(coming soon)</span>
          </div>
          <div>Vaccinations <span class="font-italic">(coming soon)</span></div>
        </div>
      </b-col>
    </b-row>
    <b-row class="circle-icons justify-content-center">
      <div
        v-for="icon in icons"
        :key="icon.label"
        class="icon-wrapper text-center px-xl-5"
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
    <b-row class="tile-section my-0 my-md-1">
      <div>
        <b-row
          v-for="(tile, index) in tiles"
          :key="tile.title"
          class="d-flex justify-content-center align-content-around tile-row my-md-5 my-0"
        >
          <b-col class="col-12 col-md-5" :class="getTileClass(index + 1)">
            <div class="background-tint"></div>
            <img
              class="img-fluid d-md-block "
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
    <b-row
      class="secure justify-content-around"
      :style="{ backgroundImage: 'url(\'' + bottomImage + '\')' }"
    >
      <b-col class="col-12 col-md-6">
        <div class="secure-wrapper pl-5 pl-md-0">
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
              class="btn btn-primary-landing m-2"
              role="button"
              >Sign Up</b-button
            >
            <b-button
              id="btnLearnMore"
              href="https://www2.gov.bc.ca/gov/content/governments/government-id/bc-services-card"
              class="btn btn-secondary-landing m-2"
              >Learn More</b-button
            >
          </div>
        </div>
      </b-col>
      <b-col class="col-0 col-md-1 d-none d-md-block " />
    </b-row>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import { Component } from "vue-property-decorator";
import Image00 from "@/assets/images/landing/000_Logo-Overlay.png";
import Image01 from "@/assets/images/landing/001_Hero-01_Duotone.jpeg";
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

  private getTileClass(index: number): string {
    return index % 2 == 0 ? "order-md-1" : "order-md-2";
  }
}
</script>
