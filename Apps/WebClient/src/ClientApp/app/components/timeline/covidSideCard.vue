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
  <b-card-header header-bg-variant="danger" header-text-variant="white">
      <font-awesome-icon :icon="attentionIcon" size="1x"/>
      {{ card.title }}
  </b-card-header>
    <b-card-body @click="handleClick(card.urlLink)">
      <b-card-text>{{ card.description }}</b-card-text>
    </b-card-body>
  </b-card>
</template>

<script lang="ts">
import Vue from "vue";
import { Component } from "vue-property-decorator";
import { IconDefinition } from '@fortawesome/fontawesome-svg-core';
import { faExclamationTriangle } from '@fortawesome/free-solid-svg-icons';

interface CovidSideCard {
  title: string;
  description: string;
  urlLink: string;
}

@Component
export default class CovidSidebarComponent extends Vue {
  private card: CovidSideCard = {
    title: "COVID-19 Test Result",
    description: "If you have taken a COVID-19 test, you can view your results here.",
    urlLink: ""
  };

  private get attentionIcon(): IconDefinition {
      return faExclamationTriangle;
  }

  private handleClick(urlLink: string): void {
    window.open(urlLink, "_blank");
  }
}
</script>
