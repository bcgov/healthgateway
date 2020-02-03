<style lang="scss" scoped>
.box {
  border: none;
  cursor: pointer;
  font-size: 0.9rem;
  max-width: 280px;
}

.card-image {
  max-height: 140px;
}
</style>

<template>
  <b-card
    class="box d-none d-lg-block mx-auto bg-white border-0 rounded shadow"
    no-body
  >
    <b-card-body @click="handleClick(currentCard.urlLink)">
      <img
        class="img-fluid mx-auto d-block card-image"
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
import PhoneImage from "@/assets/images/timeline/811.png";
import HealthlinkImage from "@/assets/images/timeline/healthlink.png";

interface Healthcard {
  title: string;
  description: string;
  imageSrc: string;
  urlLink: string;
}

@Component
export default class HealthlinkSidebarComponent extends Vue {
  private currentCard: Healthcard = {
    title: "",
    description: "",
    imageSrc: "",
    urlLink: ""
  };
  private cardPool: Healthcard[] = [
    {
      title: "8-1-1",
      description:
        "Speak to a pharmacist from 5pm to 9am Pacific Time everyday of the year.",
      imageSrc: PhoneImage,
      urlLink: "https://www.healthlinkbc.ca/services-and-resources/about-8-1-1"
    },
    {
      title: "Healthlink",
      description:
        "HealthLink BC provides reliable non-emergency health information and advice in British Columbia.",
      imageSrc: HealthlinkImage,
      urlLink: "https://www.healthlinkbc.ca"
    }
  ];
  mounted() {
    let cardIndex: number = Math.floor(Math.random() * this.cardPool.length);
    this.currentCard = this.cardPool[cardIndex];
  }

  private handleClick(urlLink: string): void {
    window.open(urlLink, "_blank");
  }
}
</script>
