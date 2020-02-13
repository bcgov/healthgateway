<template>
  <v-container> REDIRECTING...</v-container>
</template>

<script lang="ts">
import { Component, Vue } from "vue-property-decorator";
import { State, Action, Getter } from "vuex-class";

const namespace: string = "auth";

@Component
export default class LoginView extends Vue {
  public name = "Dashboard";
  @Action("login", { namespace }) login: any;
  @Getter("isAuthenticated", { namespace }) isAuthenticated: boolean;
  private redirectPath: string = "";
  private routeHandler = "";
  mounted() {
    if (this.$route.query.redirect && this.$route.query.redirect !== "") {
      this.redirectPath = this.$route.query.redirect;
    } else {
      this.redirectPath = "/";
    }
    this.routeHandler = this.$router;
    if (this.isAuthenticated) {
      this.routeHandler.push({ path: this.redirectPath });
    }

    console.log("path", this.redirectPath);

    this.login({ redirectPath: this.redirectPath }).then(result => {
      if (this.isAuthenticated) {
        this.routeHandler.push({ path: this.redirectPath });
      }
    });
  }
}
</script>
