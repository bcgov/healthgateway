<template>
  <v-container>
    <v-row justify="center">
      <v-col md="9">
        <v-row no-gutters>
          <h1>Beta user list</h1>
        </v-row>
        <v-row>
          <v-col no-gutters>
            <v-data-table
              v-model="selectedRequests"
              :headers="tableHeaders"
              :items="requestList"
              :items-per-page="5"
              show-select
            >
              <template v-slot:item.registrationDatetime="{ item }">
                <span>{{ formatDate(item.registrationDatetime) }}</span>
              </template>
            </v-data-table>
          </v-col>
        </v-row>
        <v-row justify="end" no-gutters>
          <v-btn
            class="test"
            :disabled="selectedRequests.length === 0"
            @click="sendInvites()"
            >Send invites</v-btn
          >
        </v-row>
        <v-alert
          v-if="inviteSentCount > 0"
          class="mt-5"
          type="success"
          dismissible
        >
          Successfully added <strong>{{ inviteSentCount }}</strong> beta invite
          emails to be sent
        </v-alert>
      </v-col>
    </v-row>
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

  private selectedRequests: UserBetaRequest[] = [];

  private tableHeaders: any[] = [
    {
      text: "Registration Date",
      value: "registrationDatetime"
    },
    { text: "Email", value: "emailAddress" }
  ];

  private requestList: UserBetaRequest[] = [];

  private betaRequestService!: IBetaRequestService;

  private inviteSentCount: number = 0;

  mounted() {
    this.betaRequestService = container.get(
      SERVICE_IDENTIFIER.BetaRequestService
    );

    this.betaRequestService
      .getPendingRequests()
      .then(betaRequests => {
        for (let result of betaRequests) {
          this.requestList.push(result);
        }
      })
      .catch(err => {
        this.hasErrors = true;
      })
      .finally(() => {
        this.isLoading = false;
      });
  }

  private formatDate(date: Date): string {
    return new Date(Date.parse(date + "Z")).toLocaleString();
  }

  private sendInvites(): void {
    let selectedIds = this.selectedRequests.map(s => s.id);
    this.betaRequestService
      .sendBetaInvites(selectedIds)
      .then(sucessfulInvites => {
        // remove the invites that where sucessfull
        this.inviteSentCount = sucessfulInvites.length;
        for (let sentId of sucessfulInvites) {
          var elementPos = this.requestList.map(r => r.id).indexOf(sentId);
          if (elementPos > -1) {
            this.requestList.splice(elementPos, 1);
          }
        }
      })
      .catch(err => {
        this.hasErrors = true;
      })
      .finally(() => {
        this.isLoading = false;
      });
  }
}
</script>
