<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faSignInAlt, faSignOutAlt } from "@fortawesome/free-solid-svg-icons";
import Vue from "vue";
import { Component, Ref, Watch } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import RatingComponent from "@/components/modal/rating.vue";
import ScreenWidth from "@/constants/screenWidth";
import User from "@/models/user";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { ILogger } from "@/services/interfaces";
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

    private logger!: ILogger;

    private windowWidth = 0;
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

    private get isMobileWidth(): boolean {
        return this.windowWidth < ScreenWidth.Mobile;
    }

    @Watch("isMobileWidth")
    private onMobileWidth() {
        if (!this.isMobileWidth) {
            this.setHeaderState(false);
        }
    }

    private mounted() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
    }

    private created() {
        this.windowWidth = window.innerWidth;
        this.$nextTick(() => {
            window.addEventListener("scroll", this.onScroll);
            window.addEventListener("resize", this.onResize);
            if (!this.isMobileWidth) {
                this.setHeaderState(false);
            }
        });
    }

    private destroyed() {
        window.removeEventListener("scroll", this.onScroll);
        window.removeEventListener("resize", this.onResize);
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

    private onResize() {
        this.windowWidth = window.innerWidth;
    }

    private handleToggleClick() {
        this.toggleSidebar();
    }

    private toggleMenu() {
        this.toggleSidebar();
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
                    class="icon-class"
                ></b-icon>
                <b-icon v-else icon="list" class="icon-class"></b-icon>
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
                <div
                    v-if="oidcIsAuthenticated"
                    id="menuBtnLogout"
                    data-testid="logoutBtn"
                    variant="link"
                    class="nav-link"
                    @click="handleLogoutClick()"
                >
                    <font-awesome-icon icon="sign-out-alt"></font-awesome-icon>
                    <span class="pl-1">Logout</span>
                </div>
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
}

.navbar {
    padding-left: 8px;
    padding-right: 8px;
}

.nav-up {
    top: -70px;
    @media (max-width: 767px) {
        top: -66px;
    }
}

.icon-class {
    width: 1.5em;
    height: 1.5em;
}

nav {
    z-index: $z_header;

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
</style>
