<template>
  <div class="container">
    <h1>Authentication status:</h1>
    <div v-if="authState.authentication != null">
      <p>Authenticated: {{ isAuthenticated }}</p>
      <p>JWT: {{ token }}</p>
    </div>
    <div v-if="authState.error">Oops an error occured: {{authState.statusMessage}}</div>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import { State, Action, Getter } from "vuex-class";
import Component from "vue-class-component";
import { AuthState, Authentication, RootState } from "../models/authState";

const namespace: string = "auth";

@Component
export default class UserDetail extends Vue {
  @State("auth") authState: AuthState;
  @Action("login", { namespace }) login: any;
  @Getter("isAuthenticated", { namespace }) isAuthenticated: boolean;

  mounted() {
    // fetching data as soon as the component's been mounted
  }

  // computed variable based on user's email
  get token() {
    const authentication = this.authState && this.authState.authentication;
    return (authentication && authentication.token) || "";
  }
}
</script>