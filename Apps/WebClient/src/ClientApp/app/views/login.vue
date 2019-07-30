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
            <b-row>
              <b-col>
                <b-button
                  id="bcscBtn"
                  v-on:click="oidcLogin('bcsc')"
                  block
                  variant="primary"
                  disabled
                >
                  <tr>
                    <td style="width: 3rem;">
                      <span class="fa fa-address-card"></span>
                    </td>
                    <td>Log in with BC Services Card</td>
                  </tr>
                </b-button>
              </b-col>
            </b-row>
            <b-row>
              <b-col>or</b-col>
            </b-row>
            <b-row>
              <b-col>
                <b-button id="idirBtn" v-on:click="oidcLogin('idir')" block variant="primary">
                  <tr>
                    <td style="width: 3rem;">
                      <span class="fa fa-user"></span>
                    </td>
                    <td>Log in with IDIR</td>
                  </tr>
                </b-button>
              </b-col>
            </b-row>
            <b-row>
              <b-col>or</b-col>
            </b-row>
            <b-row>
              <b-col>
                <b-button id="gitBtn" v-on:click="oidcLogin('github')" block variant="primary">
                  <tr>
                    <td style="width: 3rem;">
                      <span class="fab fa-github"></span>
                    </td>
                    <td>Log in with GitHub</td>
                  </tr>
                </b-button>
              </b-col>
            </b-row>
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

const namespace: string = "auth";

@Component
export default class LoginComponent extends Vue {
  @Action("login", { namespace }) login: any;
  @Getter("isAuthenticated", { namespace }) isAuthenticated: boolean;

  private redirectPath: string = "";
  private routeHandler = undefined;

  mounted() {
    if (this.$route.query.redirect && this.$route.query.redirect !== "") {
      this.redirectPath = this.$route.query.redirect;
    } else {
      this.redirectPath = "/home";
    }

    this.routeHandler = this.$router;

    if (this.isAuthenticated) {
      this.routeHandler.push({ path: this.redirectPath });
    }
  }

  oidcLogin(hint: string) {
    // if the login action returns it means that the user already had credentials.
    this.login({ idpHint: hint, redirectPath: this.redirectPath }).then(
      result => {
        if (this.isAuthenticated) {
          this.routeHandler.push({ path: this.redirectPath });
        }
      }
    );
  }
}
</script>
      