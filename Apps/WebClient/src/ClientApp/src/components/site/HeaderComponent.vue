<script setup lang="ts">
import { computed, nextTick, onUnmounted, ref, watch } from "vue";
import { useRoute, useRouter } from "vue-router";

import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import HgIconButtonComponent from "@/components/common/HgIconButtonComponent.vue";
import AppTourComponent from "@/components/private/home/AppTourComponent.vue";
import RatingComponent from "@/components/site/RatingComponent.vue";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import {
    Action,
    Destination,
    InternalUrl,
    Text,
    Type,
} from "@/plugins/extensions";
import { ILogger, ITrackingService } from "@/services/interfaces";
import { useAuthStore } from "@/stores/auth";
import { useConfigStore } from "@/stores/config";
import { useLayoutStore } from "@/stores/layout";
import { useNotificationStore } from "@/stores/notification";
import { useUserStore } from "@/stores/user";

const headerScrollThreshold = 100;

const layoutStore = useLayoutStore();
const configStore = useConfigStore();
const userStore = useUserStore();
const notificationStore = useNotificationStore();
const authStore = useAuthStore();

const route = useRoute();
const router = useRouter();
const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const trackingService = container.get<ITrackingService>(
    SERVICE_IDENTIFIER.TrackingService
);

const notificationButtonClicked = ref(false);
const hasViewedTour = ref(false);
const isScrollNearBottom = ref(false);
const isHeaderVisible = ref();

const ratingComponent = ref<InstanceType<typeof RatingComponent>>();
const appTourComponent = ref<InstanceType<typeof AppTourComponent>>();

const isMobileWidth = computed(() => layoutStore.isMobile);
const isOffline = computed(() => configStore.isOffline);
const oidcIsAuthenticated = computed(() => authStore.oidcIsAuthenticated);
const isValidIdentityProvider = computed(
    () => userStore.isValidIdentityProvider
);
const isHeaderShown = computed(
    () => layoutStore.isHeaderShown || isScrollNearBottom.value
);
const user = computed(() => userStore.user);
const userIsRegistered = computed(() => userStore.userIsRegistered);
const userIsActive = computed(() => userStore.userIsActive);
const patientRetrievalFailed = computed(() => userStore.patientRetrievalFailed);
const isPcrTest = computed(() =>
    route.path.toLowerCase().startsWith("/pcrtest")
);
const isSidebarButtonShown = computed(
    () =>
        !isOffline.value &&
        oidcIsAuthenticated.value &&
        isValidIdentityProvider.value &&
        userIsRegistered.value &&
        userIsActive.value &&
        !patientRetrievalFailed.value &&
        !isPcrTest.value
);
const isNotificationCentreAvailable = computed(
    () =>
        configStore.webConfig.featureToggleConfiguration.notificationCentre
            .enabled &&
        !isOffline.value &&
        !isPcrTest.value &&
        oidcIsAuthenticated.value &&
        isValidIdentityProvider.value &&
        userIsRegistered.value &&
        userIsActive.value &&
        !patientRetrievalFailed.value
);
const isAppTourAvailable = computed(
    () =>
        !isOffline.value &&
        !isPcrTest.value &&
        oidcIsAuthenticated.value &&
        isValidIdentityProvider.value &&
        userIsRegistered.value &&
        userIsActive.value &&
        !patientRetrievalFailed.value
);
const isLoggedInMenuShown = computed(
    () => oidcIsAuthenticated.value && !isPcrTest.value
);
const isLogOutButtonShown = computed(
    () => oidcIsAuthenticated.value && isPcrTest.value
);
const isLogInButtonShown = computed(
    () =>
        !oidcIsAuthenticated.value &&
        !isOffline.value &&
        !isPcrTest.value &&
        route.path.toLowerCase() !== "/login"
);
const isProfileLinkAvailable = computed(
    () =>
        isLoggedInMenuShown.value &&
        isValidIdentityProvider.value &&
        !patientRetrievalFailed.value
);
const logoLinkDestination = computed(() =>
    !isOffline.value &&
    oidcIsAuthenticated.value &&
    isValidIdentityProvider.value &&
    userIsRegistered.value &&
    userIsActive.value &&
    !patientRetrievalFailed.value
        ? "/home"
        : "/"
);

const newNotifications = computed(() => notificationStore.newNotifications);
const hasNewNotifications = computed(
    () => newNotifications.value.length > 0 && !notificationButtonClicked.value
);
const notificationBadgeContent = computed(() => {
    const count = newNotifications.value.length;
    return hasNewNotifications.value ? count.toString() : "";
});
const highlightTourChangeIndicator = computed(
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
    layoutStore.toggleSidebar();
}

function setHeaderState(isOpen: boolean): void {
    layoutStore.setHeaderState(isOpen);
}

function handleLoginClick(): void {
    trackingService.trackEvent({
        action: Action.ButtonClick,
        text: Text.Login,
        type: Type.Login,
        url: InternalUrl.Login,
    });
    router.push({ path: "/login" });
}

function handleNotificationCentreClick(): void {
    notificationButtonClicked.value = true;
    notificationStore.isNotificationCenterOpen =
        !notificationStore.isNotificationCenterOpen;
}

function handleLogoutClick(): void {
    if (isValidIdentityProvider.value) {
        showRating();
    } else {
        processLogout();
    }
}

function handleProfileClick(): void {
    trackingService.trackEvent({
        action: Action.ButtonClick,
        text: Text.Profile,
        type: Type.Profile,
        url: InternalUrl.Profile,
    });
    router.push({ path: "/profile" });
}

function showRating(): void {
    ratingComponent.value?.showDialog();
}

function processLogout(): void {
    trackingService.trackEvent({
        action: Action.ButtonClick,
        text: Text.Logout,
        type: Type.Logout,
    });
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
            appTourComponent.value?.showDialog();
        }
    }
);

watch(
    () => route.path,
    () => {
        isHeaderVisible.value = true;
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
        v-model="isHeaderVisible"
        :scroll-behavior="!isHeaderShown ? 'hide' : undefined"
        class="border-b-md border-borderLight border-opacity-100 d-print-none"
        color="background"
        :scroll-threshold="headerScrollThreshold"
        flat
    >
        <template v-if="isSidebarButtonShown" #prepend>
            <HgIconButtonComponent
                icon="bars"
                data-testid="navbar-toggle-button"
                @click="toggleSidebar"
            />
        </template>
        <router-link
            :to="logoLinkDestination"
            class="px-2"
            style="max-width: 240px; width: 100%"
            @click="
                trackingService.trackEvent({
                    action: Action.InternalLink,
                    text: Text.HealthGatewayLogo,
                    destination: Destination.Home,
                    url: InternalUrl.Home,
                })
            "
        >
            <v-img
                alt="Go to Health Gateway home page"
                src="@/assets/images/gov/hg-logo-rev.svg"
                width="100%"
                height="auto"
                contain
            />
        </router-link>
        <v-spacer />
        {{
            /* AB#16942 Disable App Tour access via  :is-available="isAppTourAvailable && false" */ ""
        }}
        <AppTourComponent
            ref="appTourComponent"
            :is-available="isAppTourAvailable && false"
            :highlight-tour-change-indicator="highlightTourChangeIndicator"
            @click="hasViewedTour = true"
        />
        {{
            /* AB#16927 Disable notifications while aligning Classic with Salesforce version
               v-if="isNotificationCentreAvailable && false" */ ""
        }}
        <HgIconButtonComponent
            v-if="isNotificationCentreAvailable && false"
            data-testid="notification-centre-button"
            @click="handleNotificationCentreClick"
        >
            <v-badge
                color="red"
                :model-value="hasNewNotifications"
                :content="notificationBadgeContent"
            >
                <v-icon icon="fas fa-bell" class="text-grey-darken-1" />
            </v-badge>
        </HgIconButtonComponent>
        <template v-if="isLoggedInMenuShown">
            <HgIconButtonComponent
                id="menuBtnLogout"
                data-testid="headerDropdownBtn"
                class="mx-2"
            >
                <v-avatar
                    data-testid="profileButtonInitials"
                    color="grey-lighten-2"
                >
                    {{ userStore.userInitials }}
                </v-avatar>
            </HgIconButtonComponent>
            <v-menu activator="#menuBtnLogout">
                <v-list>
                    <v-list-item
                        data-testid="profileUserName"
                        :title="userStore.userName"
                    />
                    <v-divider />
                    <v-list-item
                        v-if="isProfileLinkAvailable"
                        prepend-icon="fas fa-user"
                        data-testid="profileBtn"
                        @click="handleProfileClick"
                        >Profile</v-list-item
                    >
                    <v-list-item
                        prepend-icon="fas fa-sign-out-alt"
                        data-testid="logoutBtn"
                        @click="handleLogoutClick"
                        >Log Out</v-list-item
                    >
                </v-list>
            </v-menu>
        </template>
        <HgButtonComponent
            v-else-if="isLogInButtonShown"
            variant="primary"
            prepend-icon="fas fa-sign-in-alt"
            class="mx-2"
            data-testid="loginBtn"
            text="Log in"
            :uppercase="false"
            @click="handleLoginClick"
        />
        <HgButtonComponent
            v-else-if="isLogOutButtonShown"
            variant="secondary"
            prepend-icon="fas fa-sign-out-alt"
            class="mx-2"
            data-testid="header-log-out-button"
            to="/logout"
            :uppercase="false"
        >
            Log out
        </HgButtonComponent>
    </v-app-bar>
    <RatingComponent ref="ratingComponent" @on-close="processLogout()" />
</template>
