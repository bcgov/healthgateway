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
  <div class="container">
    <LoadingComponent :is-loading="isLoading"></LoadingComponent>
    <div class="row pt-5">
      <div class="col-lg-12 col-md-12 text-center">
        <div id="pageTitle">
          <h1 id="subject">
            Profile
          </h1>
          <hr />
        </div>
        <b-row class="mb-3">
          <b-col>
            Full Name
            <b-form-input
              id="profileNames"
              v-model="fullName"
              :disabled="true"
            />
          </b-col>
        </b-row>
        <b-row class="mb-3">
          <b-col>
            Email Address
            <b-form-input
              id="email"
              v-model="$v.email.$model"
              type="email"
              placeholder="Your email address"
              :disabled="isEdditable"
              :state="isValid($v.email)"
            />
            <b-form-invalid-feedback :state="isValid($v.email)">
              Valid email is required
            </b-form-invalid-feedback>
          </b-col>
        </b-row>
        <b-row v-if="!isEdditable" class="mb-3">
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
  }) oidcIsAuthenticated: boolean;
  @Action("getPatientData", { namespace: authNamespace }) getPatientData;
  @Getter("user", { namespace: userNamespace }) user: User;

  private isLoading: boolean = true;
  private hasErrors: boolean = false;
  private errorMessage: string = "";

  private email: string = "";
  private emailConfirmation: string = "";
  private isEdditable: boolean = false;
  private oidcUser: any = {};

  mounted() {
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
      },
      accepted: { isChecked: sameAs(() => true) }
    };
  }

  get fullName(): string {
    return this.oidcUser.given_name + " " + this.oidcUser.family_name;
  }

  private isValid(param: any): boolean | undefined {
    return param.$dirty ? !param.$invalid : undefined;
  }
}
</script>
