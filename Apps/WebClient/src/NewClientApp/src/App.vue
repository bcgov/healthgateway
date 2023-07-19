<script setup lang="ts">
import { computed } from "vue";
import { useRoute } from "vue-router";

import ErrorCardComponent from "@/components/error/ErrorCardComponent.vue";
import CommunicationComponent from "@/components/site/CommunicationComponent.vue";
import FooterComponent from "@/components/site/FooterComponent.vue";
import HeaderComponent from "@/components/site/HeaderComponent.vue";
import IdleComponent from "@/components/site/IdleComponent.vue";
import NotificationCentreComponent from "@/components/site/NotificationCentreComponent.vue";
import ResourceCentreComponent from "@/components/site/ResourceCentreComponent.vue";
import SidebarComponent from "@/components/site/SidebarComponent.vue";
import { Path } from "@/constants/path";

const route = useRoute();

const hideErrorAlerts = computed(() =>
    currentPathMatches(Path.Root, Path.VaccineCard, Path.Queue, Path.Busy)
);
const isHeaderVisible = computed(
    () => !currentPathMatches(Path.LoginCallback, Path.VaccineCard)
);
const isCommunicationVisible = computed(
    () =>
        !currentPathMatches(Path.LoginCallback, Path.VaccineCard) &&
        !route.path.toLowerCase().startsWith(Path.PcrTest.toLowerCase())
);
const isResourceCentreAvailable = computed(() =>
    currentPathMatches(Path.Dependents, Path.Reports, Path.Timeline)
);

const isFooterVisible = computed(
    () =>
        !currentPathMatches(
            Path.LoginCallback,
            Path.Registration,
            Path.VaccineCard
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
        <HeaderComponent v-if="isHeaderVisible" />
        <SidebarComponent />
        <NotificationCentreComponent />
        <v-main>
            <CommunicationComponent v-if="isCommunicationVisible" />
            <v-container class="pt-6">
                <ErrorCardComponent v-if="!hideErrorAlerts" />
                <router-view />
            </v-container>
            <ResourceCentreComponent v-if="isResourceCentreAvailable" />
        </v-main>
        <IdleComponent />
        <FooterComponent v-if="isFooterVisible" :order="-1" />
    </v-app>
</template>
