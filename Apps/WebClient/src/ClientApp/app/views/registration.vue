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
    <LoadingComponent :is-loading="isLoading"></LoadingComponent>
    <div v-if="!isLoading">
      <b-row class="my-3">
        <b-col>
          <b-alert :show="hasErrors" dismissible variant="danger">
            <h4>Error</h4>
            <span
              >An unexpected error occured while processing the request.</span
            >
          </b-alert>
          <div id="pageTitle">
            <h1 id="Subject">
              Terms of Service
            </h1>
            <h3 id="Description">
              {{ fullName }}, you can provide your email to be informed about
              changes in your records, notifications, ...(TODO)
            </h3>
          </div>
        </b-col>
      </b-row>
      <b-form ref="registrationForm" @submit.prevent="onSubmit">
        <b-row class="mb-3">
          <b-col>
            <b-form-input
              id="email"
              v-model="$v.email.$model"
              type="email"
              placeholder="Your email address"
              :disabled="emailOptout"
              :state="isValid($v.email)"
            />
            <b-form-invalid-feedback :state="isValid($v.email)">
              Valid email is required
            </b-form-invalid-feedback>
          </b-col>
        </b-row>
        <b-row class="mb-5">
          <b-col>
            <b-form-input
              id="emailConfirmation"
              v-model="$v.emailConfirmation.$model"
              type="email"
              placeholder="Confirm your email address"
              :disabled="emailOptout"
              :state="isValid($v.emailConfirmation)"
            />
            <b-form-invalid-feedback :state="$v.emailConfirmation.sameAsEmail">
              Emails must match
            </b-form-invalid-feedback>
          </b-col>
        </b-row>
        <b-row class="mb-5">
          <b-col>
            <b-form-checkbox
              id="optout"
              v-model="emailOptout"
              class="optout"
              @change="optOutChanged($event)"
            >
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
            <b-form-checkbox
              id="accept"
              v-model="accepted"
              class="accept"
              :state="isValid($v.accepted)"
            >
              I agree to the terms of service above.
            </b-form-checkbox>
            <b-form-invalid-feedback :state="isValid($v.accepted)">
              Field is required.
            </b-form-invalid-feedback>
          </b-col>
        </b-row>
        <b-row class="mb-5">
          <b-col class="justify-content-right">
            <b-button class="px-5 float-right" type="submit" size="lg"
              >Register</b-button
            >
          </b-col>
        </b-row>
      </b-form>
    </div>
  </b-container>
</template>

<script lang="ts">
import Vue from "vue";
import { Getter, Action } from "vuex-class";
import { Component, Ref } from "vue-property-decorator";
import {
  IUserProfileService,
  IAuthenticationService
} from "@/services/interfaces";
import SERVICE_IDENTIFIER from "@/constants/serviceIdentifiers";
import container from "@/inversify.config";
import User from "@/models/user";
import { required, requiredIf, sameAs, email } from "vuelidate/lib/validators";
import LoadingComponent from "@/components/loading.vue";

@Component({
  components: {
    LoadingComponent
  }
})
export default class RegistrationComponent extends Vue {
  @Action("checkRegistration", { namespace: "user" }) checkRegistration;
  @Ref("registrationForm") form!: HTMLFormElement;

  private termsOfService: string =
    "The personal information you provide will be used to connect your Health Gateway account to your BC Services Card account. This will be done through the BC Services Identity Assurance Service. Once your Health Gateway account is authenticated, you will be able to view your health records from various health information systems in one place. Health Gateway’s collection of your personal information is in compliance with BC Privacy legislation under section 26(c) of the Freedom of Information and Protection of Privacy Act. \n\n If you have any questions about our collection or use of personal information, please direct your inquiries to the Nino Samson. ";
  private emailOptout: boolean = false;
  private accepted: boolean = false;
  private email: string = "";
  private emailConfirmation: string = "";
  private oidcUser: any = {};
  private userProfileService: IUserProfileService;
  private submitStatus: string = "";
  private validate: boolean;
  private isLoading: boolean = false;
  private hasErrors: boolean = false;

  mounted() {
    this.isLoading = true;
    this.validate = false;

    this.userProfileService = container.get(
      SERVICE_IDENTIFIER.UserProfileService
    );

    // Load the user name
    var authenticationService: IAuthenticationService = container.get(
      SERVICE_IDENTIFIER.AuthenticationService
    );
    authenticationService
      .getOidcUserProfile()
      .then(oidcUser => {
        if (oidcUser) {
          this.oidcUser = oidcUser;
        }
      })
      .catch(() => {
        this.hasErrors = true;
      })
      .finally(() => {
        this.isLoading = false;
      });
  }

  validations() {
    return {
      email: {
        required: requiredIf(() => {
          return !this.emailOptout;
        }),
        email
      },
      emailConfirmation: {
        required: requiredIf(() => {
          return !this.emailOptout;
        }),
        sameAsEmail: sameAs("email"),
        email
      },
      accepted: { isChecked: sameAs(() => true) }
    };
  }

  private isValid(param: any): boolean | undefined {
    return param.$dirty ? !param.$invalid : undefined;
  }

  private optOutChanged(isChecked: boolean) {
    if (isChecked) {
      this.email = "";
      this.emailConfirmation = "";
    }
  }

  private onSubmit(event: any) {
    console.log("submit!");
    console.log(this.$v);
    this.$v.$touch();
    if (this.$v.$invalid) {
      this.submitStatus = "ERROR";
    } else {
      this.submitStatus = "PENDING";

      this.isLoading = true;
      this.userProfileService
        .createProfile({
          hdid: this.oidcUser.hdid,
          acceptedTermsOfService: this.accepted,
          email: this.email
        })
        .then(result => {
          console.log(result);
          this.checkRegistration({ hdid: this.oidcUser.hdid }).then(
            isRegistered => {
              if (isRegistered) {
                this.$router.push({ path: "/timeline" });
              } else {
                this.hasErrors = true;
              }
            }
          );
        })
        .catch(() => {
          this.hasErrors = true;
        })
        .finally(() => {
          this.isLoading = false;
        });
    }

    event.preventDefault();
  }

  get fullName(): string {
    return this.oidcUser.given_name + " " + this.oidcUser.family_name;
  }
}
</script>
