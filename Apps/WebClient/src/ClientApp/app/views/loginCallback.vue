<template>
  <div>
    Waiting...
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import { Component } from "vue-property-decorator";
import { State, Action, Getter } from "vuex-class";

const namespace: string = "auth";

@Component
export default class LoginCallbackComponent extends Vue {
  @Action("oidcSignInCallback", { namespace }) oidcSignInCallback;
  @Getter("userIsRegistered", { namespace }) userIsRegistered: boolean;
  created() {
    this.oidcSignInCallback()
      .then(redirectPath => {
        console.log(this.userIsRegistered);
        if (this.userIsRegistered) {
          this.$router.push({ path: redirectPath });
        } else {
          this.$router.push({ path: "/registration" });
        }
        console.log(redirectPath);
        console.log("here");
      })
      .catch(err => {
        console.error(err);
        this.$router.push("/signin-oidc-error"); // Handle errors any way you want
      });
  }
}
</script>
