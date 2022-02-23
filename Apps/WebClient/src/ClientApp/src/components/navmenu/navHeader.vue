<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import {
    faBars,
    faSignInAlt,
    faSignOutAlt,
    faTimes,
    faUserCircle,
} from "@fortawesome/free-solid-svg-icons";
import Vue from "vue";
import { Component, Ref, Watch } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import RatingComponent from "@/components/modal/rating.vue";
import User, { OidcUserProfile } from "@/models/user";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import { IAuthenticationService, ILogger } from "@/services/interfaces";

library.add(faBars, faSignInAlt, faSignOutAlt, faTimes, faUserCircle);

@Component({
    components: {
        RatingComponent,
    },
})
export default class HeaderComponent extends Vue {
    @Action("toggleSidebar", { namespace: "navbar" })
    toggleSidebar!: () => void;

    @Action("setHeaderState", { namespace: "navbar" })
    setHeaderState!: (isOpen: boolean) => void;

    @Getter("isMobile")
    isMobileWidth!: boolean;

    @Getter("isOffline", { namespace: "config" })
    isOffline!: boolean;

    @Getter("oidcIsAuthenticated", { namespace: "auth" })
    oidcIsAuthenticated!: boolean;

    @Getter("isValidIdentityProvider", { namespace: "auth" })
    isValidIdentityProvider!: boolean;

    @Getter("isHeaderShown", { namespace: "navbar" })
    isHeaderShown!: boolean;

    @Getter("isSidebarOpen", { namespace: "navbar" })
    isSidebarOpen!: boolean;

    @Getter("isSidebarAvailable", { namespace: "navbar" })
    isSidebarAvailable!: boolean;

    @Getter("user", { namespace: "user" })
    user!: User;

    @Ref("ratingComponent")
    readonly ratingComponent!: RatingComponent;

    private oidcUser: OidcUserProfile | null = null;

    private logger!: ILogger;
    private authenticationService!: IAuthenticationService;

    private lastScrollTop = 0;
    private static minimunScrollChange = 2;

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
    private loadOidcUserOnChange() {
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
        this.loadOidcUserOnChange();
    }

    private destroyed() {
        window.removeEventListener("scroll", this.onScroll);
    }

    private get isPcrTest(): boolean {
        return this.$route.path.toLowerCase().startsWith("/pcrtest");
    }

    private get isSidebarButtonShown(): boolean {
        return this.isSidebarAvailable && !this.isPcrTest && this.isMobileWidth;
    }

    private get isLoggedInMenuShown(): boolean {
        return this.oidcIsAuthenticated && !this.isPcrTest;
    }

    private get isLogOutButtonShown(): boolean {
        return this.oidcIsAuthenticated && this.isPcrTest;
    }

    private get isLogInButtonShown(): boolean {
        return !this.oidcIsAuthenticated && !this.isOffline && !this.isPcrTest;
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
            <hg-button
                v-if="isSidebarButtonShown"
                class="mr-2"
                variant="icon"
                @click="handleToggleClick"
            >
                <hg-icon
                    :icon="isSidebarOpen ? 'times' : 'bars'"
                    size="large"
                    class="menu-icon"
                />
            </hg-button>

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
                    v-if="isLoggedInMenuShown"
                    id="menuBtnLogout"
                    menu-class="drop-menu-position"
                    data-testid="headerDropdownBtn"
                    :no-caret="true"
                    toggle-class="p-0 m-0 mr-1"
                    right
                >
                    <!-- Using 'button-content' slot -->
                    <template #button-content class="align-middle">
                        <b-row class="p-0 m-0 align-items-center">
                            <b-col class="p-0 m-0">
                                <hg-icon
                                    icon="user-circle"
                                    size="large"
                                    fixed-width
                                    class="profile-menu"
                                />
                            </b-col>
                            <b-col v-if="!isMobileWidth" class="p-0 m-0 ml-2">
                                <span data-testid="profileButtonUserName">
                                    {{ userName }}
                                </span>
                            </b-col>
                        </b-row>
                    </template>
                    <span v-if="isMobileWidth">
                        <b-dropdown-item class="text-center">
                            <span data-testid="profileUserNameMobileOnly">
                                {{ userName }}
                            </span>
                        </b-dropdown-item>
                        <b-dropdown-divider />
                    </span>
                    <b-dropdown-item
                        id="menuBtnProfile"
                        data-testid="profileBtn"
                        to="/profile"
                    >
                        <hg-icon
                            icon="user-circle"
                            size="medium"
                            data-testid="profileDropDownIcon"
                            class="mr-2"
                            fixed-width
                        />
                        <span data-testid="profileDropDownLabel">
                            Profile
                        </span>
                    </b-dropdown-item>
                    <b-dropdown-item-button
                        data-testid="logoutBtn"
                        @click="handleLogoutClick()"
                    >
                        <hg-icon
                            icon="sign-out-alt"
                            size="medium"
                            data-testid="logoutDropDownIcon"
                            class="mr-2"
                            fixed-width
                        />
                        <span>Log Out</span>
                    </b-dropdown-item-button>
                </b-nav-item-dropdown>
                <router-link
                    v-else-if="isLogInButtonShown"
                    id="menuBtnLogin"
                    data-testid="loginBtn"
                    class="nav-link d-flex align-items-center"
                    to="/login"
                >
                    <hg-icon icon="sign-in-alt" size="large" class="mr-2" />
                    <span>Log In</span>
                </router-link>
                <router-link
                    v-else-if="isLogOutButtonShown"
                    id="header-log-out-button"
                    data-testid="header-log-out-button"
                    class="nav-link d-flex align-items-center"
                    to="/logout"
                >
                    <hg-icon icon="sign-out-alt" size="large" class="mr-2" />
                    <span>Log Out</span>
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
    min-width: 1em;
    min-height: 1em;
}

nav {
    a h4 {
        text-decoration: none;
        color: white;
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
