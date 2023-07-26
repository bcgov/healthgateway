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
import { EventName, useEventStore } from "@/stores/event";
import { useWaitlistStore } from "@/stores/waitlist";

const route = useRoute();
const eventStore = useEventStore();
const waitlistStore = useWaitlistStore();

const hideErrorAlerts = computed(() =>
    currentPathMatches(Path.Root, Path.Queue, Path.Busy)
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

eventStore.subscribe(EventName.RegisterOnBeforeUnloadWaitlistListener, () => {
    window.addEventListener("beforeunload", waitlistStore.releaseTicket);
});
eventStore.subscribe(EventName.UnregisterOnBeforeUnloadWaitlistListener, () => {
    window.removeEventListener("beforeunload", waitlistStore.releaseTicket);
});
eventStore.emit(EventName.RegisterOnBeforeUnloadWaitlistListener);
</script>

<template>
    <v-app>
        <HeaderComponent v-if="isHeaderVisible" />
        <SidebarComponent />
        <NotificationCentreComponent />
        <v-main class="position-relative">
            <CommunicationComponent v-if="isCommunicationVisible" />
            <router-view v-if="currentPathMatches(Path.VaccineCard)" />
            <v-container v-else class="pt-6">
                <ErrorCardComponent v-if="!hideErrorAlerts" />
                <router-view />
            </v-container>
            <ResourceCentreComponent v-if="isResourceCentreAvailable" />
        </v-main>
        <IdleComponent />
        <FooterComponent v-if="isFooterVisible" :order="-1" />
    </v-app>
</template>
