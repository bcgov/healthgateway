<template>
  <div class="container" align="center">
    <b-row style="height: 3rem;"></b-row>
    <b-row>
      <b-col>
        <b-card
          class="shadow-lg bg-white"
          style="max-width: 25rem;"
          align="center"
          id="loginPicker"
        >
          <h3 slot="header">Log In</h3>
          <p slot="footer">
            Not yet registered?
            <b-link to="/registration">Sign up</b-link>
          </p>
          <b-card-body>
              <div :key="provider.id" v-for="provider in identityProviders">
                  <b-row>
                      <b-col>
                          <b-button :id="`${provider.id}Btn`"
                                    v-on:click="oidcLogin(provider.hint)"
                                    block
                                    variant="primary">
                              <b-row>
                                  <b-col class="col-2">
                                      <span :class="`fa ${provider.icon}`"></span>
                                  </b-col>
                                  <b-col class="text-justify">
                                      <span>{{ provider.name }}</span>
                                  </b-col>
                              </b-row>
                          </b-button>
                      </b-col>
                  </b-row>
                  <b-row v-if="identityProviders.indexOf(provider) < identityProviders.length - 1"><b-col>or</b-col>
                  </b-row>
              </div>
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
} from "../models/ConfigData";

const namespace: string = "auth";

@Component
export default class LoginComponent extends Vue {
  @Action("authenticateOidc", { namespace }) authenticateOidc: any;
  @Getter("oidcIsAuthenticated", { namespace }) oidcIsAuthenticated: boolean;
  @Getter("identityProviders", { namespace: "config" })
  identityProviders: IdentityProviderConfiguration[];

  private redirectPath: string = "";
  private routeHandler = undefined;

  mounted() {
    if (this.$route.query.redirect && this.$route.query.redirect !== "") {
      this.redirectPath = this.$route.query.redirect;
    } else {
      this.redirectPath = "/home";
    }

    this.routeHandler = this.$router;
    if (this.oidcIsAuthenticated) {
      this.routeHandler.push({ path: this.redirectPath });
    }
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
