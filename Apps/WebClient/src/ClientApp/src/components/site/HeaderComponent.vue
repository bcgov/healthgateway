<script setup lang="ts">
import { computed, nextTick, onUnmounted, ref, watch } from "vue";
import { useRoute, useRouter } from "vue-router";

import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import HgIconButtonComponent from "@/components/common/HgIconButtonComponent.vue";
import RatingComponent from "@/components/site/RatingComponent.vue";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { ILogger } from "@/services/interfaces";
import { useAuthStore } from "@/stores/auth";
import { useConfigStore } from "@/stores/config";
import { useLayoutStore } from "@/stores/layout";
import { useUserStore } from "@/stores/user";

const headerScrollThreshold = 100;

const layoutStore = useLayoutStore();
const configStore = useConfigStore();
const userStore = useUserStore();
/* AB#16927 Disable notifications while aligning Classic with Salesforce version 
const notificationStore = useNotificationStore(); 
*/
const authStore = useAuthStore();

const route = useRoute();
const router = useRouter();
const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);

/* AB#16927 Disable notifications while aligning Classic with Salesforce version 
const notificationButtonClicked = ref(false); 
*/
/* AB#16927 Disable app tour while aligning Classic with Salesforce version
const hasViewedTour = ref(false); 
*/
const isScrollNearBottom = ref(false);
const isHeaderVisible = ref();

const ratingComponent = ref<InstanceType<typeof RatingComponent>>();
/* AB#16927 Disable app tour while aligning Classic with Salesforce version
const appTourComponent = ref<InstanceType<typeof AppTourComponent>>(); 
*/

const isMobileWidth = computed(() => layoutStore.isMobile);
const isOffline = computed(() => configStore.isOffline);
const oidcIsAuthenticated = computed(() => authStore.oidcIsAuthenticated);
const isValidIdentityProvider = computed(
    () => userStore.isValidIdentityProvider
);
const isHeaderShown = computed(
    () => layoutStore.isHeaderShown || isScrollNearBottom.value
);
/* AB#16927 Disable notifications while aligning Classic with Salesforce version 
const user = computed(() => userStore.user); 
*/
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
/* AB#16927 Disable notifications while aligning Classic with Salesforce version 
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
*/
/* AB#16927 Disable app tour while aligning Classic with Salesforce version 
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
*/
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
/* AB#16927 Disable notifications while aligning Classic with Salesforce version 
const newNotifications = computed(() => notificationStore.newNotifications); 
*/
/* AB#16927 Disable notifications while aligning Classic with Salesforce version 
const hasNewNotifications = computed(
    () => newNotifications.value.length > 0 && !notificationButtonClicked.value
); 
*/
/* AB#16927 Disable notifications while aligning Classic with Salesforce version 
const notificationBadgeContent = computed(() => {
    const count = newNotifications.value.length;
    return hasNewNotifications.value ? count.toString() : "";
}); 
*/
/* AB#16927 Disable app tour while aligning Classic with Salesforce version 
const highlightTourChangeIndicator = computed(
    () => user.value.hasTourUpdated && !hasViewedTour.value
); 
*/

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

/* AB#16927 Disable notifications while aligning Classic with Salesforce version 
function handleNotificationCentreClick(): void {
    notificationButtonClicked.value = true;
    notificationStore.isNotificationCenterOpen =
        !notificationStore.isNotificationCenterOpen;
} 
*/

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

/* AB#16927 Disable app tour while aligning Classic with Salesforce version 
watch(
    () => route.query,
    (value) => {
        if (value.registration === "success") {
            router.replace({ query: {} });
            appTourComponent.value?.showDialog();
        }
    }
); 
*/

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
        class="border-b-md border-accent border-opacity-100 d-print-none"
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
        <router-link to="/" class="px-2" style="max-width: 240px; width: 100%">
            <v-img
                alt="Go to Health Gateway home page"
                src="@/assets/images/gov/hg-logo-rev.svg"
                width="100%"
                height="auto"
                contain
            />
        </router-link>
        <v-spacer />
        <!-- AB#16927 Disable app tour while aligning Classic with Salesforce version 
        <AppTourComponent
            ref="appTourComponent"
            :is-available="isAppTourAvailable"
            :highlight-tour-change-indicator="highlightTourChangeIndicator"
            @click="hasViewedTour = true"
        /> 
        -->
        <!-- AB#16927 Disable notifications while aligning Classic with Salesforce version 
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
                <v-icon icon="fas fa-bell" class="text-grey-darken-1" />
            </v-badge>
        </HgIconButtonComponent> 
        -->
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
            variant="white"
            prepend-icon="fas fa-sign-in-alt"
            data-testid="loginBtn"
            to="/login"
            text="Log In"
        />
        <HgButtonComponent
            v-else-if="isLogOutButtonShown"
            variant="white"
            prepend-icon="fas fa-sign-out-alt"
            data-testid="header-log-out-button"
            to="/logout"
        >
            Log Out
        </HgButtonComponent>
    </v-app-bar>
    <RatingComponent ref="ratingComponent" @on-close="processLogout()" />
</template>
