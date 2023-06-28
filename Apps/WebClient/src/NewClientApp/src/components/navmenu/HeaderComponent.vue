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

import RatingComponent from "@/components/modal/RatingComponent.vue";
import HgButtonComponent from "@/components/shared/HgButtonComponent.vue";
import HgIconButtonComponent from "@/components/shared/HgIconButtonComponent.vue";
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
import AppTourComponent from "@/components/modal/AppTourComponent.vue";

library.add(faBars, faSignInAlt, faSignOutAlt, faTimes, faUser, faLightbulb);

const sidebarId = "notification-centre-sidebar";
const headerScrollThreshold = 100;

const appStore = useAppStore();
const configStore = useConfigStore();
const userStore = useUserStore();
const notificationStore = useNotificationStore();
const authStore = useAuthStore();
const navbarStore = useNavbarStore();

const route = useRoute();
const router = useRouter();
const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);

const notificationButtonClicked = ref(false);
const hasViewedTour = ref(false);
const isScrollNearBottom = ref(false);

const ratingComponent = ref<InstanceType<typeof RatingComponent>>();

const isMobileWidth = computed<boolean>(() => appStore.isMobile);

const isOffline = computed<boolean>(() => configStore.isOffline);

const config = computed<WebClientConfiguration>(() => configStore.webConfig);

const oidcIsAuthenticated = computed<boolean>(
    () => authStore.oidcIsAuthenticated
);

const isValidIdentityProvider = computed<boolean>(
    () => userStore.isValidIdentityProvider
);

const isHeaderShown = computed<boolean>(
    () => navbarStore.isHeaderShown || isScrollNearBottom.value
);

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

const hasNewNotifications = computed(() => newNotifications.value.length > 0);

const notificationBadgeContent = computed(() => {
    const count = newNotifications.value.length;
    return hasNewNotifications.value ? count.toString() : "";
});

const highlightTourChangeIndicator = computed<boolean>(
    () => user.value.hasTourUpdated && !hasViewedTour.value
);

function testIfScrollIsNearBottom() {
    const scrollPosition = window.scrollY;
    const scrollHeight = document.body.scrollHeight;
    const clientHeight = document.documentElement.clientHeight;
    isScrollNearBottom.value =
        scrollPosition + clientHeight >= scrollHeight - headerScrollThreshold;
}

function toggleSidebar(): void {
    navbarStore.toggleSidebar();
}

function setHeaderState(isOpen: boolean): void {
    navbarStore.setHeaderState(isOpen);
}

function handleLogoutClick(): void {
    if (isValidIdentityProvider.value) {
        showRating();
    } else {
        processLogout();
    }
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
            // appTourComponent.value?.showModal();
        }
    }
);

onUnmounted(() => {
    window.removeEventListener("scroll", testIfScrollIsNearBottom);
});

nextTick(() => {
    window.addEventListener("scroll", testIfScrollIsNearBottom);
    if (!isMobileWidth.value) {
        setHeaderState(false);
    }
});
</script>

<template>
    <v-app-bar
        :scroll-behavior="!isHeaderShown ? 'hide' : undefined"
        class="border-b-md border-accent border-opacity-100 d-print-none"
        color="primary"
        :scroll-threshold="headerScrollThreshold"
    >
        <template #prepend v-if="isSidebarButtonShown">
            <HgIconButtonComponent
                :icon="isSidebarOpen ? 'fas fa-times' : 'fas fa-bars'"
                @click="toggleSidebar"
            />
        </template>
        <router-link to="/">
            <v-img
                alt="Go to Health Gateway home page"
                src="@/assets/images/gov/hg-logo-rev.svg"
                max-width="143px"
                class="pa-1"
            />
        </router-link>
        <v-spacer />
        <AppTourComponent
            v-if="isAppTourAvailable"
            :highlight-tour-change-indicator="highlightTourChangeIndicator"
        />
        <HgIconButtonComponent
            v-if="isNotificationCentreAvailable"
            @click="notificationButtonClicked = true"
            data-testid="notification-centre-button"
        >
            <v-badge
                color="red"
                :model-value="hasNewNotifications"
                :content="notificationBadgeContent"
            >
                <v-icon icon="fas fa-bell" />
            </v-badge>
        </HgIconButtonComponent>
        <template v-if="isLoggedInMenuShown">
            <HgIconButtonComponent
                id="menuBtnLogout"
                data-testid="headerDropdownBtn"
            >
                <v-avatar data-testid="profileButtonInitials" color="info">
                    {{ userInitials }}
                </v-avatar>
            </HgIconButtonComponent>
            <v-menu activator="#menuBtnLogout">
                <v-list>
                    <v-list-item
                        data-testid="profileUserName"
                        :title="userName"
                    />
                    <v-divider />
                    <v-list-item
                        v-if="isProfileLinkAvailable"
                        to="/profile"
                        prepend-icon="fas fa-user"
                        data-testid="profileBtn"
                        >Profile</v-list-item
                    >
                    <v-list-item
                        @click="handleLogoutClick"
                        prepend-icon="fas fa-sign-out-alt"
                        data-testid="logoutBtn"
                        >Log Out</v-list-item
                    >
                </v-list>
            </v-menu>
        </template>
        <HgButtonComponent
            v-else-if="isLogInButtonShown"
            variant="secondary"
            inverse
            prepend-icon="fas fa-sign-in-alt"
            data-testid="loginBtn"
            to="/login"
            text="Log In"
        />
        <HgButtonComponent
            v-else-if="isLogOutButtonShown"
            variant="secondary"
            inverse
            prepend-icon="fas fa-sign-out-alt"
            data-testid="header-log-out-button"
            to="/logout"
        >
            Log Out
        </HgButtonComponent>
    </v-app-bar>
    <RatingComponent ref="ratingComponent" @on-close="processLogout()" />
</template>
