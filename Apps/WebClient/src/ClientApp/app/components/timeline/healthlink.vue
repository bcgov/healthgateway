<style lang="scss" scoped>
.box {
  box-shadow: 0 2.8px 2.2px rgba(0, 0, 0, 0.015),
    0 6.7px 5.3px rgba(0, 0, 0, 0.048), 0 12.5px 10px rgba(0, 0, 0, 0.06),
    0 22.3px 17.9px rgba(0, 0, 0, 0.072), 0 41.8px 33.4px rgba(0, 0, 0, 0.05),
    0 100px 80px rgba(0, 0, 0, 0.07);

  margin: 100px auto;
  width: 160px;
  background: white;
  border-radius: 5px;
}
.box:hover {
  background: rgba(0, 0, 0, 0.022);
}
.stretched-link::after {
  position: absolute;
  top: 0;
  right: 0;
  bottom: 0;
  left: 0;
  z-index: 1;
  pointer-events: auto;
  content: "";
  background-color: rgba(0, 0, 0, 0);
}
.stretched-link::after {
  position: absolute;
  top: 0;
  right: 0;
  bottom: 0;
  left: 0;
  z-index: 1;
  pointer-events: auto;
  content: "";
  background-color: rgba(0, 0, 0, 0);
}
</style>

<template>
  <b-card
    :title="currentCard.title"
    img-top
    tag="article"
    style="max-width: 50rem;border: none;"
    class="mb-2 box d-none d-lg-block"
  >
    <a
      href="https://www.healthlinkbc.ca/services-and-resources/about-8-1-1 "
      target="_blank"
      class="stretched-link"
    ></a>
    <b-card-text>{{ currentCard.description }}</b-card-text>
  </b-card>
</template>

<script lang="ts">
import Vue from "vue";
import { Prop, Component } from "vue-property-decorator";
interface Healthcard {
  title: string;
  description: string;
  url: string;
}
@Component
export default class HealthlinkSidebarComponent extends Vue {
  private currentCard: Healthcard = { title: "", description: "", url: "" };
  private cardPool: Healthcard[] = [
    {
      title: "8-1-1",
      description:
        "Speak to a pharmacist from 5pm to 9am Pacific Time everyday of the year.",
      url: "sdsa.com"
    },
    {
      title: "Healthlink",
      description:
        "HealthLink BC provides reliable non-emergency health information and advice in British Columbia.",
      url: "lab.com"
    }
  ];
  mounted() {
    let cardIndex: number = Math.floor(Math.random() * this.cardPool.length);
    this.currentCard = this.cardPool[cardIndex];
    console.log(this.currentCard);
  }
}
</script>
