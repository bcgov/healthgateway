<script setup lang="ts">
import { computed } from "vue";
import { useRoute } from "vue-router";

import ErrorCardComponent from "@/components/error/ErrorCardComponent.vue";
import CommunicationComponent from "@/components/site/CommunicationComponent.vue";
import DevBannerComponent from "@/components/site/DevBannerComponent.vue";
import FooterComponent from "@/components/site/FooterComponent.vue";
import HeaderComponent from "@/components/site/HeaderComponent.vue";
import IdleComponent from "@/components/site/IdleComponent.vue";
import NotificationCentreComponent from "@/components/site/NotificationCentreComponent.vue";
import SidebarComponent from "@/components/site/SidebarComponent.vue";
import { Path } from "@/constants/path";
import { useAuthStore } from "@/stores/auth";

const route = useRoute();
const authStore = useAuthStore();

const hideErrorAlerts = computed(() => currentPathMatches(Path.Root));
const isHeaderVisible = computed(
    () =>
        !currentPathMatches(Path.LoginCallback, Path.VaccineCard, Path.VppLogin)
);
const isCommunicationVisible = computed(
    () =>
        !currentPathMatches(Path.LoginCallback, Path.VaccineCard) &&
        !currentPathMatches(Path.LoginCallback, Path.VppLogin) &&
        !route.path
            .toLowerCase()
            .startsWith(Path.PcrTestKitRegistration.toLowerCase())
);
const isFooterVisible = computed(
    () =>
        !currentPathMatches(
            Path.LoginCallback,
            Path.Registration,
            Path.VaccineCard,
            Path.VppLogin
        )
);

function currentPathMatches(...paths: string[]): boolean {
    return paths.some(
        (path) => route.path.toLowerCase() === path.toLowerCase()
    );
}
</script>

<template>
    <v-app>
        <DevBannerComponent />
        <HeaderComponent v-if="isHeaderVisible" />
        <SidebarComponent />
        <NotificationCentreComponent v-if="authStore.oidcIsAuthenticated" />
        <v-main class="position-relative">
            <CommunicationComponent v-if="isCommunicationVisible" />
            <router-view
                v-if="currentPathMatches(Path.VaccineCard, Path.VppLogin)"
            />
            <v-container v-else fluid class="pt-6">
                <ErrorCardComponent v-if="!hideErrorAlerts" />
                <router-view />
            </v-container>
        </v-main>
        <IdleComponent />
        <FooterComponent v-if="isFooterVisible" />
    </v-app>
</template>
