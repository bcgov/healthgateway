<template>
  <div class="container my-5" align="center">
    <b-row>
      <b-col>
        <b-card
          id="loginPicker"
          class="shadow-lg bg-white"
          style="max-width: 25rem;"
          align="center"
        >
          <h3 slot="header">Log In</h3>
          <p v-if="hasMultipleProviders" slot="footer">
            Not yet registered?
            <b-link to="/registrationInfo">Sign up</b-link>
          </p>
          <b-card-body v-if="hasMultipleProviders">
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
                        <span :class="`${provider.icon}`"></span>
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
            <b-spinner class="ml-2"></b-spinner>
          </b-card-body>
        </b-card>
      </b-col>
    </b-row>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import { Component } from "vue-property-decorator";
import { State, Action, Getter } from "vuex-class";
import {
  IdentityProviderConfiguration,
  ExternalConfiguration
} from "@/models/configData";

const namespace: string = "auth";

@Component
export default class LoginComponent extends Vue {
  @Action("authenticateOidc", { namespace }) authenticateOidc: any;
  @Getter("oidcIsAuthenticated", { namespace }) oidcIsAuthenticated: boolean;
  @Getter("userIsRegistered", { namespace: "user" }) userIsRegistered: boolean;
  @Getter("identityProviders", { namespace: "config" })
  identityProviders: IdentityProviderConfiguration[];

  private redirectPath: string = "";
  private routeHandler = undefined;

  mounted() {
    if (this.$route.query.redirect && this.$route.query.redirect !== "") {
      this.redirectPath = this.$route.query.redirect;
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
      this.identityProviders.length == 1
    ) {
      this.oidcLogin(this.identityProviders[0].hint);
    }
  }

  get hasMultipleProviders(): boolean {
    return this.identityProviders.length > 1;
  }

  oidcLogin(hint: string) {
    // if the login action returns it means that the user already had credentials.
    this.authenticateOidc({
      idpHint: hint,
      redirectPath: this.redirectPath
    }).then(result => {
      if (this.oidcIsAuthenticated) {
        this.routeHandler.push({ path: this.redirectPath });
      }
    });
  }
}
</script>
