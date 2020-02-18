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
              :headers="tableHeaders"
              :items="feedbackList"
              :items-per-page="5"
            >
              <template v-slot:item.createdDateTime="{ item }">
                <span>{{ formatDate(item.createdDateTime) }}</span>
              </template>
              <template v-slot:item.isReviewed="{ item }">
                <td>
                  <v-btn class="mx-2" dark small @click="toggleReviewed(item)">
                    <v-icon v-if="item.isReviewed" color="green" dark
                      >fa-check</v-icon
                    >
                    <v-icon v-if="!item.isReviewed" color="red" dark
                      >fa-times</v-icon
                    >
                  </v-btn>
                </td>
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
import LoadingComponent from "@/components/core/Loading.vue";
import BannerFeedbackComponent from "@/components/core/BannerFeedback.vue";
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
      value: "createdDateTime",
      width: "20%"
    },
    {
      text: "Satisfied?",
      value: "isSatisfied",
      width: "10%"
    },
    {
      text: "Comments",
      value: "comments",
      width: "65%"
    },
    {
      text: "Reviewed?",
      value: "isReviewed",
      width: "5%"
    }
  ];

  private feedbackList: UserFeedback[] = [];

  private userFeedbackService!: IUserFeedbackService;

  mounted() {
    this.userFeedbackService = container.get(
      SERVICE_IDENTIFIER.UserFeedbackService
    );

    this.loadFeedbackList();
  }

  private loadFeedbackList() {
    this.userFeedbackService
      .getFeedbackList()
      .then(userFeedbacks => {
        this.feedbackList = [];
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

  private toggleReviewed(feedback: UserFeedback): void {
    this.isLoading = true;
    feedback.isReviewed = !feedback.isReviewed;
    this.userFeedbackService
      .toggleReviewed(feedback)
      .then(sucessfulInvites => {
        this.showFeedback = true;
        this.bannerFeedback = {
          type: ResultType.Success,
          title: "Feedback Reviewed",
          message: "Successfully Reviewed User Feedback."
        };
        this.loadFeedbackList();
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
