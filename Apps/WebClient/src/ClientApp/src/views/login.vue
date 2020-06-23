<template>
  <div class="container my-5" align="center">
    <LoadingComponent :is-loading="isLoading"></LoadingComponent>
    <b-row>
      <b-col>
        <b-alert
          style="max-width: 25rem;"
          :show="isRetry"
          dismissible
          variant="danger"
        >
          <h4>Error</h4>
          <span
            >An unexpected error occured while processing the request, please
            try again.</span
          >
        </b-alert>
        <b-card
          v-if="identityProviders && identityProviders.length > 0"
          id="loginPicker"
          class="shadow-lg bg-white"
          style="max-width: 25rem;"
          align="center"
        >
          <h3 slot="header">Log In</h3>
          <p v-if="hasMultipleProviders || isRetry" slot="footer">
            Not yet registered?
            <b-link to="/registrationInfo">Sign up</b-link>
          </p>
          <b-card-body v-if="hasMultipleProviders || isRetry">
            <div v-for="provider in identityProviders" :key="provider.id">
              <b-row>
                <b-col>
                  <b-button
                    :id="`${provider.id}Btn`"
                    block
                    :disabled="provider.disabled"
                    variant="primary"
                    @click="oidcLogin(provider.hint)"
                  >
                    <b-row>
                      <b-col class="col-2">
                        <font-awesome-icon
                          :icon="`${provider.icon}`"
                        ></font-awesome-icon>
                      </b-col>
                      <b-col class="text-justify">
                        <span>{{ provider.name }}</span>
                      </b-col>
                    </b-row>
                  </b-button>
                </b-col>
              </b-row>
              <b-row
                v-if="
                  identityProviders.indexOf(provider) <
                  identityProviders.length - 1
                "
                ><b-col>or</b-col>
              </b-row>
            </div>
          </b-card-body>
          <b-card-body v-else>
            <span
              >Redirecting to <strong>{{ identityProviders[0].name }}</strong
              >...</span
            >
          </b-card-body>
        </b-card>
        <div v-else>No login providers configured</div>
      </b-col>
    </b-row>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import { Component, Prop } from "vue-property-decorator";
import { Action, Getter, State } from "vuex-class";
import VueRouter, { Route } from "vue-router";
import LoadingComponent from "@/components/loading.vue";
import {
  ExternalConfiguration,
  IdentityProviderConfiguration,
} from "@/models/configData";

const namespace: string = "auth";

@Component({
  components: {
    LoadingComponent,
  },
})
export default class LoginView extends Vue {
  @Action("authenticateOidc", { namespace }) authenticateOidc!: (params: {
    idpHint: string;
    redirectPath: string;
  }) => Promise<void>;

  @Getter("oidcIsAuthenticated", { namespace }) oidcIsAuthenticated!: boolean;
  @Getter("userIsRegistered", { namespace: "user" }) userIsRegistered!: boolean;
  @Getter("identityProviders", { namespace: "config" })
  identityProviders!: IdentityProviderConfiguration[];

  @Prop() isRetry?: boolean;

  private isLoading: boolean = true;
  private redirectPath: string = "";
  private routeHandler!: VueRouter;

  private mounted() {
    if (this.$route.query.redirect && this.$route.query.redirect !== "") {
      this.redirectPath = this.$route.query.redirect.toString();
    } else {
      this.redirectPath = "/timeline";
    }

    this.routeHandler = this.$router;
    if (this.oidcIsAuthenticated && this.userIsRegistered) {
      this.routeHandler.push({ path: this.redirectPath });
    } else if (this.oidcIsAuthenticated) {
      this.redirectPath = "/registrationInfo";
      this.routeHandler.push({ path: this.redirectPath });
    } else if (
      !this.oidcIsAuthenticated &&
      this.identityProviders.length == 1 &&
      !this.isRetry
    ) {
      this.oidcLogin(this.identityProviders[0].hint);
    } else {
      this.isLoading = false;
    }
  }

  get hasMultipleProviders(): boolean {
    return this.identityProviders.length > 1;
  }

  oidcLogin(hint: string) {
    // if the login action returns it means that the user already had credentials.
    this.authenticateOidc({
      idpHint: hint,
      redirectPath: this.redirectPath,
    }).then(() => {
      if (this.oidcIsAuthenticated) {
        this.routeHandler.push({ path: this.redirectPath });
      }
    });
  }
}
</script>
