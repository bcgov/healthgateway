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

import AppTourComponent from "@/components/modal/AppTourComponent.vue";
import RatingComponent from "@/components/modal/RatingComponent.vue";
import type { WebClientConfiguration } from "@/models/configData";
import { DateWrapper, StringISODateTime } from "@/models/dateWrapper";
import Notification from "@/models/notification";
import User, { OidcUserInfo } from "@/models/user";
import { ILogger } from "@/services/interfaces";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { useRoute, useRouter } from "vue-router";
import { useConfigStore } from "@/stores/config";
import { useUserStore } from "@/stores/user";
import { useAuthStore } from "@/stores/auth";
import { useNotificationStore } from "@/stores/notification";
import { useNavbarStore } from "@/stores/navbar";
import { useAppStore } from "@/stores/app";

library.add(faBars, faSignInAlt, faSignOutAlt, faTimes, faUser, faLightbulb);

const sidebarId = "notification-centre-sidebar";
const minimumScrollChange = 2;

const appStore = useAppStore();
const configStore = useConfigStore();
const userStore = useUserStore();
const notificationStore = useNotificationStore();
const authStore = useAuthStore();
const navbarStore = useNavbarStore();

const route = useRoute();
const router = useRouter();
const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);

const lastScrollTop = ref(0);
const notificationButtonClicked = ref(false);
const hasViewedTour = ref(false);

const ratingComponent = ref<InstanceType<typeof RatingComponent>>();
const appTourComponent = ref<InstanceType<typeof AppTourComponent>>();

const isMobileWidth = computed<boolean>(() => appStore.isMobile);

const isOffline = computed<boolean>(() => configStore.isOffline);

const config = computed<WebClientConfiguration>(() => configStore.webConfig);

const oidcIsAuthenticated = computed<boolean>(
    () => authStore.oidcIsAuthenticated
);

const isValidIdentityProvider = computed<boolean>(
    () => userStore.isValidIdentityProvider
);

const isHeaderShown = computed<boolean>(() => navbarStore.isHeaderShown);

const isSidebarOpen = computed<boolean>(() => navbarStore.isSidebarOpen);

const user = computed<User>(() => userStore.user);

const userLastLoginDateTime = computed<StringISODateTime | undefined>(
    () => userStore.lastLoginDateTime
);

const oidcUserInfo = computed<OidcUserInfo | undefined>(
    () => userStore.oidcUserInfo
);

const userIsRegistered = computed<boolean>(() => userStore.userIsRegistered);

const userIsActive = computed<boolean>(() => userStore.userIsActive);

const patientRetrievalFailed = computed<boolean>(
    () => userStore.patientRetrievalFailed
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
    route.path.toLowerCase().startsWith("/pcrtest")
);

const isQueuePage = computed<boolean>(
    () =>
        route.path.toLowerCase() === "/queue" ||
        route.path.toLowerCase() === "/busy"
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
        const lastLoginDateTime = new DateWrapper(userLastLoginDateTime.value);
        return notificationStore.notifications.filter((n) =>
            new DateWrapper(n.scheduledDateTimeUtc, {
                isUtc: true,
                hasTime: true,
            }).isAfter(lastLoginDateTime)
        );
    }
    return notificationStore.notifications;
});

const hasNewNotifications = computed<boolean>(
    () => newNotifications.value.length > 0
);

const notificationBadgeContent = computed<string>(() => {
    const count = newNotifications.value.length;
    return hasNewNotifications.value ? count.toString() : "";
});

const highlightTourChangeIndicator = computed<boolean>(
    () => user.value.hasTourUpdated && !hasViewedTour.value
);

function toggleSidebar(): void {
    navbarStore.toggleSidebar();
}

function setHeaderState(isOpen: boolean): void {
    navbarStore.setHeaderState(isOpen);
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
    () => route.query,
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
    <v-app-bar
        :scroll-behavior="!isHeaderShown ? 'hide' : undefined"
        class="bg-primary hg-bottom-brand-border"
        border="bottom"
        border-color="secondary"
    >
        <template #prepend v-if="isSidebarButtonShown">
            <v-app-bar-nav-icon @click="handleToggleClick"></v-app-bar-nav-icon>
        </template>
        <v-img
            alt="Go to health Gateway home page"
            to="/"
            src="@/assets/images/gov/hg-logo-rev.svg"
            max-width="143px"
            class="pa-1"
        />
        <v-spacer></v-spacer>
        <v-btn
            v-if="isAppTourAvailable"
            icon
            @click="handleShowTourClick"
            data-testid="app-tour-button"
        >
            <v-badge color="red" :value="highlightTourChangeIndicator">
                <v-icon icon="fas fa-lightbulb"></v-icon>
            </v-badge>
        </v-btn>
        <v-btn
            v-if="isNotificationCentreAvailable"
            icon
            @click="notificationButtonClicked = true"
            data-testid="notification-centre-button"
        >
            <v-badge
                color="red"
                :value="hasNewNotifications"
                :content="notificationBadgeContent"
            >
                <v-icon icon="fas fa-bell"></v-icon>
            </v-badge>
        </v-btn>
        <template v-if="isLoggedInMenuShown">
            <v-btn id="menuBtnLogout" icon data-testid="headerDropdownBtn">
                <v-avatar data-testid="profileButtonInitials" color="info">
                    <span data-testid="profileButtonInitials">{{
                        userInitials
                    }}</span>
                </v-avatar>
            </v-btn>
            <v-menu activator="#menuBtnLogout">
                <v-list>
                    <v-list-item data-testid="profileUserName">{{
                        userName
                    }}</v-list-item>
                    <v-divider></v-divider>
                    <v-list-item
                        v-if="isProfileLinkAvailable"
                        to="/profile"
                        prepend-icon="fas fa-user"
                        data-testid="profileBtn"
                        >Profile</v-list-item
                    >
                    <v-list-item
                        @click="handleLogoutClick"
                        data-testid="logoutBtn"
                        >Log Out</v-list-item
                    >
                </v-list></v-menu
            >
        </template>
        <v-btn
            v-else-if="isLogInButtonShown"
            prepend-icon="fas fa-sign-in-alt"
            data-testid="loginBtn"
            to="/login"
        >
            Log In
        </v-btn>
        <v-btn
            v-else-if="isLogOutButtonShown"
            prepend-icon="fas fa-sign-out-alt"
            data-testid="header-log-out-button"
            to="/logout"
        >
            Log out
        </v-btn>
    </v-app-bar>
    <!--    <RatingComponent ref="ratingComponent" @on-close="processLogout()" />-->
    <!--    <AppTourComponent ref="appTourComponent" />-->
</template>

<style lang="scss" scoped>
.hg-bottom-brand-border {
    border-bottom: 4px solid #fcba19;
}
</style>
