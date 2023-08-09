<script setup lang="ts">
import { computed, nextTick, onUnmounted, ref, watch } from "vue";
import { useRoute, useRouter } from "vue-router";

import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import HgIconButtonComponent from "@/components/common/HgIconButtonComponent.vue";
import AppTourComponent from "@/components/private/home/AppTourComponent.vue";
import RatingComponent from "@/components/site/RatingComponent.vue";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { ILogger } from "@/services/interfaces";
import { useAppStore } from "@/stores/app";
import { useAuthStore } from "@/stores/auth";
import { useConfigStore } from "@/stores/config";
import { useNavigationStore } from "@/stores/navigation";
import { useNotificationStore } from "@/stores/notification";
import { useUserStore } from "@/stores/user";

const headerScrollThreshold = 100;

const appStore = useAppStore();
const configStore = useConfigStore();
const userStore = useUserStore();
const notificationStore = useNotificationStore();
const authStore = useAuthStore();
const navigationStore = useNavigationStore();

const route = useRoute();
const router = useRouter();
const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);

const notificationButtonClicked = ref(false);
const hasViewedTour = ref(false);
const isScrollNearBottom = ref(false);

const ratingComponent = ref<InstanceType<typeof RatingComponent>>();
const appTourComponent = ref<InstanceType<typeof AppTourComponent>>();

const isMobileWidth = computed(() => appStore.isMobile);
const isOffline = computed(() => configStore.isOffline);
const oidcIsAuthenticated = computed(() => authStore.oidcIsAuthenticated);
const isValidIdentityProvider = computed(
    () => userStore.isValidIdentityProvider
);
const isHeaderShown = computed(
    () => navigationStore.isHeaderShown || isScrollNearBottom.value
);
const isSidebarOpen = computed(() => navigationStore.isSidebarOpen);
const user = computed(() => userStore.user);
const userIsRegistered = computed(() => userStore.userIsRegistered);
const userIsActive = computed(() => userStore.userIsActive);
const patientRetrievalFailed = computed(() => userStore.patientRetrievalFailed);
const isPcrTest = computed(() =>
    route.path.toLowerCase().startsWith("/pcrtest")
);
const isQueuePage = computed(
    () =>
        route.path.toLowerCase() === "/queue" ||
        route.path.toLowerCase() === "/busy"
);
const isSidebarButtonShown = computed(
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
const isNotificationCentreAvailable = computed(
    () =>
        configStore.webConfig.featureToggleConfiguration.notificationCentre
            .enabled &&
        !isOffline.value &&
        !isQueuePage.value &&
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
        !isQueuePage.value &&
        !isPcrTest.value &&
        oidcIsAuthenticated.value &&
        isValidIdentityProvider.value &&
        userIsRegistered.value &&
        userIsActive.value &&
        !patientRetrievalFailed.value
);
const isLoggedInMenuShown = computed(
    () => oidcIsAuthenticated.value && !isPcrTest.value && !isQueuePage.value
);
const isLogOutButtonShown = computed(
    () => oidcIsAuthenticated.value && isPcrTest.value
);
const isLogInButtonShown = computed(
    () =>
        !oidcIsAuthenticated.value &&
        !isOffline.value &&
        !isPcrTest.value &&
        !isQueuePage.value
);
const isProfileLinkAvailable = computed(
    () =>
        isLoggedInMenuShown.value &&
        isValidIdentityProvider.value &&
        !patientRetrievalFailed.value
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
    navigationStore.toggleSidebar();
}

function setHeaderState(isOpen: boolean): void {
    navigationStore.setHeaderState(isOpen);
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

function showRating(): void {
    ratingComponent.value?.showDialog();
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
            appTourComponent.value?.showDialog();
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
        flat
    >
        <template v-if="isSidebarButtonShown" #prepend>
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
            data-testid="notification-centre-button"
            @click="handleNotificationCentreClick"
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
                class="mx-2"
            >
                <v-avatar data-testid="profileButtonInitials" color="info">
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
                        to="/profile"
                        prepend-icon="fas fa-user"
                        data-testid="profileBtn"
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
