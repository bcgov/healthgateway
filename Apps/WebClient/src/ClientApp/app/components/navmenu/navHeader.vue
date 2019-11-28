<template>
<b-navbar toggleable="lg" type="dark">
    <!-- Brand -->
    <b-navbar-brand>
        <router-link to="/timeLine">
            <img class="img-fluid d-none d-md-block" src="@/assets/images/gov/bcid-logo-rev-en.svg" width="181" height="44" alt="B.C. Government Logo" />
            <img class="img-fluid d-md-none" src="@/assets/images/gov/bcid-symbol-rev.svg" width="64" height="44" alt="B.C. Government Logo" />
            <b-navbar-brand>
                <h4 class="nav-link " to="/timeLine">HealthGateway</h4>
            </b-navbar-brand>
        </router-link>
    </b-navbar-brand>

    <b-navbar-toggle target="nav-collapse"></b-navbar-toggle>

    <!-- Navbar links -->
    <b-collapse id="nav-collapse" is-nav>
        <!-- Menu -->

        <b-navbar-nav v-if="displayRegistration">
            <router-link class="nav-link" to="/registration">
                <span class="fa fa-key"></span> Register
            </router-link>
        </b-navbar-nav>

        <!-- Right aligned nav items -->
        <b-navbar-nav class="ml-auto">
            <b-nav-item-dropdown v-if="oidcIsAuthenticated" id="menuBtndUser" :text="greeting" right variant="dark">

                <b-dropdown-item>
                    <router-link variant="primary" to="/timeLine">
                        <span class="fa fa-stream"></span> Timeline
                    </router-link>
                    <b-dropdown-divider />
                    <router-link variant="primary" id="menuBtnLogout" to="/logout">
                        <span class="fa fa-user"></span> Logout
                    </router-link>
                </b-dropdown-item>
            </b-nav-item-dropdown>
            <router-link v-else id="menuBtnLogin" class="nav-link" to="/login">
                <span class="fa fa-user"></span> Login
            </router-link>

        </b-navbar-nav>
    </b-collapse>
</b-navbar>
</template>

<script lang="ts">
import Vue from "vue";
import {
    Prop,
    Component,
    Watch
} from "vue-property-decorator";
import {
    Getter
} from "vuex-class";
import {
    User as OidcUser
} from "oidc-client";
import {
    IAuthenticationService
} from "@/services/interfaces";
import SERVICE_IDENTIFIER from "@/constants/serviceIdentifiers";
import container from "@/inversify.config";
import User from "@/models/user";

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
    }) user: User;
    @Getter("userIsRegistered", {
        namespace: user
    })
    userIsRegistered: boolean;

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
        let isLandingPage = this.$route.path === "/";
        if (
            this.oidcIsAuthenticated &&
            this.name &&
            this.userIsRegistered &&
            !isLandingPage
        ) {
            return true;
        }

        return false;
    }

    get displayRegistration(): boolean {
        console.log(this.userIsRegistered);
        if (this.oidcIsAuthenticated && !this.userIsRegistered) {
            return true;
        }
        return false;
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
