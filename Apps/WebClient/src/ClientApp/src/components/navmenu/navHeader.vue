<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

nav {
    z-index: $z_top_layer;

    a h4 {
        text-decoration: none;
        color: white;
    }
    a:hover h4 {
        text-decoration: underline;
    }
}
</style>
<template>
    <b-navbar toggleable="md" type="dark">
        <!-- Hamburger toggle -->
        <b-navbar-toggle
            v-if="displayMenu"
            class="mr-1"
            target="NONE"
            @click="toggleSidebar"
        ></b-navbar-toggle>

        <!-- Brand -->
        <b-navbar-brand class="mx-0">
            <router-link to="/timeline">
                <img
                    class="img-fluid d-none d-md-block mx-1"
                    src="@/assets/images/gov/bcid-logo-rev-en.svg"
                    width="181"
                    height="44"
                    alt="Go to healthgateway timeline"
                />

                <img
                    class="img-fluid d-md-none"
                    src="@/assets/images/gov/bcid-symbol-rev.svg"
                    width="30"
                    height="44"
                    alt="Go to healthgateway timeline"
                />
            </router-link>
        </b-navbar-brand>
        <b-navbar-brand class="px-0 pr-md-5 px-lg-5 mx-0">
            <router-link
                to="/timeline"
                class="nav-link my-0 px-0 pr-md-5 pr-lg-5 mx-0"
            >
                <h4 class="my-0 px-0 pr-md-5 pr-lg-5 mx-0">HealthGateway</h4>
            </router-link>
        </b-navbar-brand>

        <!-- Navbar links -->
        <b-navbar-nav class="ml-auto">
            <router-link
                v-if="oidcIsAuthenticated"
                id="menuBtnLogin"
                class="nav-link"
                to="/logout"
            >
                <font-awesome-icon icon="sign-out-alt"></font-awesome-icon
                >Logout
            </router-link>
            <router-link v-else id="menuBtnLogin" class="nav-link" to="/login">
                <font-awesome-icon icon="sign-in-alt"></font-awesome-icon> Login
            </router-link>
        </b-navbar-nav>
    </b-navbar>
</template>

<script lang="ts">
import Vue from "vue";
import { Component, Prop, Watch } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";
import { User as OidcUser } from "oidc-client";
import { IAuthenticationService } from "@/services/interfaces";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import User from "@/models/user";
import { library } from "@fortawesome/fontawesome-svg-core";
import { faSignInAlt, faSignOutAlt } from "@fortawesome/free-solid-svg-icons";
library.add(faSignInAlt);
library.add(faSignOutAlt);

interface ILanguage {
    code: string;
    description: string;
}

const auth: string = "auth";
const user: string = "user";
const sidebar: string = "sidebar";

@Component
export default class HeaderComponent extends Vue {
    @Action("toggleSidebar", { namespace: sidebar }) toggleSidebar!: () => void;
    @Getter("isOpen", { namespace: sidebar }) isOpen!: boolean;

    @Getter("oidcIsAuthenticated", {
        namespace: auth,
    })
    oidcIsAuthenticated!: boolean;

    @Getter("userIsRegistered", {
        namespace: user,
    })
    userIsRegistered!: boolean;

    @Getter("userIsActive", { namespace: user })
    userIsActive!: boolean;

    private authenticationService!: IAuthenticationService;

    private mounted() {
        this.authenticationService = container.get(
            SERVICE_IDENTIFIER.AuthenticationService
        );
    }

    private get displayMenu(): boolean {
        return (
            this.oidcIsAuthenticated &&
            this.userIsRegistered &&
            this.userIsActive
        );
    }

    private toggleMenu() {
        this.toggleSidebar();
    }
}
</script>
