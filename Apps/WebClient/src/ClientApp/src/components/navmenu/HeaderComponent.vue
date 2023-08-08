<script setup lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import {
    faBars,
    faLightbulb,
    faSignInAlt,
    faSignOutAlt,
    faTimes,
    faUser,
} from "@fortawesome/free-solid-svg-icons";
import { computed, nextTick, onUnmounted, ref, watch } from "vue";
import { useRoute, useRouter, useStore } from "vue-composition-wrapper";

import AppTourComponent from "@/components/modal/AppTourComponent.vue";
import RatingComponent from "@/components/modal/RatingComponent.vue";
import type { WebClientConfiguration } from "@/models/configData";
import { DateWrapper, StringISODateTime } from "@/models/dateWrapper";
import Notification from "@/models/notification";
import User, { OidcUserInfo } from "@/models/user";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger } from "@/services/interfaces";

library.add(faBars, faSignInAlt, faSignOutAlt, faTimes, faUser, faLightbulb);

const sidebarId = "notification-centre-sidebar";
const minimumScrollChange = 2;

const store = useStore();
const route = useRoute();
const router = useRouter();
const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);

const lastScrollTop = ref(0);
const notificationButtonClicked = ref(false);
const hasViewedTour = ref(false);

const ratingComponent = ref<InstanceType<typeof RatingComponent>>();
const appTourComponent = ref<InstanceType<typeof AppTourComponent>>();

const isMobileWidth = computed<boolean>(() => store.getters["isMobile"]);

const isOffline = computed<boolean>(() => store.getters["config/isOffline"]);

const config = computed<WebClientConfiguration>(
    () => store.getters["config/webClient"]
);

const oidcIsAuthenticated = computed<boolean>(
    () => store.getters["auth/oidcIsAuthenticated"]
);

const isValidIdentityProvider = computed<boolean>(
    () => store.getters["user/isValidIdentityProvider"]
);

const isHeaderShown = computed<boolean>(
    () => store.getters["navbar/isHeaderShown"]
);

const isSidebarOpen = computed<boolean>(
    () => store.getters["navbar/isSidebarOpen"]
);

const notifications = computed<Notification[]>(
    () => store.getters["notification/notifications"]
);

const user = computed<User>(() => store.getters["user/user"]);

const userLastLoginDateTime = computed<StringISODateTime | undefined>(
    () => store.getters["user/lastLoginDateTime"]
);

const oidcUserInfo = computed<OidcUserInfo | undefined>(
    () => store.getters["user/oidcUserInfo"]
);

const userIsRegistered = computed<boolean>(
    () => store.getters["user/userIsRegistered"]
);

const userIsActive = computed<boolean>(
    () => store.getters["user/userIsActive"]
);

const patientRetrievalFailed = computed<boolean>(
    () => store.getters["user/patientRetrievalFailed"]
);

const userName = computed<string>(() =>
    oidcUserInfo.value === undefined
        ? ""
        : `${oidcUserInfo.value.given_name} ${oidcUserInfo.value.family_name}`
);

const userInitials = computed<string>(() => {
    const first = oidcUserInfo.value?.given_name;
    const last = oidcUserInfo.value?.family_name;
    if (first && last) {
        return first.charAt(0) + last.charAt(0);
    } else if (first) {
        return first.charAt(0);
    } else if (last) {
        return last.charAt(0);
    } else {
        return "?";
    }
});

const isPcrTest = computed<boolean>(() =>
    route.value.path.toLowerCase().startsWith("/pcrtest")
);

const isQueuePage = computed<boolean>(
    () =>
        route.value.path.toLowerCase() === "/queue" ||
        route.value.path.toLowerCase() === "/busy"
);

const isSidebarButtonShown = computed<boolean>(
    () =>
        !isOffline.value &&
        oidcIsAuthenticated.value &&
        isValidIdentityProvider.value &&
        userIsRegistered.value &&
        userIsActive.value &&
        !patientRetrievalFailed.value &&
        !isQueuePage.value &&
        !isPcrTest.value &&
        isMobileWidth.value
);

const isNotificationCentreAvailable = computed<boolean>(
    () =>
        config.value.featureToggleConfiguration.notificationCentre.enabled &&
        !isOffline.value &&
        !isQueuePage.value &&
        !isPcrTest.value &&
        oidcIsAuthenticated.value &&
        isValidIdentityProvider.value &&
        userIsRegistered.value &&
        userIsActive.value &&
        !patientRetrievalFailed.value
);

const isAppTourAvailable = computed<boolean>(
    () =>
        !isOffline.value &&
        !isQueuePage.value &&
        !isPcrTest.value &&
        oidcIsAuthenticated.value &&
        isValidIdentityProvider.value &&
        userIsRegistered.value &&
        userIsActive.value &&
        !patientRetrievalFailed.value
);

const isLoggedInMenuShown = computed<boolean>(
    () => oidcIsAuthenticated.value && !isPcrTest.value && !isQueuePage.value
);

const isLogOutButtonShown = computed<boolean>(
    () => oidcIsAuthenticated.value && isPcrTest.value
);

const isLogInButtonShown = computed<boolean>(
    () =>
        !oidcIsAuthenticated.value &&
        !isOffline.value &&
        !isPcrTest.value &&
        !isQueuePage.value
);

const isProfileLinkAvailable = computed<boolean>(
    () =>
        isLoggedInMenuShown.value &&
        isValidIdentityProvider.value &&
        !patientRetrievalFailed.value
);

const newNotifications = computed<Notification[]>(() => {
    logger.debug(`User last login: ${userLastLoginDateTime.value}`);
    if (userLastLoginDateTime.value) {
        const lastLoginDateTime = new DateWrapper(userLastLoginDateTime.value, {
            isUtc: true,
        });
        return notifications.value.filter((n) =>
            new DateWrapper(n.scheduledDateTimeUtc, {
                isUtc: true,
            }).isAfter(lastLoginDateTime)
        );
    }
    return notifications.value;
});

const notificationBadgeContent = computed<string | boolean>(() => {
    const count = newNotifications.value.length;
    return count === 0 || notificationButtonClicked.value
        ? false
        : count.toString();
});

const highlightTourChangeIndicator = computed<boolean>(
    () => user.value.hasTourUpdated && !hasViewedTour.value
);

function toggleSidebar(): void {
    store.dispatch("navbar/toggleSidebar");
}

function setHeaderState(isOpen: boolean): void {
    store.dispatch("navbar/setHeaderState", isOpen);
}

function onScroll(): void {
    const st = window.scrollY || document.documentElement.scrollTop;
    if (
        Math.abs(st - lastScrollTop.value) > minimumScrollChange &&
        isMobileWidth.value
    ) {
        if (st > lastScrollTop.value) {
            // down-scroll
            if (isHeaderShown.value) {
                setHeaderState(false);
            }
        } else {
            // up-scroll
            if (!isHeaderShown.value) {
                setHeaderState(true);
            }
        }
    }
    // For Mobile or negative scrolling
    lastScrollTop.value = st <= 0 ? 0 : st;
}

function handleToggleClick(): void {
    toggleSidebar();
}

function handleLogoutClick(): void {
    if (isValidIdentityProvider.value) {
        showRating();
    } else {
        processLogout();
    }
}

function handleShowTourClick(): void {
    hasViewedTour.value = true;
    appTourComponent.value?.showModal();
}

function showRating(): void {
    ratingComponent.value?.showModal();
}

function processLogout(): void {
    logger.debug(`redirecting to logout view ...`);
    router.push({ path: "/logout" });
}

watch(isMobileWidth, (value) => {
    if (!value) {
        setHeaderState(false);
    }
});

watch(
    () => route.value.query,
    (value) => {
        if (value.registration === "success") {
            router.replace({ query: {} });
            appTourComponent.value?.showModal();
        }
    }
);

onUnmounted(() => {
    window.removeEventListener("scroll", onScroll);
});

nextTick(() => {
    window.addEventListener("scroll", onScroll);
    if (!isMobileWidth.value) {
        setHeaderState(false);
    }
});
</script>

<template>
    <header class="sticky-top" :class="{ 'nav-up': !isHeaderShown }">
        <b-navbar type="dark" aria-label="Top Nav" class="p-0">
            <!-- Hamburger toggle -->
            <hg-button
                v-if="isSidebarButtonShown"
                class="my-2 ml-2"
                variant="icon"
                @click="handleToggleClick"
            >
                <hg-icon
                    :icon="isSidebarOpen ? 'times' : 'bars'"
                    size="large"
                    class="menu-icon"
                    square
                />
            </hg-button>

            <!-- Brand -->
            <b-navbar-brand class="mr-0 ml-3 d-flex">
                <router-link to="/">
                    <img
                        class="img-fluid"
                        src="@/assets/images/gov/hg-logo-rev.svg"
                        width="143"
                        alt="Go to healthgateway home page"
                    />
                </router-link>
            </b-navbar-brand>

            <!-- Navbar links -->
            <b-navbar-nav class="nav-pills ml-auto">
                <b-avatar
                    v-if="isAppTourAvailable"
                    button
                    variant="transparent"
                    :badge="highlightTourChangeIndicator"
                    badge-variant="danger"
                    badge-top
                    icon="lightbulb"
                    class="text-white my-3 mx-2 rounded-0"
                    data-testid="app-tour-button"
                    @click="handleShowTourClick"
                />
                <b-avatar
                    v-if="isNotificationCentreAvailable"
                    v-b-toggle="sidebarId"
                    button
                    variant="transparent"
                    :badge="notificationBadgeContent"
                    badge-variant="danger"
                    badge-top
                    icon="bell"
                    class="text-white my-3 mx-2 rounded-0"
                    data-testid="notification-centre-button"
                    @click="notificationButtonClicked = true"
                />
                <b-nav-item-dropdown
                    v-if="isLoggedInMenuShown"
                    id="menuBtnLogout"
                    menu-class="drop-menu-position"
                    data-testid="headerDropdownBtn"
                    toggle-class="py-3 px-2 rounded-0"
                    right
                >
                    <!-- Using 'button-content' slot -->
                    <template #button-content>
                        <b-avatar
                            :text="userInitials"
                            class="mr-1 background-secondary"
                            data-testid="profileButtonInitials"
                        />
                    </template>
                    <span
                        class="dropdown-item-text text-center"
                        data-testid="profileUserName"
                    >
                        {{ userName }}
                    </span>
                    <b-dropdown-divider />
                    <b-dropdown-item
                        v-if="isProfileLinkAvailable"
                        id="menuBtnProfile"
                        data-testid="profileBtn"
                        to="/profile"
                    >
                        <hg-icon
                            icon="user"
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
                <b-nav-item
                    v-else-if="isLogInButtonShown"
                    id="menuBtnLogin"
                    data-testid="loginBtn"
                    link-classes="d-flex align-items-center"
                    to="/login"
                >
                    <hg-icon icon="sign-in-alt" size="large" class="mr-2" />
                    <span>Log In</span>
                </b-nav-item>
                <b-nav-item
                    v-else-if="isLogOutButtonShown"
                    id="header-log-out-button"
                    data-testid="header-log-out-button"
                    link-classes="d-flex align-items-center"
                    to="/logout"
                >
                    <hg-icon icon="sign-out-alt" size="large" class="mr-2" />
                    <span>Log Out</span>
                </b-nav-item>
            </b-navbar-nav>
        </b-navbar>

        <RatingComponent ref="ratingComponent" @on-close="processLogout()" />
        <AppTourComponent ref="appTourComponent" />
    </header>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

.sticky-top {
    transition: all 0.3s;
    z-index: $z_header;
}

.navbar {
    min-height: $header-height;
}

.nav-up {
    top: -74px;
}

.menu-icon {
    min-width: 1em;
    min-height: 1em;
}

nav {
    .navbar-brand {
        a {
            color: white;
            text-decoration: none;
        }

        a:hover {
            text-decoration: underline;
        }

        font-weight: bold;
        line-height: 1;
        font-size: 1rem;

        @media (min-width: 360px) {
            font-size: 1.25rem;
        }

        @media (min-width: 576px) {
            font-size: 1.5rem;
        }

        @media (max-width: 319px) {
            display: none !important;
        }
    }

    button {
        svg {
            width: 1.5em;
            height: 1.5em;
        }
    }

    .background-secondary {
        background-color: $hg-brand-secondary;
    }

    .dropdown-item-text {
        font-size: 0.875rem;
    }
}
</style>

<style lang="scss">
.drop-menu-position {
    position: absolute !important;
    min-width: 270px;
}
</style>
