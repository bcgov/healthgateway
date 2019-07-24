<template>
  <div id="login" align="center">
    <b-row style="height: 3rem;"></b-row>
    <b-row>
      <b-col>
        <b-card class="shadow-lg bg-white" tag="login" style="max-width: 25rem;" align="center">
          <h3 slot="header">Log In</h3>
          <p slot="footer">
            Not yet registered?
            <b-link to="/registration">Sign up</b-link>
          </p>
          <b-card-body>
            <b-row>
              <b-col>
                <b-button v-on:click="oidcLogin('bcsc')" block variant="primary" disabled>
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
                <b-button v-on:click="oidcLogin('idir')" block variant="primary">
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
                <b-button href="#" v-on:click="oidcLogin('github')" block variant="primary">
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
const namespace: string = "auth";
import { State, Action, Getter } from "vuex-class";
import { AuthState, Authentication, RootState } from "../models/authState";

@Component
export default class LoginComponent extends Vue {
  @State("auth") authState: AuthState;
  @Action("login", { namespace }) login: any;
  @Getter("isAuthenticated", { namespace }) isAuthenticated: boolean;
  @Getter("requestedRoute", { namespace }) requestedRoute: string;

  oidcLogin(idirHint: string) {
    const requestedRoute = this.authState && this.authState.requestedRoute;

    console.log(window.location.hostname + requestedRoute);
    this.login(idirHint, requestedRoute);
  }
}
</script>
      