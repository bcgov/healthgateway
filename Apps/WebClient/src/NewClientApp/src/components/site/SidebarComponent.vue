<script setup lang="ts">
import { computed, ref } from "vue";
import { useRoute } from "vue-router";

import HgIconButtonComponent from "@/components/common/HgIconButtonComponent.vue";
import FeedbackComponent from "@/components/site/FeedbackComponent.vue";
import { useAppStore } from "@/stores/app";
import { useAuthStore } from "@/stores/auth";
import { useConfigStore } from "@/stores/config";
import { useNavbarStore } from "@/stores/navbar";
import { useUserStore } from "@/stores/user";

const route = useRoute();

const appStore = useAppStore();
const authStore = useAuthStore();
const configStore = useConfigStore();
const navbarStore = useNavbarStore();
const userStore = useUserStore();

const collapsedOnDesktop = ref(false);
const feedbackDialog = ref<InstanceType<typeof FeedbackComponent>>();

const isMobile = computed(() => appStore.isMobile);
const isSidebarOpen = computed(() => navbarStore.isSidebarOpen);
const isDependentEnabled = computed(
    () => configStore.webConfig.featureToggleConfiguration.dependents.enabled
);
const isServicesEnabled = computed(
    () => configStore.webConfig.featureToggleConfiguration.services.enabled
);

const isQueuePage = computed(
    () =>
        route.path.toLowerCase() === "/queue" ||
        route.path.toLowerCase() === "/busy"
);

const isSidebarAvailable = computed(
    () =>
        !configStore.isOffline &&
        authStore.oidcIsAuthenticated &&
        userStore.isValidIdentityProvider &&
        userStore.userIsRegistered &&
        userStore.userIsActive &&
        !userStore.patientRetrievalFailed &&
        !isQueuePage.value
);

const visibleOnMobile = computed({
    get() {
        return isSidebarOpen.value;
    },
    set(value: boolean) {
        if (isSidebarOpen.value !== value) {
            navbarStore.toggleSidebar();
        }
    },
});

function dismiss() {
    if (isMobile.value) {
        visibleOnMobile.value = false;
    } else {
        collapsedOnDesktop.value = true;
    }
}

function handleSendFeedbackClick() {
    feedbackDialog.value?.showDialog();
}
</script>

<template>
    <v-navigation-drawer
        v-if="isSidebarAvailable"
        :model-value="isMobile ? visibleOnMobile : true"
        :permanent="!isMobile"
        :temporary="isMobile"
        :rail="isMobile ? false : collapsedOnDesktop"
        color="primary"
        class="d-print-none"
        @click.stop="collapsedOnDesktop = false"
        @update:model-value="visibleOnMobile = false"
    >
        <v-list>
            <v-list-item :title="userStore.userName" nav>
                <template #prepend>
                    <v-avatar
                        data-testid="sidenavbar-profile-initials"
                        color="info"
                    >
                        {{ userStore.userInitials }}
                    </v-avatar>
                </template>
                <template #append>
                    <HgIconButtonComponent
                        icon="fas fa-chevron-left"
                        data-testid="sidenavbar-dismiss-btn"
                        @click.stop="dismiss"
                    />
                </template>
            </v-list-item>
        </v-list>
        <v-divider></v-divider>
        <v-list nav class="d-flex flex-column">
            <v-list-item
                title="Home"
                to="/home"
                data-testid="menu-btn-home-link"
                prepend-icon="house"
            />
            <v-list-item
                title="Timeline"
                to="/timeline"
                data-testid="menu-btn-time-line-link"
                prepend-icon="box-archive"
            />
            <v-list-item
                title="COVID-19"
                to="/covid19"
                data-testid="menu-btn-covid19-link"
                prepend-icon="circle-check"
            />
            <v-list-item
                v-show="isDependentEnabled && userStore.userIsActive"
                title="Dependents"
                to="/dependents"
                data-testid="menu-btn-dependents-link"
                prepend-icon="user-group"
            />
            <v-list-item
                v-show="isServicesEnabled && userStore.userIsActive"
                prepend-icon="fas fa-hand-holding-medical"
                title="Services"
                to="/services"
                data-testid="menu-btn-dependents-link"
            />
            <v-list-item
                v-show="userStore.userIsActive"
                prepend-icon="fas fa-cloud-arrow-down"
                title="Export Records"
                to="/reports"
                data-testid="menu-btn-reports-link"
            />
        </v-list>
        <template #append>
            <v-list-item
                title="Feedback"
                data-testid="menu-btn-feedback-link"
                class="bg-blue text-white"
                @click.prevent="handleSendFeedbackClick()"
            >
                <template #prepend>
                    <v-icon icon="comments" class="text-white opacity-100" />
                </template>
            </v-list-item>
        </template>
    </v-navigation-drawer>
    <FeedbackComponent ref="feedbackDialog" />
</template>

<style scoped>
.nav-list-item-icon {
    width: 1.5em;
}
</style>
