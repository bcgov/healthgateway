<template>
  <v-container>
    <LoadingComponent :is-loading="isLoading"></LoadingComponent>
    <BannerFeedbackComponent
      :show-feedback.sync="showFeedback"
      :feedback="bannerFeedback"
      class="mt-5"
    ></BannerFeedbackComponent>
    <v-form ref="form" v-model="valid">
      <v-row>
        <v-col md="9">
          <v-row>
            <v-col>
              <v-text-field
                v-model="subject"
                label="Subject"
                maxlength="100"
                :rules="[v => !!v || 'Subject is required']"
                required
              ></v-text-field>
            </v-col>
          </v-row>
          <v-row>
            <v-col>
              <v-textarea
                v-model="text"
                label="Communication Text"
                maxlength="1000"
                :rules="[v => !!v || 'Text is required']"
                required
              ></v-textarea>
            </v-col>
          </v-row>
          <v-row>
              <v-col>
                  <v-datetime-picker v-model="effectiveDateTime"
                                     requried
                                     label="Effective On"></v-datetime-picker>
              </v-col>
              <v-col>
                  <v-datetime-picker v-model="expiryDateTime"
                                     required
                                     label="Expires On"></v-datetime-picker>
              </v-col>
          </v-row>
          <v-row justify="end" no-gutters>
            <v-btn :disabled="!valid" @click="add()">Add</v-btn>
          </v-row>
        </v-col>
      </v-row>
    </v-form>
  </v-container>
</template>

<script lang="ts">
import { Component, Vue } from "vue-property-decorator";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import LoadingComponent from "@/components/core/Loading.vue";
import BannerFeedbackComponent from "@/components/core/BannerFeedback.vue";
import BannerFeedback from "@/models/bannerFeedback";
import { ResultType } from "@/constants/resulttype";
import { ICommunicationService } from "@/services/interfaces";

@Component({
  components: {
    LoadingComponent,
    BannerFeedbackComponent
  }
})
export default class CommunicationView extends Vue {
  private valid: boolean = false;
  private subject: string = "";
  private text: string = "";
  private effectiveDateTime: Date = new Date();
  private expiryDateTime: Date = new Date();
  private isLoading: boolean = false;
  private showFeedback: boolean = false;
  private bannerFeedback: BannerFeedback = {
    type: ResultType.NONE,
    title: "",
    message: ""
  };

  private communicationService!: ICommunicationService;

  mounted() {
    this.communicationService = container.get(
      SERVICE_IDENTIFIER.CommunicationService
    );
    this.clearForm();
  }

  private clearForm() {
    this.subject = "";
    this.text = "";
    this.effectiveDateTime = new Date();
    this.expiryDateTime = new Date();
    this.expiryDateTime.setDate(this.effectiveDateTime.getDate() + 1);
  }

  private add(): void {
    this.isLoading = true;

    this.communicationService
      .add({
        subject: this.subject,
        text: this.text,
        effectiveDateTime: this.effectiveDateTime,
        expiryDateTime: this.expiryDateTime
      })
      .then(() => {
        this.clearForm();
        this.showFeedback = true;
        this.bannerFeedback = {
          type: ResultType.Success,
          title: "Success",
          message: "Communication Added."
        };
      })
      .catch(err => {
        this.showFeedback = true;
        this.bannerFeedback = {
          type: ResultType.Error,
          title: "Error",
          message: "Add communication failed, please try again"
        };
      })
      .finally(() => {
        this.isLoading = false;
      });
  }
}
</script>
