<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

#pageTitle {
  color: $primary;
}

#pageTitle hr {
  border-top: 2px solid $primary;
}

label {
  font-weight: bold;
}

input {
  width: 320px !important;
  max-width: 320px !important;
}

.actionButton {
  width: 80px;
}
</style>
<template>
  <div class="container">
    <LoadingComponent :is-loading="isLoading"></LoadingComponent>
    <div class="row py-5">
      <div class="col-lg-12 col-md-12">
        <b-alert :show="hasErrors" dismissible variant="danger">
          <h4>Error</h4>
          <span>An unexpected error occured while processing the request.</span>
        </b-alert>
        <div id="pageTitle">
          <h1 id="subject">
            Profile
          </h1>
          <hr />
        </div>
        <div v-if="isActiveProfile">
          <b-row class="mb-3">
            <b-col>
              <label for="profileNames">Full Name</label>
              <div id="profileNames">
                {{ fullName }}
              </div>
            </b-col>
          </b-row>
          <b-row class="mb-3">
            <b-col>
              <label for="lastLoginDate">Last Login Date</label>
              <div id="lastLoginDate">
                {{ lastLoginDateString }}
              </div>
            </b-col>
          </b-row>
          <b-row class="mb-3">
            <b-col>
              <label for="email">Email Address</label>
              <b-button
                v-if="!isEmailEditable"
                id="editEmail"
                class="mx-auto"
                variant="link"
                @click="makeEmailEditable()"
                >Edit
              </b-button>
              <b-button
                v-if="email"
                id="removeEmail"
                class="text-danger"
                variant="link"
                @click="
                  makeEmailEditable();
                  removeEmail();
                "
              >
                Remove
              </b-button>
              <div class="form-inline">
                <b-form-input
                  id="email"
                  v-model="$v.email.$model"
                  type="email"
                  :placeholder="
                    isEmailEditable ? 'Your email address' : 'Empty'
                  "
                  :disabled="!isEmailEditable"
                  :state="isValid($v.email)"
                />
                <div
                  v-if="!emailVerified && !isEmailEditable && email"
                  class="ml-3"
                >
                  (Not Verified)
                </div>
                <b-button
                  v-if="!emailVerified && !isEmailEditable && email"
                  id="resendEmail"
                  variant="warning"
                  class="ml-auto"
                  :disabled="verificationSent"
                  @click="sendUserEmailUpdate()"
                  >Resend Verification
                </b-button>
              </div>
              <b-form-invalid-feedback :state="isValid($v.email)">
                Valid email is required
              </b-form-invalid-feedback>
              <b-form-invalid-feedback :state="$v.email.newEmail">
                New email must be different from the previous one
              </b-form-invalid-feedback>
            </b-col>
          </b-row>
          <b-row v-if="isEmailEditable" class="mb-3">
            <b-col>
              <b-form-input
                id="emailConfirmation"
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
          <b-row v-if="!email && tempEmail">
            <b-col class="font-weight-bold text-primary text-center">
              <font-awesome-icon
                icon="exclamation-triangle"
                aria-hidden="true"
              ></font-awesome-icon>
              Removing your email address will disable future email
              communications from the Health Gateway
            </b-col>
          </b-row>
          <b-row v-if="isEmailEditable" class="mb-3 justify-content-end">
            <b-col class="text-right">
              <b-button
                id="cancelBtn"
                class="mx-2 actionButton"
                @click="cancelEmailEdit()"
                >Cancel
              </b-button>
              <b-button
                id="saveBtn"
                variant="primary"
                class="mx-2 actionButton"
                :disabled="tempEmail === email"
                @click="saveEmailEdit()"
                >Save
              </b-button>
            </b-col>
          </b-row>
          <b-row class="mb-3">
            <b-col>
              <label for="email">Cell Phone Number (SMS notifications)</label>
              <b-button
                v-if="!isPhoneEditable"
                id="editPhone"
                class="mx-auto"
                variant="link"
                @click="makePhoneEditable()"
                >Edit
              </b-button>
              <b-button
                v-if="phoneNumber"
                id="removePhone"
                class="text-danger"
                variant="link"
                @click="
                  makePhoneEditable();
                  removePhone();
                "
              >
                Remove
              </b-button>
              <div class="form-inline">
                <b-form-input
                  id="phoneNumber"
                  v-model="$v.phoneNumber.$model"
                  type="email"
                  :placeholder="isPhoneEditable ? 'Your phone number' : 'Empty'"
                  :disabled="!isPhoneEditable"
                  :state="isValid($v.phoneNumber)"
                />
              </div>
              <b-form-invalid-feedback :state="isValid($v.phoneNumber)">
                Valid phone number is required
              </b-form-invalid-feedback>
              <b-form-invalid-feedback :state="$v.phoneNumber.newPhoneNumber">
                New phone number must be different from the previous one
              </b-form-invalid-feedback>
            </b-col>
          </b-row>
          <b-row v-if="!phoneNumber && tempPhone">
            <b-col class="font-weight-bold text-primary text-center">
              <font-awesome-icon
                icon="exclamation-triangle"
                aria-hidden="true"
              ></font-awesome-icon>
              Removing your phone number will disable future SMS communications
              from the Health Gateway
            </b-col>
          </b-row>
          <b-row v-if="isPhoneEditable" class="mb-3 justify-content-end">
            <b-col class="text-right">
              <b-button
                id="cancelBtn"
                class="mx-2 actionButton"
                @click="cancelPhoneEdit()"
                >Cancel
              </b-button>
              <b-button
                id="saveBtn"
                variant="primary"
                class="mx-2 actionButton"
                :disabled="tempPhone === phoneNumber"
                @click="savePhoneEdit()"
                >Save
              </b-button>
            </b-col>
          </b-row>
        </div>
        <div v-else-if="!isLoading">
          <b-row class="mb-3">
            <b-col>
              <font-awesome-icon
                icon="exclamation-triangle"
                aria-hidden="true"
                class="text-danger"
              ></font-awesome-icon>
              <label for="deletionWarning">
                Account marked for removal
              </label>
              <div id="deletionWarning">
                Your account has been deactivated. If you wish to recover your
                account click on the "Recover Account" button before the time
                expires.
              </div>
            </b-col>
          </b-row>
          <b-row class="mb-3">
            <b-col>
              <label>Time remaining for deletion: </label>
              {{ timeForDeletionString }}
            </b-col>
          </b-row>
          <b-row class="mb-3">
            <b-col>
              <b-button
                id="recoverBtn"
                class="mx-auto"
                variant="warning"
                @click="recoverAccount()"
                >Recover Account
              </b-button>
            </b-col>
          </b-row>
        </div>
        <b-row class="mb-3">
          <b-col>
            <label>Other</label>
            <div>
              <b-button
                v-if="isActiveProfile && !showCloseWarning"
                id="showCloseWarningBtn"
                class="p-0 pt-2"
                variant="link"
                @click="showCloseWarningBtn()"
                >Close My Account
              </b-button>
              <b-row v-if="showCloseWarning">
                <b-col class="font-weight-bold text-danger text-center">
                  <hr />
                  <font-awesome-icon
                    icon="exclamation-triangle"
                    aria-hidden="true"
                  ></font-awesome-icon>
                  Your account will be marked for removal, preventing you from
                  accessing your information on the Health Gateway. After a set
                  period of time it will be removed permanently.
                </b-col>
              </b-row>
              <b-row v-if="showCloseWarning" class="mb-3 justify-content-end">
                <b-col class="text-right">
                  <b-button
                    id="cancelCloseBtn"
                    class="mx-2"
                    @click="cancelClose()"
                    >Cancel
                  </b-button>
                  <b-button
                    id="closeAccountBtn"
                    class="mx-2"
                    variant="danger"
                    @click="closeAccount()"
                    >Close Account
                  </b-button>
                </b-col>
              </b-row>
            </div>
          </b-col>
        </b-row>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import { Component } from "vue-property-decorator";
import LoadingComponent from "@/components/loading.vue";
import { Action, Getter } from "vuex-class";
import {
  required,
  requiredIf,
  sameAs,
  email,
  not,
  helpers
} from "vuelidate/lib/validators";
import {
  IUserProfileService,
  IAuthenticationService
} from "@/services/interfaces";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { User as OidcUser } from "oidc-client";
import User from "@/models/user";
import UserEmailInvite from "@/models/userEmailInvite";
import UserProfile from "@/models/userProfile";
import { WebClientConfiguration } from "@/models/configData";
import { library } from "@fortawesome/fontawesome-svg-core";
import { faExclamationTriangle } from "@fortawesome/free-solid-svg-icons";
import moment from "moment";

library.add(faExclamationTriangle);

const userNamespace: string = "user";
const authNamespace: string = "auth";

@Component({
  components: {
    LoadingComponent
  }
})
export default class ProfileComponent extends Vue {
  @Getter("oidcIsAuthenticated", {
    namespace: authNamespace
  })
  oidcIsAuthenticated!: boolean;

  @Action("getUserEmail", { namespace: userNamespace })
  getUserEmail!: ({ hdid }: { hdid: string }) => Promise<UserEmailInvite>;

  @Action("updateUserEmail", { namespace: userNamespace })
  updateUserEmail!: ({
    hdid,
    emailAddress
  }: {
    hdid: string;
    emailAddress: string;
  }) => Promise<void>;

  @Action("closeUserAccount", { namespace: userNamespace })
  closeUserAccount!: ({ hdid }: { hdid: string }) => Promise<void>;

  @Action("recoverUserAccount", { namespace: userNamespace })
  recoverUserAccount!: ({ hdid }: { hdid: string }) => Promise<void>;

  @Getter("user", { namespace: userNamespace }) user!: User;

  @Getter("userIsActive", { namespace: userNamespace })
  isActiveProfile!: boolean;

  @Getter("webClient", { namespace: "config" })
  webClientConfig!: WebClientConfiguration;

  private isLoading: boolean = true;
  private hasErrors: boolean = false;
  private errorMessage: string = "";

  private emailVerified = false;
  private email: string = "";
  private emailConfirmation: string = "";
  private isEmailEditable: boolean = false;
  private oidcUser: any = {};
  private verificationSent: boolean = false;

  private phoneNumber: string = "";
  private isPhoneEditable: boolean = false;
  private tempPhone: string = "";

  private tempEmail: string = "";
  private submitStatus: string = "";
  private userProfileService!: IUserProfileService;
  private userProfile!: UserProfile;

  private lastLoginDateString: string = "";
  private plannedDeletionDateTime: Date = new Date();

  private showCloseWarning = false;

  private timeForDeletion: number = -1;

  private interval: any;

  mounted() {
    this.userProfileService = container.get<IUserProfileService>(
      SERVICE_IDENTIFIER.UserProfileService
    );

    // Load the user name and current email
    let authenticationService = container.get<IAuthenticationService>(
      SERVICE_IDENTIFIER.AuthenticationService
    );

    this.isLoading = true;
    var oidcUserPromise = authenticationService.getOidcUserProfile();
    var userEmailPromise = this.getUserEmail({ hdid: this.user.hdid });
    var userProfilePromise = this.userProfileService.getProfile(this.user.hdid);

    Promise.all([oidcUserPromise, userEmailPromise, userProfilePromise])
      .then(results => {
        // Load oidc user details
        if (results[0]) {
          this.oidcUser = results[0];
        }

        if (results[1]) {
          // Load user email
          var userEmailInvite = results[1];
          this.email = userEmailInvite.emailAddress;
          this.emailVerified = userEmailInvite.validated;
          this.verificationSent = this.emailVerified;
        }

        if (results[2]) {
          // Load user profile
          this.userProfile = results[2];
          console.log("User Profile: ", this.userProfile);
          this.lastLoginDateString = moment(
            this.userProfile.lastLoginDateTime
          ).format("lll");
          this.phoneNumber = this.userProfile.phoneNumber;
        }

        this.isLoading = false;
      })
      .catch(err => {
        console.log("Error loading profile");
        console.log(err);
        this.hasErrors = true;
        this.isLoading = false;
      });

    this.calculateTimeForDeletion();
    this.interval = setInterval(() => {
      this.calculateTimeForDeletion();
    }, 1000);
  }

  validations() {
    const phone = helpers.regex("phone", /^\D?(\d{3})\D?\D?(\d{3})\D?(\d{4})$/);
    return {
      phoneNumber: {
        required: requiredIf(() => {
          return this.isPhoneEditable && this.phoneNumber !== "";
        }),
        newPhoneNumber: not(sameAs("tempPhone")),
        phone
      },
      email: {
        required: requiredIf(() => {
          return this.isEmailEditable && this.email !== "";
        }),
        newEmail: not(sameAs("tempEmail")),
        email
      },
      emailConfirmation: {
        required: requiredIf(() => {
          return this.isEmailEditable && this.emailConfirmation !== "";
        }),
        sameAsEmail: sameAs("email"),
        email
      }
    };
  }

  private get fullName(): string {
    return this.oidcUser.given_name + " " + this.oidcUser.family_name;
  }

  private calculateTimeForDeletion(): void {
    if (this.isActiveProfile) {
      return undefined;
    }

    let endDate = moment(this.user.closedDateTime);
    endDate.add(this.webClientConfig.hoursForDeletion, "h");
    this.timeForDeletion = endDate.diff(moment());
  }

  private get timeForDeletionString(): string {
    if (this.isActiveProfile) {
      return "";
    }

    if (this.timeForDeletion < 0) {
      return "Your account will be closed imminently";
    }

    let duration = moment.duration(this.timeForDeletion);
    let timeRemaining = duration.asDays();
    if (timeRemaining > 1) {
      return this.pluralize(timeRemaining, "day");
    }
    timeRemaining = duration.asHours();
    if (timeRemaining > 1) {
      return this.pluralize(timeRemaining, "hour");
    }
    timeRemaining = duration.asMinutes();
    if (timeRemaining > 1) {
      return this.pluralize(timeRemaining, "minute");
    }

    timeRemaining = duration.asSeconds();
    return this.pluralize(timeRemaining, "second");
  }

  private pluralize(count: number, message: string): string {
    let roundCount = Math.floor(count);
    return roundCount.toString() + " " + message + (roundCount > 1 ? "s" : "");
  }

  private isValid(param: any): boolean | undefined {
    return param.$dirty ? !param.$invalid : undefined;
  }

  private makeEmailEditable(): void {
    this.isEmailEditable = true;
    this.emailConfirmation = this.email;
    this.tempEmail = this.email || "";
  }

  private makePhoneEditable(): void {
    this.isPhoneEditable = true;
    this.tempPhone = this.phoneNumber || "";
  }

  private cancelEmailEdit(): void {
    this.isEmailEditable = false;
    this.email = this.tempEmail;
    this.emailConfirmation = "";
    this.tempEmail = "";
    this.$v.$reset();
  }

  private cancelPhoneEdit(): void {
    this.isPhoneEditable = false;
    this.phoneNumber = this.tempPhone;
    this.tempPhone = "";
    this.$v.$reset();
  }

  private saveEmailEdit(event: any): void {
    this.$v.$touch();
    console.log(this.$v);
    if (this.$v.email.$invalid || this.$v.emailConfirmation.$invalid) {
      this.submitStatus = "ERROR";
    } else {
      this.submitStatus = "PENDING";

      console.log(this.email);

      this.sendUserEmailUpdate();
    }

    event.preventDefault();
  }

  private savePhoneEdit(event: any): void {
    this.$v.$touch();
    console.log(this.$v);
    if (this.$v.phoneNumber.$invalid) {
      this.submitStatus = "ERROR";
    } else {
      this.submitStatus = "PENDING";
      console.log(this.phoneNumber);
      this.updatePhoneNumber();
    }
    event.preventDefault();
  }

  private sendUserEmailUpdate(): void {
    this.isLoading = true;
    this.updateUserEmail({
      hdid: this.user.hdid || "",
      emailAddress: this.email
    })
      .then(() => {
        console.log("success!");
        this.isEmailEditable = false;
        this.verificationSent = true;
        this.emailConfirmation = "";
        this.tempEmail = "";
        this.$v.$reset();
      })
      .catch(err => {
        this.hasErrors = true;
        console.log(err);
      })
      .finally(() => {
        this.isLoading = false;
      });
  }

  private updatePhoneNumber(): void {
    console.log(
      "Updating " + this.phoneNumber ? this.phoneNumber : "phone number..."
    );
    // Send update to backend
    this.userProfileService
      .updatePhoneNumber(this.user.hdid, this.phoneNumber)
      .then(() => {
        this.isPhoneEditable = false;
        this.tempPhone = "";
        this.$v.$reset();
      });
  }

  private removeEmail(): void {
    this.$v.$touch();
    this.email = "";
    this.emailConfirmation = "";
  }

  private removePhone(): void {
    this.$v.$touch();
    this.phoneNumber = "";
  }

  private recoverAccount(): void {
    this.isLoading = true;
    this.recoverUserAccount({
      hdid: this.user.hdid
    })
      .then(() => {
        console.log("success!");
      })
      .catch(err => {
        this.hasErrors = true;
        console.log(err);
      })
      .finally(() => {
        this.isLoading = false;
      });
  }

  private showCloseWarningBtn(): void {
    this.showCloseWarning = true;
  }

  private cancelClose(): void {
    this.showCloseWarning = false;
  }

  private closeAccount(): void {
    this.isLoading = true;
    this.closeUserAccount({
      hdid: this.user.hdid
    })
      .then(() => {
        console.log("success!");
        this.showCloseWarning = false;
      })
      .catch(err => {
        this.hasErrors = true;
        console.log(err);
      })
      .finally(() => {
        this.isLoading = false;
      });
  }
}
</script>
