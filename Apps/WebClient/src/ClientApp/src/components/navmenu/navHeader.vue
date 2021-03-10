<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faSignInAlt, faSignOutAlt } from "@fortawesome/free-solid-svg-icons";
import Vue from "vue";
import { Component, Ref, Watch } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import RatingComponent from "@/components/modal/rating.vue";
import User, { OidcUserProfile } from "@/models/user";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { IAuthenticationService, ILogger } from "@/services/interfaces";
library.add(faSignInAlt);
library.add(faSignOutAlt);

const auth = "auth";
const user = "user";
const navbar = "navbar";
const config = "config";

@Component({
    components: {
        RatingComponent,
    },
})
export default class HeaderComponent extends Vue {
    @Action("toggleSidebar", { namespace: navbar }) toggleSidebar!: () => void;
    @Action("setHeaderState", { namespace: navbar }) setHeaderState!: (
        isOpen: boolean
    ) => void;

    @Getter("isMobile") isMobileWidth!: boolean;
    @Getter("isSidebarOpen", { namespace: navbar }) isSidebarOpen!: boolean;
    @Getter("isHeaderShown", { namespace: navbar }) isHeaderShown!: boolean;
    @Getter("isOffline", {
        namespace: config,
    })
    isOffline!: boolean;

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

    @Getter("user", { namespace: "user" }) user!: User;

    @Getter("isValidIdentityProvider", {
        namespace: auth,
    })
    isValidIdentityProvider!: boolean;

    @Ref("ratingComponent")
    readonly ratingComponent!: RatingComponent;

    private oidcUser: OidcUserProfile | null = null;

    private logger!: ILogger;
    private authenticationService!: IAuthenticationService;

    private lastScrollTop = 0;
    private static minimunScrollChange = 2;

    private get displayMenu(): boolean {
        return (
            !this.isOffline &&
            this.oidcIsAuthenticated &&
            this.userIsRegistered &&
            this.userIsActive
        );
    }

    private get userName(): string {
        return this.oidcUser
            ? this.oidcUser.given_name + " " + this.oidcUser.family_name
            : "";
    }

    @Watch("isMobileWidth")
    private onMobileWidth() {
        if (!this.isMobileWidth) {
            this.setHeaderState(false);
        }
    }

    @Watch("oidcIsAuthenticated")
    private onPropertyChanged() {
        // If there is no name in the scope, retrieve it from the service.
        if (this.oidcIsAuthenticated) {
            this.loadOidcUser();
        }
    }

    private created() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        // Load the user name and current email
        this.authenticationService = container.get<IAuthenticationService>(
            SERVICE_IDENTIFIER.AuthenticationService
        );
        this.$nextTick(() => {
            window.addEventListener("scroll", this.onScroll);
            if (!this.isMobileWidth) {
                this.setHeaderState(false);
            }
        });
    }

    private mounted() {
        if (this.oidcIsAuthenticated) {
            this.loadOidcUser();
        }
    }

    private destroyed() {
        window.removeEventListener("scroll", this.onScroll);
    }

    private onScroll() {
        var st = window.pageYOffset || document.documentElement.scrollTop;
        if (
            Math.abs(st - this.lastScrollTop) >
                HeaderComponent.minimunScrollChange &&
            this.isMobileWidth
        ) {
            if (st > this.lastScrollTop) {
                // downscroll
                if (this.isHeaderShown) {
                    this.setHeaderState(false);
                }
            } else {
                // upscroll
                if (!this.isHeaderShown) {
                    this.setHeaderState(true);
                }
            }
        }
        // For Mobile or negative scrolling
        this.lastScrollTop = st <= 0 ? 0 : st;
    }

    private handleToggleClick() {
        this.toggleSidebar();
    }

    private toggleMenu() {
        this.toggleSidebar();
    }

    private loadOidcUser(): void {
        this.authenticationService.getOidcUserProfile().then((oidcUser) => {
            if (oidcUser) {
                this.oidcUser = oidcUser;
            }
        });
    }

    private handleLogoutClick() {
        if (this.isValidIdentityProvider) {
            this.showRating();
        } else {
            this.processLogout();
        }
    }

    private showRating() {
        this.ratingComponent.showModal();
    }

    private processLogout() {
        this.logger.debug(`redirecting to logout view ...`);
        this.$router.push({ path: "/logout" });
    }
}
</script>

<template>
    <header class="sticky-top" :class="{ 'nav-up': !isHeaderShown }">
        <b-navbar toggleable="md" type="dark">
            <!-- Hamburger toggle -->
            <span
                v-if="displayMenu"
                class="navbar-toggler mr-1"
                displayMenu
                @click="handleToggleClick"
            >
                <b-icon
                    v-if="isSidebarOpen"
                    icon="x"
                    class="menu-icon"
                ></b-icon>
                <b-icon v-else icon="list" class="menu-icon"></b-icon>
            </span>

            <!-- Brand -->
            <b-navbar-brand class="mx-0">
                <router-link to="/">
                    <img
                        class="img-fluid d-none d-md-block mx-1"
                        src="@/assets/images/gov/bcid-logo-rev-en.svg"
                        width="181"
                        height="44"
                        alt="Go to healthgateway home page"
                    />

                    <img
                        class="img-fluid d-md-none"
                        src="@/assets/images/gov/bcid-symbol-rev.svg"
                        width="30"
                        height="44"
                        alt="Go to healthgateway home page"
                    />
                </router-link>
            </b-navbar-brand>
            <b-navbar-brand class="px-0 pr-md-5 px-lg-5 mx-0">
                <router-link
                    to="/"
                    class="nav-link my-0 px-0 pr-md-5 pr-lg-5 mx-0"
                >
                    <h4 class="my-0 px-0 pr-md-5 pr-lg-5 mx-0">
                        Health Gateway
                    </h4>
                </router-link>
            </b-navbar-brand>

            <!-- Navbar links -->
            <b-navbar-nav class="ml-auto">
                <b-nav-item-dropdown
                    v-if="oidcIsAuthenticated"
                    id="menuBtnLogout"
                    menu-class="drop-menu-position"
                    data-testid="headerDropdownBtn"
                    :no-caret="true"
                    toggle-class="p-0 m-0 pr-1"
                    right
                >
                    <!-- Using 'button-content' slot -->
                    <template #button-content class="align-middle">
                        <b-row class="p-0 m-0 align-items-center">
                            <b-col class="p-0 m-0">
                                <font-awesome-icon
                                    icon="user-circle"
                                    class="profile-menu p-0 m-0"
                                ></font-awesome-icon>
                            </b-col>
                            <b-col v-if="!isMobileWidth" class="p-0 m-0 pl-2">
                                <span data-testid="profileButtonUserName">{{
                                    userName
                                }}</span>
                            </b-col>
                        </b-row>
                    </template>

                    <span v-if="isMobileWidth">
                        <b-dropdown-item>
                            <b-row>
                                <b-col cols="1"></b-col>
                                <b-col>
                                    <span
                                        data-testid="profileUserNameMobileOnly"
                                    >
                                        {{ userName }}
                                    </span>
                                </b-col>
                            </b-row>
                        </b-dropdown-item>
                        <b-dropdown-divider />
                    </span>
                    <b-dropdown-item
                        id="menuBtnProfile"
                        data-testid="profileBtn"
                        to="/profile"
                    >
                        <b-row>
                            <b-col cols="1">
                                <font-awesome-icon
                                    data-testid="profileDropDownIcon"
                                    icon="user-circle"
                                    class="p-0 m-0"
                                >
                                </font-awesome-icon>
                            </b-col>
                            <b-col
                                ><span data-testid="profileDropDownLabel"
                                    >Profile</span
                                >
                            </b-col>
                        </b-row>
                    </b-dropdown-item>
                    <b-dropdown-item-button
                        data-testid="logoutBtn"
                        @click="handleLogoutClick()"
                    >
                        <b-row>
                            <b-col cols="1">
                                <font-awesome-icon
                                    data-testid="logoutDropDownIcon"
                                    icon="sign-out-alt"
                                    class="p-0 m-0"
                                >
                                </font-awesome-icon>
                            </b-col>
                            <b-col>Logout</b-col>
                        </b-row>
                    </b-dropdown-item-button>
                </b-nav-item-dropdown>

                <router-link
                    v-else-if="!isOffline"
                    id="menuBtnLogin"
                    data-testid="loginBtn"
                    class="nav-link"
                    to="/login"
                >
                    <font-awesome-icon icon="sign-in-alt"></font-awesome-icon>
                    <span class="pl-1">Login</span>
                </router-link>
            </b-navbar-nav>
            <RatingComponent
                ref="ratingComponent"
                @on-close="processLogout()"
            />
        </b-navbar>
    </header>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

.sticky-top {
    transition: all 0.3s;
    z-index: $z_header;
}

.navbar {
    padding-left: 8px;
    padding-right: 8px;
    min-height: $header-height;
}

.nav-up {
    top: -70px;
    @media (max-width: 767px) {
        top: -66px;
    }
}

.menu-icon {
    width: 1.5em;
    height: 1.5em;
}

nav {
    a h4 {
        text-decoration: none;
        color: white;
        @media (max-width: 360px) {
            font-size: 1em !important;
        }
    }
    a:hover h4 {
        text-decoration: underline;
    }
    button {
        svg {
            width: 1.5em;
            height: 1.5em;
        }
    }
    .nav-link {
        cursor: pointer;
    }
}

.profile-menu {
    font-size: 2em;
    color: white;
}
</style>

<style lang="scss">
.drop-menu-position {
    position: absolute !important;
}
</style>
