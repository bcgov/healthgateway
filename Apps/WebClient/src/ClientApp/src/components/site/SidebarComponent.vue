<script setup lang="ts">
import { computed, ref, watch } from "vue";

import FeedbackComponent from "@/components/site/FeedbackComponent.vue";
import { useAuthStore } from "@/stores/auth";
import { useConfigStore } from "@/stores/config";
import { useLayoutStore } from "@/stores/layout";
import { useUserStore } from "@/stores/user";

const layoutStore = useLayoutStore();
const authStore = useAuthStore();
const configStore = useConfigStore();
const userStore = useUserStore();

const collapsedOnDesktop = ref(false);
const feedbackDialog = ref<InstanceType<typeof FeedbackComponent>>();

const isMobile = computed(() => layoutStore.isMobile);
const isSidebarOpen = computed(() => layoutStore.isSidebarOpen);
const isDependentEnabled = computed(
    () => configStore.webConfig.featureToggleConfiguration.dependents.enabled
);
const isServicesEnabled = computed(
    () => configStore.webConfig.featureToggleConfiguration.services.enabled
);
const isSidebarAvailable = computed(
    () =>
        !configStore.isOffline &&
        authStore.oidcIsAuthenticated &&
        userStore.isValidIdentityProvider &&
        userStore.userIsRegistered &&
        userStore.userIsActive &&
        !userStore.patientRetrievalFailed
);

const visibleOnMobile = computed({
    get() {
        return isSidebarOpen.value;
    },
    set(value: boolean) {
        if (isSidebarOpen.value !== value) {
            layoutStore.toggleSidebar();
        }
    },
});

function toggleOpenClose(state: boolean) {
    if (isMobile.value) {
        visibleOnMobile.value = state;
    } else {
        collapsedOnDesktop.value = !state;
    }
}

watch(isSidebarOpen, (value: boolean) => {
    toggleOpenClose(value);
});
</script>

<template>
    <v-navigation-drawer
        v-if="isSidebarAvailable"
        :model-value="isMobile ? visibleOnMobile : true"
        :permanent="!isMobile"
        :temporary="isMobile"
        :rail="isMobile ? false : collapsedOnDesktop"
        color="background"
        class="d-print-none"
        data-testid="sidenavbar"
        @click.stop="collapsedOnDesktop = false"
        @update:model-value="visibleOnMobile = false"
    >
        <v-list density="compact" nav>
            <v-list-item
                class="nav-hover"
                title="Home"
                to="/home"
                data-testid="menu-btn-home-link"
            >
                <template #prepend>
                    <div class="nav-list-item-icon mr-8 d-flex justify-center">
                        <v-icon icon="fas fa-house" />
                    </div>
                </template>
            </v-list-item>
            <v-list-item
                class="nav-hover"
                title="Timeline"
                to="/timeline"
                data-testid="menu-btn-timeline-link"
            >
                <template #prepend>
                    <div class="nav-list-item-icon mr-8 d-flex justify-center">
                        <v-icon icon="fas fa-box-archive" />
                    </div>
                </template>
            </v-list-item>
            <v-list-item
                v-show="isDependentEnabled && userStore.userIsActive"
                class="nav-hover"
                title="Dependents"
                to="/dependents"
                data-testid="menu-btn-dependents-link"
            >
                <template #prepend>
                    <div class="nav-list-item-icon mr-8 d-flex justify-center">
                        <v-icon icon="fas fa-user-group" />
                    </div>
                </template>
            </v-list-item>
            <v-list-item
                v-show="isServicesEnabled && userStore.userIsActive"
                class="nav-hover"
                title="Services"
                to="/services"
                data-testid="menu-btn-services-link"
            >
                <template #prepend>
                    <div class="nav-list-item-icon mr-8 d-flex justify-center">
                        <v-icon icon="fas fa-hand-holding-medical" />
                    </div>
                </template>
            </v-list-item>
            <v-list-item
                v-show="userStore.userIsActive"
                class="nav-hover"
                title="Export"
                to="/reports"
                data-testid="menu-btn-reports-link"
            >
                <template #prepend>
                    <div class="nav-list-item-icon mr-8 d-flex justify-center">
                        <v-icon icon="fas fa-cloud-arrow-down" />
                    </div>
                </template>
            </v-list-item>
        </v-list>
        <template #append>
            <v-list-item
                title="Feedback"
                data-testid="menu-btn-feedback-link"
                class="nav-feedback-button"
                @click.stop="feedbackDialog?.showDialog()"
            >
                <template #prepend>
                    <div class="nav-list-item-icon mr-8 d-flex justify-center">
                        <v-icon icon="comments" />
                    </div>
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
.nav-feedback-button {
    background-color: rgb(var(--v-theme-navHighlight)) !important;
    border-top: 1px solid rgb(var(--v-theme-borderDivider));
}

:deep(.nav-hover) {
    --v-hover-opacity: 0;
}

:deep(.nav-hover:not(.v-list-item--active):hover) {
    background-color: rgb(var(--v-theme-surfaceHover)) !important;
}
</style>
