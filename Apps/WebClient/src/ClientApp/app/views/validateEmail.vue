<style lang="scss">
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
          ><i class="fa fa-check-circle fa-10x"></i
        ></span>
        <span v-if="!isLoading && isSuccess === false" class="text-danger"
          ><i class="fa fa-times-circle fa-10x"></i
        ></span>
      </b-col>
    </b-row>
  </b-container>
</template>
<script lang="ts">
import Vue from "vue";
import { Component, Prop } from "vue-property-decorator";
import SERVICE_IDENTIFIER from "@/constants/serviceIdentifiers";
import { IUserEmailService } from "@/services/interfaces";
import { Action, Getter } from "vuex-class";
import container from "@/inversify.config";
import User from "@/models/user";

@Component
export default class ValidateEmailComponent extends Vue {
  @Prop() inviteKey!: string;
  @Getter("user", { namespace: "user" }) user: User;
  @Action("checkRegistration", { namespace: "user" }) checkRegistration;
  private isLoading: boolean = false;
  private isSuccess: boolean | null = null;

  mounted() {
    this.isLoading = true;
    const userEmailService: IUserEmailService = container.get(
      SERVICE_IDENTIFIER.UserEmailService
    );

    userEmailService
      .validateEmail(this.inviteKey)
      .then(isValid => {
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
