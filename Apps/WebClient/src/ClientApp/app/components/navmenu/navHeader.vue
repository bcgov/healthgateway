<template>
  <b-navbar toggleable="lg" type="dark">
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
        <b-nav-item-dropdown
          v-if="oidcIsAuthenticated"
          id="menuBtndUser"
          :text="greeting"
          right
          variant="dark"
        >
          <b-dropdown-item v-if="displayMenu">
            <router-link id="menuBtnTimeline" variant="primary" to="/timeline">
              <font-awesome-icon icon="stream"></font-awesome-icon> Timeline
            </router-link>
          </b-dropdown-item>
          <b-dropdown-divider v-if="displayMenu" />
          <b-dropdown-item v-if="displayMenu">
            <router-link id="menuBtnProfile" variant="primary" to="/profile">
              <font-awesome-icon icon="user"></font-awesome-icon> Profile
            </router-link>
          </b-dropdown-item>
          <b-dropdown-item
            v-if="displayMenu"
            href="https://github.com/bcgov/healthgateway/wiki"
          >
            <a
              href="https://github.com/bcgov/healthgateway/wiki"
              target="_blank"
            >
              <font-awesome-icon icon="info-circle"></font-awesome-icon> About
            </a>
          </b-dropdown-item>
          <b-dropdown-item>
            <router-link id="menuBtnLogout" variant="primary" to="/logout">
              <font-awesome-icon icon="sign-out-alt"></font-awesome-icon> Logout
            </router-link>
          </b-dropdown-item>
        </b-nav-item-dropdown>
        <router-link v-else id="menuBtnLogin" class="nav-link" to="/login">
          <font-awesome-icon icon="user"></font-awesome-icon> Login
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
  faSignOutAlt,
  faInfoCircle
} from "@fortawesome/free-solid-svg-icons";
library.add(faStream);
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
  oidcIsAuthenticated: boolean;

  @Getter("user", {
    namespace: user
  })
  user: User;

  @Getter("userIsRegistered", {
    namespace: user
  })
  userIsRegistered: boolean;

  @Getter("userIsActive", { namespace: user })
  userIsActive: boolean;

  private authenticationService: IAuthenticationService;

  private name: string = "";

  @Watch("oidcIsAuthenticated")
  onPropertyChanged() {
    // If there is no name in the scope, retrieve it from the service.
    if (this.oidcIsAuthenticated && !this.name) {
      this.loadName();
    }
  }

  mounted() {
    this.authenticationService = container.get(
      SERVICE_IDENTIFIER.AuthenticationService
    );
    if (this.oidcIsAuthenticated) {
      this.loadName();
    }
  }

  get displayMenu(): boolean {
    return (
      this.oidcIsAuthenticated && this.userIsRegistered && this.userIsActive
    );
  }

  get greeting(): string {
    if (this.oidcIsAuthenticated && this.name) {
      return "Hi " + this.name;
    } else {
      return "";
    }
  }

  private loadName(): void {
    this.authenticationService.getOidcUserProfile().then(oidcUser => {
      if (oidcUser) {
        this.name = this.getFullname(oidcUser.given_name, oidcUser.family_name);
      }
    });
  }

  private getFullname(firstName: string, lastName: string): string {
    return firstName + " " + lastName;
  }
}
</script>
