<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";
.overlay {
  position: fixed;
  bottom: 0;
  right: 0.2%;
  width: 300px;
  height: 270px;

  -webkit-transition: all 600ms cubic-bezier(0.19, 1, 0.22, 1);
  transition: all 600ms cubic-bezier(0.19, 1, 0.22, 1);

  z-index: 1;
}

.overlay-tray {
  bottom: -240px;
}

.overlay-closed {
  bottom: -400px;
}
#collapse {
  background-color: $soft_background;
}
#comment {
  resize: none;
}
</style>

<template>
  <b-container
    class="d-flex flex-column overlay overlay-empty"
    :class="!visible ? 'overlay-tray' : null"
  >
    <b-button
      class="justify-content-center pr-2"
      :aria-expanded="visible ? 'true' : 'false'"
      aria-controls="collapse"
      size="sm"
      variant="primary"
      @click="visible = !visible"
    >
      <span>
        <font-awesome-icon
          icon="comments"
          aria-hidden="true"
        ></font-awesome-icon>
        Feedback?
      </span>
    </b-button>
    <b-collapse id="collapse" v-model="visible" class="border">
      <div class="pb-5">
        <b-row class="my-2 px-2">
          <b-col
            ><span class="small text-center d-flex justify-content-center"
              >Are you satisfied with the information provided to you?</span
            ></b-col
          >
        </b-row>
        <b-form ref="feedbackForm" @submit.prevent="onSubmit">
          <b-row class="my-1 px-3">
            <b-col class="d-flex justify-content-center p-1 ">
              <b-button
                :variant="
                  isSatisfied === true ? 'primary' : 'outline-secondary'
                "
                size="sm"
                class="w-100"
                :disabled="isSuccess || isLoading"
                @click="isSatisfied = true"
                >Yes</b-button
              >
            </b-col>
            <b-col class="d-flex justify-content-center p-1">
              <b-button
                :variant="
                  isSatisfied === false ? 'primary' : 'outline-secondary'
                "
                size="sm"
                class="w-100"
                :disabled="isSuccess || isLoading"
                @click="isSatisfied = false"
                >No</b-button
              >
            </b-col>
          </b-row>
          <b-row class="px-1">
            <b-col>
              <b-form-textarea
                id="comment"
                v-model="comment"
                size="sm"
                placeholder="Comments..."
                rows="3"
                max-rows="3"
                maxlength="500"
                :disabled="isSuccess || isLoading"
              />
            </b-col>
          </b-row>
          <b-row class="mt-3">
            <b-col class="d-flex justify-content-center">
              <b-button
                v-if="!isSuccess && !isLoading"
                variant="primary"
                size="sm"
                class="px-5"
                type="submit"
                :disabled="isSatisfied === null"
                >Submit</b-button
              >
              <b-spinner v-if="isLoading"></b-spinner>
              <p v-if="isSuccess" class="text-success">Thank you!</p>
            </b-col>
          </b-row>
          <b-row v-if="hasErrors">
            <b-col>
              <p class="small text-danger text-center">
                Ops Something is wrong! Please try again.
              </p>
            </b-col>
          </b-row>
        </b-form>
      </div>
    </b-collapse>
  </b-container>
</template>

<script lang="ts">
import Vue from "vue";
import { Component } from "vue-property-decorator";
import container from "@/plugins/inversify.config";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { IUserFeedbackService } from "@/services/interfaces";
import { library } from "@fortawesome/fontawesome-svg-core";
import { faComments } from "@fortawesome/free-solid-svg-icons";
library.add(faComments);

@Component
export default class FeedbackComponent extends Vue {
  private visible: boolean = false;
  private comment: string = "";
  private isSatisfied: boolean | null = null;
  private isSuccess: boolean = false;
  private hasErrors: boolean = false;
  private isLoading: boolean = false;
  private userFeedbackService!: IUserFeedbackService;

  mounted() {
    this.userFeedbackService = container.get(
      SERVICE_IDENTIFIER.UserFeedbackService
    );
  }
  private onSubmit(event: any) {
    this.isLoading = true;
    this.userFeedbackService
      .submitFeedback({
        isSatisfied: this.isSatisfied!,
        comment: this.comment
      })
      .then(result => {
        this.isSuccess = result;
        this.hasErrors = !this.isSuccess;
      })
      .catch(() => {
        this.hasErrors = true;
      })
      .finally(() => {
        this.isLoading = false;
      });

    event.preventDefault();
  }
}
</script>
