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
              <div v-if="!emailVerified && !isEdditable && email" class="ml-3">
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
            <b-form-invalid-feedback :state="$v.emailConfirmation.sameAsEmail">
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
            Removing your email address will disable future communications from
            the Health Gateway
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
  IUserEmailService,
  IAuthenticationService
} from "@/services/interfaces";
import SERVICE_IDENTIFIER from "@/constants/serviceIdentifiers";
import container from "@/inversify.config";
import { User as OidcUser } from "oidc-client";
import User from "@/models/user";
import UserEmailInvite from "@/models/userEmailInvite";
import { library } from "@fortawesome/fontawesome-svg-core";
import { faExclamationTriangle } from "@fortawesome/free-solid-svg-icons";
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
  @Getter("user", { namespace: userNamespace }) user: User;

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

  mounted() {
    // Load the user name and current email
    var authenticationService: IAuthenticationService = container.get(
      SERVICE_IDENTIFIER.AuthenticationService
    );

    this.userEmailService = container.get<IUserEmailService>(
      SERVICE_IDENTIFIER.UserEmailService
    );

    this.isLoading = true;
    var oidcUserPromise = authenticationService.getOidcUserProfile();
    var userEmailPromise = this.getUserEmail({ hdid: this.user.hdid });
    Promise.all([oidcUserPromise, userEmailPromise])
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

        this.isLoading = false;
      })
      .catch(err => {
        console.log("Error loading profile");
        console.log(err);
        this.hasErrors = true;
        this.isLoading = false;
      });
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

  get fullName(): string {
    return this.oidcUser.given_name + " " + this.oidcUser.family_name;
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
}
</script>
