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
                {{ lastLoginDate }}
              </div>
            </b-col>
          </b-row>
          <b-row class="mb-3">
            <b-col>
              <label for="email">Email Address</label>
              <b-button
                v-if="!isEdditable"
                id="editEmail"
                class="mx-auto"
                variant="link"
                @click="makeEdditable()"
                >Edit
              </b-button>
              <b-button
                v-if="email"
                id="removeEmail"
                class="text-danger"
                variant="link"
                @click="
                  makeEdditable();
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
                  :placeholder="isEdditable ? 'Your email address' : 'Empty'"
                  :disabled="!isEdditable"
                  :state="isValid($v.email)"
                />
                <div
                  v-if="!emailVerified && !isEdditable && email"
                  class="ml-3"
                >
                  (Not Verified)
                </div>
                <b-button
                  v-if="!emailVerified && !isEdditable && email"
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
          <b-row v-if="isEdditable" class="mb-3">
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
              Removing your email address will disable future communications
              from the Health Gateway
            </b-col>
          </b-row>
          <b-row v-if="isEdditable" class="mb-3 justify-content-end">
            <b-col class="text-right">
              <b-button
                id="cancelBtn"
                class="mx-2 actionButton"
                @click="cancelEdit()"
                >Cancel
              </b-button>
              <b-button
                id="saveBtn"
                variant="primary"
                class="mx-2 actionButton"
                :disabled="tempEmail === email"
                @click="saveEdit()"
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
                Account marked for deletion
              </label>
              <div id="deletionWarning">
                Your account has been deactivated from the site and will be
                permanently deleted. If you wish to recover your account click
                on the "Recover Account" button before the time expires.
              </div>
            </b-col>
          </b-row>
          <b-row class="mb-3">
            <b-col>
              <label for="lastLoginDate">Time remaining for deletion: </label>
              {{ timeForDeletionString }}
            </b-col>
          </b-row>
          <b-row class="mb-3">
            <b-col>
              <b-button
                id="recoverBtn"
                class="mx-auto"
                variant="warning"
                @click="restoreAccount()"
                >Recover Account
              </b-button>
            </b-col>
          </b-row>
        </div>
        <b-row class="mb-3">
          <b-col>
            <label>Other</label>
            <div>
              <router-link
                id="termsOfService"
                variant="primary"
                to="/termsOfService"
                class="p-0"
              >
                Terms of Service
              </router-link>
            </div>
            <div>
              <b-button
                v-if="isActiveProfile && !showDeletionWarning"
                id="showDeletionWarningBtn"
                class="p-0 pt-2"
                variant="link"
                @click="showDeletionWarningBtn()"
                >Delete My Account
              </b-button>
              <b-row v-if="showDeletionWarning">
                <b-col class="font-weight-bold text-danger text-center">
                  <hr />
                  <font-awesome-icon
                    icon="exclamation-triangle"
                    aria-hidden="true"
                  ></font-awesome-icon>
                  Your account will be marked for deletion, preventing you from
                  accessing your information on the Health Gateway. After a set
                  period of time it will be deleted permanently.
                </b-col>
              </b-row>
              <b-row
                v-if="showDeletionWarning"
                class="mb-3 justify-content-end"
              >
                <b-col class="text-right">
                  <b-button
                    id="cancelDeleteBtn"
                    class="mx-2 actionButton"
                    @click="cancelDelete()"
                    >Cancel
                  </b-button>
                  <b-button
                    id="deleteAccountBtn"
                    class="mx-2 actionButton"
                    variant="danger"
                    @click="removeAccount()"
                    >Delete
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
  not
} from "vuelidate/lib/validators";
import {
  IUserProfileService,
  IUserEmailService,
  IAuthenticationService
} from "@/services/interfaces";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { User as OidcUser } from "oidc-client";
import User from "@/models/user";
import UserEmailInvite from "@/models/userEmailInvite";
import UserProfile from "@/models/userProfile";
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
  oidcIsAuthenticated: boolean;
  @Action("getUserEmail", { namespace: userNamespace }) getUserEmail;
  @Action("updateUserEmail", { namespace: userNamespace }) updateUserEmail;
  @Action("deleteAccount", { namespace: userNamespace }) deleteAccount;
  @Action("recoverAccount", { namespace: userNamespace }) recoverAccount;

  @Getter("user", { namespace: userNamespace }) user: User;
  @Getter("userIsActive", { namespace: userNamespace })
  isActiveProfile: boolean;

  private isLoading: boolean = true;
  private hasErrors: boolean = false;
  private errorMessage: string = "";

  private emailVerified = false;
  private email: string = "";
  private emailConfirmation: string = "";
  private isEdditable: boolean = false;
  private oidcUser: any = {};
  private verificationSent: boolean = false;

  private tempEmail: string = "";
  private submitStatus: string = "";
  private userEmailService: IUserEmailService;
  private userProfileService: IUserProfileService;
  private userProfile: UserProfile;

  private showDeletionWarning = false;

  private timeForDeletion: number = -1;

  private interval: any;

  mounted() {
    this.userProfileService = container.get<IUserProfileService>(
      SERVICE_IDENTIFIER.UserProfileService
    );

    this.userEmailService = container.get<IUserEmailService>(
      SERVICE_IDENTIFIER.UserEmailService
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
    return {
      email: {
        required: requiredIf(() => {
          return !this.isEdditable;
        }),
        newEmail: not(sameAs("tempEmail")),
        email
      },
      emailConfirmation: {
        required: requiredIf(() => {
          return !this.isEdditable;
        }),
        sameAsEmail: sameAs("email"),
        email
      }
    };
  }

  private get fullName(): string {
    return this.oidcUser.given_name + " " + this.oidcUser.family_name;
  }

  private get lastLoginDate(): string {
    return moment(this.userProfile).format("lll");
  }

  private calculateTimeForDeletion(): void {
    if (this.isActiveProfile) {
      return undefined;
    }

    let endDate = moment(this.user.plannedDeletionDateTime);
    this.timeForDeletion = endDate.diff(moment());
  }

  private get timeForDeletionString(): string {
    if (this.isActiveProfile) {
      return "";
    }

    let duration = moment.duration(this.timeForDeletion);
    let timeRemaining = duration.asDays();
    if (timeRemaining > 1) {
      return Math.floor(timeRemaining).toString() + " days";
    }
    timeRemaining = duration.asHours();
    if (timeRemaining > 1) {
      return Math.floor(timeRemaining).toString() + " hours";
    }
    timeRemaining = duration.asMinutes();
    if (timeRemaining > 1) {
      return Math.floor(timeRemaining).toString() + " minutes";
    }

    timeRemaining = duration.asSeconds();
    return Math.floor(timeRemaining).toString() + " seconds";
  }

  private isValid(param: any): boolean | undefined {
    return param.$dirty ? !param.$invalid : undefined;
  }

  private makeEdditable(): void {
    this.isEdditable = true;
    this.emailConfirmation = this.email;
    this.tempEmail = this.email || "";
  }

  private cancelEdit(): void {
    this.isEdditable = false;
    this.email = this.tempEmail;
    this.emailConfirmation = "";
    this.tempEmail = "";
    this.$v.$reset();
  }

  private saveEdit(): void {
    this.$v.$touch();
    console.log(this.$v);
    if (this.$v.$invalid) {
      this.submitStatus = "ERROR";
    } else {
      this.submitStatus = "PENDING";

      console.log(this.email);

      this.sendUserEmailUpdate();
    }

    event.preventDefault();
  }

  private sendUserEmailUpdate(): void {
    this.isLoading = true;
    this.updateUserEmail({
      hdid: this.user.hdid,
      emailAddress: this.email
    })
      .then(() => {
        console.log("success!");
        this.isEdditable = false;
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

  private removeEmail(): void {
    this.$v.$touch();
    this.email = "";
    this.emailConfirmation = "";
  }

  private restoreAccount(): void {
    this.isLoading = true;
    this.recoverAccount({
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

  private showDeletionWarningBtn(): void {
    this.showDeletionWarning = true;
  }

  private cancelDelete(): void {
    this.showDeletionWarning = false;
  }

  private removeAccount(): void {
    this.isLoading = true;
    this.deleteAccount({
      hdid: this.user.hdid
    })
      .then(() => {
        console.log("success!");
        this.showDeletionWarning = false;
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
