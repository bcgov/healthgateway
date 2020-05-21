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
import CovidImage from "@/assets/images/timeline/CovidSidecardImage.png";

interface Healthcard {
  title: string;
  description: string;
  imageSrc: string;
  urlLink: string;
}

@Component
export default class CovidSidecardComponent extends Vue {
  private currentCard: Healthcard = {
    title: "COVID-19",
    description:
      "Non-medical information about COVID-19 is available 7:30am-8pm, 7 days a week at 1-888-COVID19 (1-888-268-4319).",
    imageSrc: CovidImage,
    urlLink: "http://www.bccdc.ca/health-info/diseases-conditions/covid-19",
  };

  private handleClick(urlLink: string): void {
    window.open(urlLink, "_blank");
  }
}
</script>
