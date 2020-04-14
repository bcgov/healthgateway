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
            <v-text-field label="Filter" hide-details="auto" v-model="filterText">
                <v-icon slot="append">fas fa-search</v-icon>
            </v-text-field>
        </v-col>
    </v-row>
    <v-row justify="center">
      <v-col md="9">
        <v-row>
          <v-col no-gutters>
            <v-data-table
              v-model="selectedEmails"
              :headers="tableHeaders"
              :items="emailList"
              :items-per-page="5"
              show-select
              :search="filterText"
            >
              <template v-slot:item.sentDateTime="{ item }">
                  <span>{{ item.sentDateTime ? formatDate(item.sentDateTime) : "" }}</span>
              </template>
            </v-data-table>
          </v-col>
        </v-row>
        <v-row justify="end" no-gutters>
          <v-btn :disabled="selectedEmails.length === 0" @click="resendEmails()"
            >Resend Emails</v-btn
          >
        </v-row>
      </v-col>
    </v-row>
  </v-container>
</template>

<script lang="ts">
import { Component, Vue } from "vue-property-decorator";
import Vuetify, { VLayout } from "vuetify/lib";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import LoadingComponent from "@/components/core/Loading.vue";
import BannerFeedbackComponent from "@/components/core/BannerFeedback.vue";
import BannerFeedback from "@/models/bannerFeedback";
import { ResultType } from "@/constants/resulttype";
import Email from "@/models/email";
import { IEmailAdminService } from "@/services/interfaces";

@Component({
  components: {
    LoadingComponent,
    BannerFeedbackComponent
  }
})
export default class ResendEmailView extends Vue {
  private filterText: string = "";
  private isLoading: boolean = true;
  private showFeedback: boolean = false;
  private bannerFeedback: BannerFeedback = {
    type: ResultType.NONE,
    title: "",
    message: ""
  };

  private selectedEmails: Email[] = [];

  private tableHeaders: any[] = [
    {
      text: "Subject",
      value: "subject"
    },
    {
      text: "Status",
      value: "emailStatusCode"
    },
    {
      text: "Date",
      value: "sentDateTime"
    },
    { text: "Email", value: "to" },
    { text: "Is Invited?", value: "userInviteStatus" }
  ];

  private emailList: Email[] = [];

  private emailService!: IEmailAdminService;

  mounted() {
    this.emailService = container.get(SERVICE_IDENTIFIER.EmailAdminService);
    this.loadEmails();
  }

  private loadEmails() {
    this.isLoading = true;
    this.emailList = [];
    this.selectedEmails = [];
    this.emailService
      .getEmails()
      .then(emails => {
        this.emailList.push(...emails);
      })
      .catch(err => {
          console.log(err);
        this.showFeedback = true;
        this.bannerFeedback = {
          type: ResultType.Error,
          title: "Error",
          message: "Failed to load emails"
        };
      })
      .finally(() => {
        this.isLoading = false;
      });
  }

  private formatDate(date: Date): string {
    return new Date(Date.parse(date + "Z")).toLocaleString();
  }

  private resendEmails(): void {
    this.isLoading = true;
    let selectedIds = this.selectedEmails.map(s => s.id);
    this.emailService
      .resendEmails(selectedIds)
      .then(emails => {
        this.showFeedback = true;
        this.bannerFeedback = {
          type: ResultType.Success,
          title: "Success.",
          message: "Emails queued to be sent"
        };
      })
      .catch(err => {
        this.showFeedback = true;
        this.bannerFeedback = {
          type: ResultType.Error,
          title: "Error",
          message: "Sending emails failed, please try again"
        };
      })
      .finally(() => {
        this.isLoading = false;
        this.loadEmails();
      });
  }
}
</script>
