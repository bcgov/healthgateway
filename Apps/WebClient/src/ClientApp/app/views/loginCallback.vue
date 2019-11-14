<template>
  <div>
    Waiting...
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import { Component } from "vue-property-decorator";
import { State, Action, Getter } from "vuex-class";
import User from "@/models/user";

@Component
export default class LoginCallbackComponent extends Vue {
  @Action("oidcSignInCallback", { namespace: "auth" }) oidcSignInCallback;
  @Action("checkRegistration", { namespace: "user" }) checkRegistration;
  @Getter("userIsRegistered", { namespace: "user" }) userIsRegistered: boolean;
  @Getter("user", { namespace: "user" }) user: User;
  created() {
    this.oidcSignInCallback()
      .then(redirectPath => {
        console.log(this.user);
        this.checkRegistration({ hdid: this.user.hdid }).then(() => {
          if (this.userIsRegistered) {
            this.$router.push({ path: redirectPath });
          } else {
            this.$router.push({ path: "/registrationInfo" });
          }

          console.log(redirectPath);
          console.log("here");
        });
      })
      .catch(err => {
        console.error(err);
        this.$router.push("/signin-oidc-error"); // Handle errors any way you want
      });
  }
}
</script>
