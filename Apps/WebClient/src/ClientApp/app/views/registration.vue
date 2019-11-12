<style lang="scss">
@import "@/assets/scss/_variables.scss";

#pageTitle {
  color: $primary;
}

input {
  max-width: 320px;
}

.accept label {
  color: $primary;
}

.optout label {
  color: $soft_text;
}

#termsOfService {
  background-color: $light_background;
  color: $soft_text;
}
</style>
<template>
  <b-container>
    <b-row class="my-3">
      <b-col>
        <div id="pageTitle">
          <h1 id="Subject">
            {{ user.firstName }}, you can provide your email to be informed
            about changes in your records, notifications, ...
          </h1>
        </div>
      </b-col>
    </b-row>
    <b-form ref="registrationForm" @submit="onSubmit">
      <b-row class="mb-3">
        <b-col>
          <b-form-input
            id="email"
            v-model="email"
            type="email"
            placeholder="Your email address"
            :disabled="optout"
          />
        </b-col>
      </b-row>
      <b-row class="mb-5">
        <b-col>
          <b-form-input
            id="emailConfirmation"
            v-model="emailConfirmation"
            type="email"
            placeholder="Confirm your email address"
            :disabled="optout"
          />
        </b-col>
      </b-row>
      <b-row class="mb-5">
        <b-col>
          <b-form-checkbox id="optout" v-model="optout" class="optout">
            No, I prefer not to receive any notifications about my health
            records
          </b-form-checkbox>
        </b-col>
      </b-row>
      <b-row class="mb-3">
        <b-col>
          <b-form-textarea
            id="termsOfService"
            v-model="termsOfService"
            class="px-3 py-5"
            placeholder="Terms of service..."
            readonly
            rows="20"
            max-rows="20"
          />
        </b-col>
      </b-row>
      <b-row class="mb-3">
        <b-col>
          <b-form-checkbox id="accept" v-model="accepted" class="accept">
            I agree to the terms of service above.
          </b-form-checkbox>
        </b-col>
      </b-row>
      <b-row class="mb-5">
        <b-col class="justify-content-right">
          <b-button
            class="px-5 float-right"
            type="submit"
            :disabled="!isValid()"
            size="lg"
            >Register</b-button
          >
        </b-col>
      </b-row>
    </b-form>
  </b-container>
</template>

<script lang="ts">
import Vue from "vue";
import { Getter } from "vuex-class";
import { Component, Ref } from "vue-property-decorator";
import User from "@/models/user";

@Component
export default class RegistrationComponent extends Vue {
  private termsOfService: string =
    "The personal information you provide will be used to connect your Health Gateway account to your BC Services Card account. This will be done through the BC Services Identity Assurance Service. Once your Health Gateway account is authenticated, you will be able to view your health records from various health information systems in one place. Health Gateway’s collection of your personal information is in compliance with BC Privacy legislation under section 26(c) of the Freedom of Information and Protection of Privacy Act. \n\n If you have any questions about our collection or use of personal information, please direct your inquiries to the Nino Samson. ";
  private optout: boolean = false;
  private accepted: boolean = false;
  private email: string = "";
  private emailConfirmation: string = "";
  @Ref("registrationForm") form!: HTMLFormElement;
  @Getter("user", { namespace: "user" }) user: User;

  isValid() {
    return (
      this.accepted &&
      ((this.email && this.email == this.emailConfirmation) || this.optout)
    );
  }

  mounted() {
    console.log("Registration mounted");
    console.log(this.form);
  }

  onSubmit() {
    console.log("OnSubmit registration");
  }
}
</script>
