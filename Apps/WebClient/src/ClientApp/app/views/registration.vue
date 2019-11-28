<style lang="scss">
@import "@/assets/scss/_variables.scss";

#pageTitle {
  color: $primary;
}

#Description {
  font-size: 1.2em;
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
      <b-row
        v-if="webClientConfig.registrationStatus == closedRegistration"
        class="my-3"
      >
        <b-col>
          <div id="pageTitle">
            <h1 id="Subject">
              Closed Registration
            </h1>
            <div id="Description">
              Thank you for your interest in the Health Gateway service. At this
              time, the registration is closed.
            </div>
          </div>
        </b-col>
      </b-row>
      <b-row
        v-else-if="
          webClientConfig.registrationStatus == inviteOnlyRegistration &&
            !inviteKey
        "
        class="my-3"
      >
        <b-col>
          <div id="pageTitle">
            <h1 id="Subject">
              Restricted Registration
            </h1>
            <div id="Description">
              Thank you for your interest in the Health Gateway service. At this
              time, the registration is invite only. As we will be launching
              more broadly in the coming months, please visit the site again. If
              you are one of our patient partners, check your email for your
              unique registration link.
            </div>
          </div>
        </b-col>
      </b-row>
      <b-form v-else ref="registrationForm" @submit.prevent="onSubmit">
        <b-row class="my-3">
          <b-col>
            <b-alert :show="hasErrors" dismissible variant="danger">
              <h4>Error</h4>
              <p>An unexpected error occured while processing the request:</p>
              <span>{{ errorMessage }}</span>
            </b-alert>
            <div id="pageTitle">
              <h1 id="Subject">
                Terms of Service
              </h1>
              <div id="Description">
                <strong>{{ fullName }}</strong>
                , please provide your email address to receive notifications
                about updates to the Health Gateway, such as new features and
                changes.
              </div>
            </div>
          </b-col>
        </b-row>
        <b-row class="mb-3">
          <b-col>
            <b-form-input
              id="email"
              v-model="$v.email.$model"
              type="email"
              placeholder="Your email address"
              :disabled="emailOptout || predefinedEmail"
              :state="isValid($v.email)"
            />
            <b-form-invalid-feedback :state="isValid($v.email)">
              Valid email is required
            </b-form-invalid-feedback>
          </b-col>
        </b-row>
        <b-row v-if="!predefinedEmail" class="mb-3">
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
        <b-row
          v-if="webClientConfig.registrationStatus == openRegistration"
          class="mb-3"
        >
          <b-col>
            <b-form-checkbox
              id="optout"
              v-model="emailOptout"
              class="optout"
              @change="optOutChanged($event)"
            >
              No, I prefer not to receive any notifications
            </b-form-checkbox>
          </b-col>
        </b-row>
        <b-row class="mb-3">
          <b-col>
            <HtmlTextAreaComponent :input="termsOfService" />
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
              You must accept the terms of service.
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
import { Component, Ref, Prop } from "vue-property-decorator";
import {
  IUserProfileService,
  IAuthenticationService
} from "@/services/interfaces";
import SERVICE_IDENTIFIER from "@/constants/serviceIdentifiers";
import container from "@/inversify.config";
import User from "@/models/user";
import { required, requiredIf, sameAs, email } from "vuelidate/lib/validators";
import { RegistrationStatus } from "@/constants/registrationStatus";
import LoadingComponent from "@/components/loading.vue";
import HtmlTextAreaComponent from "@/components/htmlTextarea.vue";
import termsAndConditionsHTML from "@/assets/docs/termsAndConditions.html";
import { WebClientConfiguration } from "@/models/configData";

@Component({
  components: {
    LoadingComponent,
    HtmlTextAreaComponent
  }
})
export default class RegistrationComponent extends Vue {
  @Action("checkRegistration", { namespace: "user" }) checkRegistration;
  @Ref("registrationForm") form!: HTMLFormElement;
  @Getter("webClient", { namespace: "config" })
  webClientConfig: WebClientConfiguration;
  @Prop() inviteKey?: string;
  @Prop() email?: string;

  private termsOfService: string = termsAndConditionsHTML;
  private emailOptout: boolean = false;
  private accepted: boolean = false;
  private emailConfirmation: string = "";
  private oidcUser: any = {};
  private userProfileService: IUserProfileService;
  private submitStatus: string = "";
  private isLoading: boolean = true;
  private hasErrors: boolean = false;
  private errorMessage: string = "";
  private predefinedEmail: boolean = false;
  private inviteOnlyRegistration: RegistrationStatus =
    RegistrationStatus.InviteOnly;
  private openRegistration: RegistrationStatus = RegistrationStatus.Open;
  private closedRegistration: RegistrationStatus = RegistrationStatus.Closed;

  mounted() {
    if (this.webClientConfig.registrationStatus == RegistrationStatus.Open) {
      this.email = "";
      this.inviteKey = "";
    }

    this.emailConfirmation = this.email || "";
    this.predefinedEmail = !!this.email;
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
          profile: {
            hdid: this.oidcUser.hdid,
            acceptedTermsOfService: this.accepted,
            email: this.email || ""
          },
          inviteCode: this.inviteKey || ""
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
        .catch(err => {
          this.hasErrors = true;
          this.errorMessage = err;
          console.log(err);
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
