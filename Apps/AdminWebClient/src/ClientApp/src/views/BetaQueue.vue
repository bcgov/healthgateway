<template>
  <v-container>
    <LoadingComponent :is-loading="isLoading"></LoadingComponent>
    <BannerFeedbackComponent
      :show-feedback.sync="showFeedback"
      :feedback="bannerFeedback"
      class="mt-5"
    ></BannerFeedbackComponent>
    <v-row justify="center">
      <v-col md="9">
        
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
            :disabled="selectedRequests.length === 0"
            @click="sendInvites()"
            >Send invites</v-btn
          >
        </v-row>
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
import LoadingComponent from "@/components/Loading.vue";
import BannerFeedbackComponent from "@/components/BannerFeedback.vue";
import BannerFeedback from "@/models/bannerFeedback";
import { ResultType } from "@/constants/resulttype";

@Component({
  components: {
    LoadingComponent,
    BannerFeedbackComponent
  }
})
export default class BetaQueueView extends Vue {
  private isLoading: boolean = true;
  private showFeedback: boolean = false;
  private bannerFeedback: BannerFeedback = {
    type: ResultType.NONE,
    title: "",
    message: ""
  };

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
        this.showFeedback = true;
        this.bannerFeedback = {
          type: ResultType.Error,
          title: "Error",
          message: "Failed to load pending beta requests"
        };
      })
      .finally(() => {
        this.isLoading = false;
      });
  }

  private formatDate(date: Date): string {
    return new Date(Date.parse(date + "Z")).toLocaleString();
  }

  private sendInvites(): void {
    this.isLoading = true;
    let selectedIds = this.selectedRequests.map(s => s.id);
    this.betaRequestService
      .sendBetaInvites(selectedIds)
      .then(sucessfulInvites => {
        // remove the invites that where sucessfull
        for (let sentId of sucessfulInvites) {
          var elementPos = this.requestList.map(r => r.id).indexOf(sentId);
          if (elementPos > -1) {
            this.requestList.splice(elementPos, 1);
          }
        }
        this.showFeedback = true;
        this.bannerFeedback = {
          type: ResultType.Success,
          title: "Invites queued",
          message: `Successfully added ${sucessfulInvites.length} invite emails to be sent`
        };
      })
      .catch(err => {
        this.showFeedback = true;
        this.bannerFeedback = {
          type: ResultType.Error,
          title: "Error",
          message: "Sending invites failed, please try again"
        };
      })
      .finally(() => {
        this.isLoading = false;
      });
  }
}
</script>
