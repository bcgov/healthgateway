<style lang="scss">
@import "@/assets/scss/_variables.scss";

#pageTitle {
  color: $primary;
}
#pageTitle hr {
  border-top: 2px solid $primary;
}
input {
  max-width: 320px;
}
</style>
<template>
  <b-container>
    <b-row class="my-3">
      <b-col>
        <div id="pageTitle">
          <h1 id="Subject">Registration</h1>
          <hr />
        </div>
      </b-col>
    </b-row>
    <b-form ref="registrationForm" @submit="onSubmit">
      <b-row>
        <b-col>
          <b-form-group label="Email:" label-for="email">
            <b-form-input
              id="email"
              v-model="email"
              type="email"
              placeholder="Enter your email"
              required
            />
          </b-form-group>
        </b-col>
      </b-row>
      <b-row>
        <b-col>
          <b-form-group
            label="Email Confirmation:"
            label-for="emailConfirmation"
          >
            <b-form-input
              id="emailConfirmation"
              v-model="emailConfirmation"
              type="email"
              placeholder="Reenter your email"
              required
            />
            <b-form-invalid-feedback id="emailConfirmationFeedback">
              This is a required field and must be at least 3 characters.
            </b-form-invalid-feedback>
          </b-form-group>
        </b-col>
      </b-row>
      <b-row>
        <b-col>
          <b-form-group label="Terms of Service:" label-for="textarea">
            <b-form-textarea
              id="textarea"
              v-model="termsOfService"
              placeholder="Terms of service..."
              readonly
              rows="10"
              max-rows="10"
            />
          </b-form-group>
        </b-col>
      </b-row>
      <b-row>
        <b-col>
          <b-form-checkbox id="accept" v-model="accepted" required
            >I accept the terms of service above.</b-form-checkbox
          >
        </b-col>
      </b-row>
      <b-row>
        <b-col>
          <b-button type="submit" :disabled="!isValid()">Submit</b-button>
        </b-col>
      </b-row>
    </b-form>
  </b-container>
</template>

<script lang="ts">
import Vue from "vue";
import { Component, Ref } from "vue-property-decorator";

@Component()
export default class RegistrationComponent extends Vue {
  private termsOfService: string =
    "The personal information you provide will be used to connect your Health Gateway account to your BC Services Card account. This will be done through the BC Services Identity Assurance Service. Once your Health Gateway account is authenticated, you will be able to view your health records from various health information systems in one place. Health Gateway’s collection of your personal information is in compliance with BC Privacy legislation under section 26(c) of the Freedom of Information and Protection of Privacy Act. \n\n If you have any questions about our collection or use of personal information, please direct your inquiries to the Nino Samson. ";
  private accepted: boolean = false;
  private email: string = "";
  private emailConfirmation: string = "";
  @Ref("registrationForm") form!: HTMLFormElement;

  isValid() {
    return this.accepted && this.email && this.email == this.emailConfirmation;
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
