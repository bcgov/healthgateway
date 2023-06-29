<script setup lang="ts">
import { computed, nextTick, onMounted } from "vue";

import { useNavbarStore } from "@/stores/navbar";
import { useUserStore } from "@/stores/user";
import { useAuthStore } from "@/stores/auth";
import { useConfigStore } from "@/stores/config";
import { useRoute } from "vue-router";
import HgIconButtonComponent from "@/components/shared/HgIconButtonComponent.vue";

const route = useRoute();

const authStore = useAuthStore();
const configStore = useConfigStore();
const navbarStore = useNavbarStore();
const userStore = useUserStore();

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

const isDependentEnabled = computed(
    () => configStore.webConfig.featureToggleConfiguration.dependents.enabled
);

const isServicesEnabled = computed(
    () => configStore.webConfig.featureToggleConfiguration.services.enabled
);

onMounted(async () => {
    await nextTick();
});
</script>

<template>
    <div v-if="isSidebarAvailable" class="wrapper">
        <v-navigation-drawer
            :rail="!navbarStore.isSidebarOpen"
            color="primary"
            permanent
        >
            <v-list density="compact" nav>
                <v-list-item
                    title="Home"
                    to="/home"
                    data-testid="menu-btn-home-link"
                >
                    <template #prepend>
                        <div
                            class="nav-list-item-icon mr-8 d-flex justify-center"
                        >
                            <v-icon icon="fas fa-house" />
                        </div>
                    </template>
                </v-list-item>
                <v-list-item
                    title="Timeline"
                    to="/timeline"
                    data-testid="menu-btn-time-line-link"
                >
                    <template #prepend>
                        <div
                            class="nav-list-item-icon mr-8 d-flex justify-center"
                        >
                            <v-icon icon="fas fa-box-archive" />
                        </div>
                    </template>
                </v-list-item>
                <v-list-item
                    title="COVID-19"
                    to="/covid19"
                    data-testid="menu-btn-covid19-link"
                >
                    <template #prepend>
                        <div
                            class="nav-list-item-icon mr-8 d-flex justify-center"
                        >
                            <v-icon icon="fas fa-circle-check" />
                        </div>
                    </template>
                </v-list-item>
                <v-list-item
                    v-show="isDependentEnabled && userStore.userIsActive"
                    title="Dependents"
                    to="/dependents"
                    data-testid="menu-btn-dependents-link"
                >
                    <template #prepend>
                        <div
                            class="nav-list-item-icon mr-8 d-flex justify-center"
                        >
                            <v-icon icon="fas fa-user-group" />
                        </div>
                    </template>
                </v-list-item>
                <v-list-item
                    v-show="isServicesEnabled && userStore.userIsActive"
                    prepend-icon="fas fa-hand-holding-medical"
                    title="Services"
                    to="/services"
                    data-testid="menu-btn-dependents-link"
                >
                    <template #prepend>
                        <div
                            class="nav-list-item-icon mr-8 d-flex justify-center"
                        >
                            <v-icon icon="fas fa-hand-holding-medical" />
                        </div>
                    </template>
                </v-list-item>
            </v-list>
            <template #append>
                <HgIconButtonComponent
                    :icon="
                        navbarStore.isSidebarOpen
                            ? 'fas fa-angle-double-left'
                            : 'fas fa-angle-double-right'
                    "
                    @click="navbarStore.toggleSidebar"
                />
            </template>
        </v-navigation-drawer>
    </div>
</template>

<style scoped>
.nav-list-item-icon {
    width: 1.5em;
}
</style>
