<style lang="scss">
@import "@/assets/scss/_variables.scss";

#pageTitle {
  color: $primary;
}

#Description {
  font-size: 1.2em;
}

input {
  width: 320px !important;
  max-width: 320px !important;
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
    <div v-if="!isLoading" class="py-5">
      <b-row v-if="isRegistrationClosed">
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
      <b-row v-else-if="isRegistrationInviteOnly && !inviteKey">
        <b-col>
          <div id="pageTitle">
            <h1 id="Subject">
              Enter your email to join the waitlist
            </h1>
            <hr />
          </div>
          <b-row class="mb-3">
            <b-col>
              <label for="waitlistNames" class="font-weight-bold"
                >Full Name</label
              >
              <b-row
                ><b-col id="waitlistNames">
                  {{ fullName }}
                </b-col>
                <b-col
                  id="authenticatedLabel"
                  class="ml-auto text-right text-success"
                  >Authenticated
                  <font-awesome-icon
                    icon="check"
                    aria-hidden="true"
                  ></font-awesome-icon>
                </b-col>
              </b-row>
            </b-col>
          </b-row>
          <b-row class="mb-3">
            <b-col>
              <label for="waitlistEditEmail" class="font-weight-bold"
                >Email Address</label
              >
              <b-button
                v-if="!waitlistEdditable && waitlistTempEmail"
                id="waitlistEditEmail"
                class="mx-auto"
                variant="link"
                @click="makeWaitlistEdditable()"
                >Edit
              </b-button>
              <div class="form-inline">
                <b-form-input
                  id="waitlistEmail"
                  v-model="$v.email.$model"
                  type="email"
                  placeholder="Your email address"
                  :disabled="!waitlistEdditable"
                  :state="isValid($v.email)"
                />
                <div
                  v-if="!waitlistEdditable && waitlistTempEmail"
                  id="authenticatedLabel"
                  class="ml-auto text-right text-warning"
                >
                  Waitlisted
                </div>
              </div>
              <b-form-invalid-feedback :state="isValid($v.email)">
                Valid email is required
              </b-form-invalid-feedback>
              <b-form-invalid-feedback :state="$v.email.newEmail">
                New email must be different from the previous one
              </b-form-invalid-feedback>
            </b-col>
          </b-row>
          <b-row v-if="waitlistEdditable" class="mb-3">
            <b-col>
              <b-form-input
                id="waitlistEmailConfirmation"
                v-model="$v.emailConfirmation.$model"
                type="email"
                placeholder="Confirm your email address"
                :state="isValid($v.emailConfirmation)"
              />
              <b-form-invalid-feedback
                :state="$v.emailConfirmation.sameAsEmail"
              >
                Emails must match
              </b-form-invalid-feedback>
            </b-col>
          </b-row>
          <b-row v-if="waitlistEdditable" class="mb-3 justify-content-end">
            <b-col class="text-right">
              <b-button
                v-if="waitlistTempEmail"
                id="cancelBtn"
                class="mx-2 actionButton"
                @click="cancelWaitlistEdit()"
                >Cancel
              </b-button>
              <b-button
                id="saveBtn"
                variant="primary"
                class="mx-2 actionButton"
                :disabled="waitlistTempEmail === email"
                @click="saveWaitlistEdit()"
                ><span v-if="waitlistTempEmail">Save</span>
                <span v-else>Join Waitlist</span>
              </b-button>
            </b-col>
          </b-row>
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
              :disabled="emailOptout || isPredefinedEmail"
              :state="isValid($v.email)"
            />
            <b-form-invalid-feedback :state="isValid($v.email)">
              Valid email is required
            </b-form-invalid-feedback>
          </b-col>
        </b-row>
        <b-row v-if="!isPredefinedEmail" class="mb-3">
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
        <b-row v-if="!isRegistrationInviteOnly" class="mb-3">
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
            <b-button
              class="px-5 float-right"
              type="submit"
              size="lg"
              variant="primary"
              :class="{ disabled: !accepted }"
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
  IAuthenticationService,
  IBetaRequestService
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
import BetaRequest from "@/models/betaRequest";
import { library } from "@fortawesome/fontawesome-svg-core";
import { faCheck } from "@fortawesome/free-solid-svg-icons";
library.add(faCheck);

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
  @Prop() inviteEmail?: string;

  private readonly termsOfService: string = termsAndConditionsHTML;
  private emailOptout: boolean = false;
  private accepted: boolean = false;
  private email: string = "";
  private emailConfirmation: string = "";

  private oidcUser: any = {};
  private userProfileService: IUserProfileService;
  private submitStatus: string = "";
  private isLoading: boolean = true;
  private hasErrors: boolean = false;
  private errorMessage: string = "";

  private betaRequestService: IBetaRequestService;
  private waitlistEdditable = true;
  private waitlistTempEmail: string = "";
  private waitlistEmail: string = "";
  private waitlistEmailConfirmation: string = "";

  mounted() {
    this.betaRequestService = container.get(
      SERVICE_IDENTIFIER.BetaRequestService
    );

    if (this.webClientConfig.registrationStatus == RegistrationStatus.Open) {
      this.email = "";
      this.emailConfirmation = "";
    } else {
      this.email = this.inviteEmail || "";
      this.emailConfirmation = this.email;
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

          this.betaRequestService
            .getRequest(this.oidcUser.hdid)
            .then(betaRequest => {
              console.log("beta request:", betaRequest);
              if (betaRequest) {
                this.email = betaRequest.emailAddress;
                this.waitlistTempEmail = this.email;
                this.waitlistEdditable = false;
              } else {
                this.makeWaitlistEdditable();
              }
            })
            .catch(() => {
              this.hasErrors = true;
            })
            .finally(() => {
              this.isLoading = false;
            });
        } else {
          this.isLoading = false;
        }
      })
      .catch(() => {
        this.hasErrors = true;
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

  get fullName(): string {
    return this.oidcUser.given_name + " " + this.oidcUser.family_name;
  }
  get isRegistrationClosed(): boolean {
    return this.webClientConfig.registrationStatus == RegistrationStatus.Closed;
  }
  get isRegistrationInviteOnly(): boolean {
    return (
      this.webClientConfig.registrationStatus == RegistrationStatus.InviteOnly
    );
  }
  get isPredefinedEmail() {
    if (this.webClientConfig.registrationStatus != RegistrationStatus.Open) {
      return !!this.inviteEmail;
    }
    return false;
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
            (isRegistered: boolean) => {
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

  private makeWaitlistEdditable(): void {
    this.waitlistEdditable = true;
    this.waitlistTempEmail = this.email || "";
  }

  private saveWaitlistEdit(): void {
    this.$v.$touch();
    console.log(this.$v);
    if (this.$v.email.$invalid || this.$v.emailConfirmation.$invalid) {
      this.submitStatus = "ERROR";
    } else {
      this.submitStatus = "PENDING";

      console.log(this.email);

      let newRequest: BetaRequest = {
        hdid: this.oidcUser.hdid,
        emailAddress: this.email
      };

      this.betaRequestService
        .putRequest(newRequest)
        .then(result => {
          console.log("success!");
          console.log(result);
          this.waitlistEdditable = false;
          this.waitlistEmailConfirmation = "";
          this.waitlistTempEmail = this.email;
          this.$v.$reset();
        })
        .catch(() => {
          this.hasErrors = true;
        })
        .finally(() => {
          this.isLoading = false;
        });
    }
  }

  private cancelWaitlistEdit(): void {
    this.waitlistEdditable = false;
    this.email = this.waitlistTempEmail;
    this.waitlistEmailConfirmation = "";
    this.$v.$reset();
  }
}
</script>
