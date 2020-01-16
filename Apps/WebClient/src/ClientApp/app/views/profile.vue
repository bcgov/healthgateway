<style lang="scss">
@import "@/assets/scss/_variables.scss";

#pageTitle {
  color: $primary;
}

label {
  font-weight: bold;
}

input {
  max-width: 320px;
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
        <div id="pageTitle">
          <h1 id="subject">
            Profile
          </h1>
          <hr />
        </div>
        <b-row class="mb-3">
          <b-col>
            <label for="profileNames">Full Name</label>
            <b-form-input
              id="profileNames"
              v-model="fullName"
              :disabled="true"
            />
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
            <b-form-input
              id="email"
              v-model="$v.email.$model"
              type="email"
              :placeholder="isEdditable ? 'Your email address' : 'Empty'"
              :disabled="!isEdditable"
              :state="isValid($v.email)"
            />
            <b-form-invalid-feedback :state="isValid($v.email)">
              Valid email is required
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
import { required, requiredIf, sameAs, email } from "vuelidate/lib/validators";
import {
  IUserEmailService,
  IAuthenticationService
} from "@/services/interfaces";
import SERVICE_IDENTIFIER from "@/constants/serviceIdentifiers";
import container from "@/inversify.config";
import { User as OidcUser } from "oidc-client";
import User from "@/models/user";

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
  @Action("getPatientData", { namespace: authNamespace }) getPatientData;
  @Action("getUserEmail", { namespace: userNamespace }) getUserEmail;
  @Action("updateUserEmail", { namespace: userNamespace }) updateUserEmail;
  @Getter("user", { namespace: userNamespace }) user: User;

  private isLoading: boolean = true;
  private hasErrors: boolean = false;
  private errorMessage: string = "";

  private email: string = "";
  private emailConfirmation: string = "";
  private isEdditable: boolean = false;
  private oidcUser: any = {};

  private tempEmail: string = "";

  private submitStatus: string = "";

  private userEmailService: IUserEmailService;

  mounted() {
    // Load the user name
    var authenticationService: IAuthenticationService = container.get(
      SERVICE_IDENTIFIER.AuthenticationService
    );

    this.userEmailService = container.get<IUserEmailService>(
      SERVICE_IDENTIFIER.UserEmailService
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

    if (this.user.hasEmail) {
      this.getUserEmail({ hdid: this.user.hdid }).then(emailInvite => {
        console.log(emailInvite);
      });
    }
  }

  validations() {
    return {
      email: {
        required: requiredIf(() => {
          return !this.isEdditable;
        }),
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
    this.tempEmail = this.email;
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

      this.isLoading = true;
      this.updateUserEmail(
        { hdid: this.user.hdid },
        { emailAddress: this.email }
      ).then(() => {
        console.log("success!");
        this.isLoading = false;
      });
      /*this.userProfileService
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
        });*/
    }

    event.preventDefault();
  }
}
</script>
