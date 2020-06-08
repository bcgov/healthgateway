<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";
.title {
  color: $primary;
  font-size: 2.1em;
}
</style>
<template>
  <b-container>
    <b-row class="pt-5">
      <b-col class="text-center mb-5">
        <h4 v-if="isLoading" class="title">
          We are verifying your email...
        </h4>
        <h4 v-if="!isLoading && isSuccess === true" class="text-success">
          Your email was successfully verified!
        </h4>
        <h4 v-if="!isLoading && isSuccess === false" class="text-danger">
          Something is not right, are you sure this is the correct link?
        </h4>
      </b-col>
    </b-row>
    <b-row class="pt-5">
      <b-col class="text-center mb-5">
        <b-spinner v-if="isLoading"></b-spinner>
        <span v-if="!isLoading && isSuccess === true" class="text-success"
          ><font-awesome-icon icon="check-circle" size="10x"></font-awesome-icon
        ></span>
        <span v-if="!isLoading && isSuccess === false" class="text-danger"
          ><font-awesome-icon icon="times-circle" size="10x"></font-awesome-icon
        ></span>
      </b-col>
    </b-row>
  </b-container>
</template>
<script lang="ts">
import Vue from "vue";
import { Component, Prop } from "vue-property-decorator";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { IUserProfileService } from "@/services/interfaces";
import { Action, Getter } from "vuex-class";
import container from "@/plugins/inversify.config";
import User from "@/models/user";
import { library } from "@fortawesome/fontawesome-svg-core";
import { faTimesCircle } from "@fortawesome/free-solid-svg-icons";
library.add(faTimesCircle);

@Component
export default class ValidateEmailComponent extends Vue {
  @Prop() inviteKey!: string;
  @Getter("user", { namespace: "user" }) user!: User;
  @Action("checkRegistration", { namespace: "user" }) checkRegistration!: ({
    hdid: String,
  }: any) => Promise<boolean>;
  private isLoading: boolean = false;
  private isSuccess: boolean | null = null;

  mounted() {
    this.isLoading = true;
    const userProfileService: IUserProfileService = container.get(
      SERVICE_IDENTIFIER.UserProfileService
    );

    userProfileService
      .validateEmail(this.user.hdid, this.inviteKey)
      .then((isValid) => {
        this.isSuccess = isValid;
        if (isValid) {
          this.checkRegistration({ hdid: this.user.hdid });
          setTimeout(() => this.$router.push({ path: "/timeline" }), 2000);
        }
      })
      .finally(() => {
        this.isLoading = false;
      });
  }
}
</script>
