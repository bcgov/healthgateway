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
            <b-alert :show="limitedRegistration" variant="info">
              <h4>Limited registration</h4>
              <span
                >Currently the registration is
                <strong>{{ config.registrationStatus }}</strong
                >.</span
              >
            </b-alert>
            <div id="Description">
              <strong>{{ fullName }}</strong>
              , please provide your email address to receive notifications about
              updates to the Health Gateway, such as new features and changes.
            </div>
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
        <b-row class="mb-3">
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
        <b-row
          v-if="webClientConfig.registrationStatus == inviteOnlyRegistration"
          class="mb-3"
        >
          <b-col>
            <b-form-input
              id="inviteKey"
              v-model="inviteKey"
              placeholder="Invitation Key"
              :state="isValid($v.inviteKey)"
              readonly
            />
            <b-form-invalid-feedback :state="isValid($v.inviteKey)">
              Invitation key is required.
            </b-form-invalid-feedback>
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
  @Getter("webClient", { namespace: "config" }) config: WebClientConfiguration;
  @Action("checkRegistration", { namespace: "user" }) checkRegistration;
  @Ref("registrationForm") form!: HTMLFormElement;
  @Getter("webClient", { namespace: "config" })
  webClientConfig: WebClientConfiguration;
  @Prop() inviteKey?: string;

  private termsOfService: string = termsAndConditionsHTML;
  private emailOptout: boolean = false;
  private accepted: boolean = false;
  private email: string = "";
  private emailConfirmation: string = "";
  private oidcUser: any = {};
  private userProfileService: IUserProfileService;
  private submitStatus: string = "";
  private isLoading: boolean = true;
  private hasErrors: boolean = false;
  private inviteOnlyRegistration: RegistrationStatus =
    RegistrationStatus.InviteOnly;
  private openRegistration: RegistrationStatus = RegistrationStatus.Open;
  private limitedRegistration: boolean = false;

  mounted() {
    if (this.config.registrationStatus !== RegistrationStatus.Open) {
      this.limitedRegistration = true;
    }

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
      accepted: { isChecked: sameAs(() => true) },
      inviteKey: {
        required: requiredIf(() => {
          return (
            this.webClientConfig.registrationStatus ==
            this.inviteOnlyRegistration
          );
        })
      }
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
            email: this.email
          },
          inviteCode: this.inviteKey
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
