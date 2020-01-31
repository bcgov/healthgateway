<template>
  <v-container fill-height fluid grid-list-xl>
    <v-layout justify-center wrap>
      <v-data-table
        :headers="headers"
        :items="desserts"
        :items-per-page="5"
        class="elevation-1"
      ></v-data-table>
    </v-layout>
  </v-container>
</template>

<script lang="ts">
import { Component, Vue } from "vue-property-decorator";
import Vuetify, { VLayout } from "vuetify/lib";
import { IBetaRequestService } from "@/services/interfaces";
import UserBetaRequest from "@/models/userBetaRequest";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";

@Component
export default class BetaQueueView extends Vue {
  private isLoading: boolean = true;
  private hasErrors: boolean = false;

  private headers: any[] = [
    {
      text: "Registration Date",
      align: "left",
      sortable: false,
      value: "registrationDatetime"
    },
    { text: "Email", value: "emailAddress" }
  ];

  private desserts: UserBetaRequest[] = [];

  private betaRequestService!: IBetaRequestService;

  mounted() {
    this.betaRequestService = container.get(
      SERVICE_IDENTIFIER.BetaRequestService
    );

    this.desserts.push({
      id: "Some cool email",
      emailAddress: "anEmail",
      registrationDatetime: new Date(),
      version: 1
    });

    this.betaRequestService
      .getPendingRequests()
      .then(betaRequests => {
        console.log("beta request:", betaRequests);
        for (let result of betaRequests) {
          this.desserts.push(result);
        }
      })
      .catch(err => {
        console.log("ERRRRORRRR", err);
        this.hasErrors = true;
      })
      .finally(() => {
        this.isLoading = false;
      });
  }
}
</script>
