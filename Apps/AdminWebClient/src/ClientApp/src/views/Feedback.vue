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
        <v-row no-gutters>
          <h1>User Feedback list</h1>
        </v-row>
        <v-row>
          <v-col no-gutters>
            <v-data-table
              :headers="tableHeaders"
              :items="feedbackList"
              :items-per-page="5"
              @click:row="markReviewed"
            >
              <template v-slot:item.createdDateTime="{ item }">
                <span>{{ formatDate(item.createdDateTime) }}</span>
              </template>
            </v-data-table>
          </v-col>
        </v-row>
      </v-col>
    </v-row>
  </v-container>
</template>

<script lang="ts">
import { Component, Vue } from "vue-property-decorator";
import Vuetify, { VLayout } from "vuetify/lib";
import { IUserFeedbackService } from "@/services/interfaces";
import UserFeedback from "@/models/userFeedback";
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
export default class FeedbackView extends Vue {
  private isLoading: boolean = true;
  private showFeedback: boolean = false;
  private bannerFeedback: BannerFeedback = {
    type: ResultType.NONE,
    title: "",
    message: ""
  };

  private tableHeaders: any[] = [
    {
      text: "Date",
      value: "createdDateTime"
    },
    {
      text: "Satisfied",
      value: "isSatisfied"
    },
    {
      text: "Comments",
      value: "comments"
    },
    {
      text: "Status",
      value: "isReviewed"
    }
  ];

  private feedbackList: UserFeedback[] = [];

  private userFeedbackService!: IUserFeedbackService;

  mounted() {
    this.userFeedbackService = container.get(
      SERVICE_IDENTIFIER.UserFeedbackService
    );

    this.userFeedbackService
      .getFeedbackList()
      .then(userFeedbacks => {
        this.feedbackList.push(...userFeedbacks);
      })
      .catch(err => {
        this.showFeedback = true;
        this.bannerFeedback = {
          type: ResultType.Error,
          title: "Error",
          message: "Error loading user feedbacks"
        };
      })
      .finally(() => {
        this.isLoading = false;
      });
  }

  private formatDate(date: Date): string {
    return new Date(Date.parse(date + "Z")).toLocaleString();
  }

  private markReviewed(feedback: UserFeedback): void {
    this.isLoading = true;
    this.userFeedbackService
      .markReviewed(feedback.id)
      .then(sucessfulInvites => {
        this.showFeedback = true;
        this.bannerFeedback = {
          type: ResultType.Success,
          title: "Feedback Reviewed",
          message: "Successfully Reviewed User Feedback."
        };
      })
      .catch(err => {
        this.showFeedback = true;
        this.bannerFeedback = {
          type: ResultType.Error,
          title: "Error",
          message: "Reviewing feedback failed, please try again."
        };
      })
      .finally(() => {
        this.isLoading = false;
      });
  }
}
</script>
