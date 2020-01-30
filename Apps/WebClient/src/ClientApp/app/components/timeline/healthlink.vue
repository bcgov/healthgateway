<style lang="scss" scoped>
.box {
  width: 160px;
  border: none;
  cursor: pointer;
}
</style>

<template>
  <b-card
    class="box d-none d-lg-block mx-auto bg-white border-0 rounded shadow"
    no-body
  >
    <b-card-body @click="handleClick()">
      <img
        class="img-fluid px-4"
        :src="currentCard.imageSrc"
        width="auto"
        height="auto"
        :alt="currentCard.title"
      />
      <a></a>
      <b-card-text class="mt-3">{{ currentCard.description }}</b-card-text>
    </b-card-body>
  </b-card>
</template>

<script lang="ts">
import Vue from "vue";
import { Component } from "vue-property-decorator";
import PhoneImage from "@/assets/images/timeline/8-1-1.png";
import HealthlinkImage from "@/assets/images/timeline/healthlink.png";

interface Healthcard {
  title: string;
  description: string;
  imageSrc: string;
}

@Component
export default class HealthlinkSidebarComponent extends Vue {
  private currentCard: Healthcard = {
    title: "",
    description: "",
    imageSrc: ""
  };
  private cardPool: Healthcard[] = [
    {
      title: "8-1-1",
      description:
        "Speak to a pharmacist from 5pm to 9am Pacific Time everyday of the year.",
      imageSrc: PhoneImage
    },
    {
      title: "Healthlink",
      description:
        "HealthLink BC provides reliable non-emergency health information and advice in British Columbia.",
      imageSrc: HealthlinkImage
    }
  ];
  mounted() {
    let cardIndex: number = Math.floor(Math.random() * this.cardPool.length);
    this.currentCard = this.cardPool[cardIndex];
  }

  private handleClick(): void {
    window.open(
      "https://www.healthlinkbc.ca/services-and-resources/about-8-1-1 ",
      "_blank"
    );
  }
}
</script>
