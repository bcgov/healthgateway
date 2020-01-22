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
  @Action("clearStorage", { namespace: "auth" }) clearStorage;
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
            if (redirectPath.startsWith("/registration")) {
              this.$router.push({ path: redirectPath });
            } else {
              this.$router.push({ path: "/registration" });
            }
          }

          console.log(redirectPath);
          console.log("here");
        });
      })
      .catch(err => {
        // Login failed redirect it back to the login page.
        console.error(err);
        this.clearStorage();
        this.$router.push({ path: "/login", query: { isRetry: "true" } });
      });
  }
}
</script>
