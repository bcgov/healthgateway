<style lang="scss" scoped>
nav {
  z-index: 9999;
}
</style>
<template>
  <b-navbar toggleable="md" type="dark">
    <!-- Brand -->
    <b-navbar-brand class="mx-0">
      <router-link to="/timeline">
        <img
          class="img-fluid d-none d-md-block mx-2"
          src="@/assets/images/gov/bcid-logo-rev-en.svg"
          width="181"
          height="44"
          alt="Go to healthgateway timeline"
        />

        <img
          class="img-fluid d-md-none"
          src="@/assets/images/gov/bcid-symbol-rev.svg"
          width="64"
          height="44"
          alt="Go to healthgateway timeline"
        />
      </router-link>
    </b-navbar-brand>

    <b-navbar-brand class="px-0 pr-md-5 px-lg-5 mx-0">
      <h4 class="nav-link my-0 px-0 pr-md-5 pr-lg-5 mx-0" to="/timeLine">
        HealthGateway
      </h4>
    </b-navbar-brand>

    <b-navbar-toggle target="nav-collapse"></b-navbar-toggle>

    <!-- Navbar links -->
    <b-collapse id="nav-collapse" is-nav>
      <!-- Right aligned nav items -->
      <b-navbar-nav class="ml-auto">
        <router-link
          v-if="oidcIsAuthenticated"
          id="menuBtnLogin"
          class="nav-link"
          to="/logout"
        >
          <font-awesome-icon icon="sign-out-alt"></font-awesome-icon> Logout
        </router-link>
        <router-link v-else id="menuBtnLogin" class="nav-link" to="/login">
          <font-awesome-icon icon="sign-in-alt"></font-awesome-icon> Login
        </router-link>
      </b-navbar-nav>
    </b-collapse>
  </b-navbar>
</template>

<script lang="ts">
import Vue from "vue";
import { Prop, Component, Watch } from "vue-property-decorator";
import { Getter } from "vuex-class";
import { User as OidcUser } from "oidc-client";
import { IAuthenticationService } from "@/services/interfaces";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import User from "@/models/user";
import { library } from "@fortawesome/fontawesome-svg-core";
import {
  faStream,
  faSignInAlt,
  faSignOutAlt,
  faInfoCircle
} from "@fortawesome/free-solid-svg-icons";
library.add(faStream);
library.add(faSignInAlt);
library.add(faSignOutAlt);
library.add(faInfoCircle);

interface ILanguage {
  code: string;
  description: string;
}

const auth: string = "auth";
const user: string = "user";

@Component
export default class HeaderComponent extends Vue {
  @Getter("oidcIsAuthenticated", {
    namespace: auth
  })
  oidcIsAuthenticated!: boolean;

  @Getter("user", {
    namespace: user
  })
  user!: User;

  @Getter("userIsRegistered", {
    namespace: user
  })
  userIsRegistered!: boolean;

  @Getter("userIsActive", { namespace: user })
  userIsActive!: boolean;

  private authenticationService!: IAuthenticationService;

  private name: string = "";

  mounted() {
    this.authenticationService = container.get(
      SERVICE_IDENTIFIER.AuthenticationService
    );
  }

  get displayMenu(): boolean {
    return (
      this.oidcIsAuthenticated && this.userIsRegistered && this.userIsActive
    );
  }
}
</script>
