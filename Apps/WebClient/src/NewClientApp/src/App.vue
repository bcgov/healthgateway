<script setup lang="ts">
import { computed, nextTick, onBeforeUnmount, onMounted, ref } from "vue";
import { useRoute } from "vue-router";

import ErrorCardComponent from "@/components/error/ErrorCardComponent.vue";
import CommunicationComponent from "@/components/site/CommunicationComponent.vue";
import FooterComponent from "@/components/site/FooterComponent.vue";
import HeaderComponent from "@/components/site/HeaderComponent.vue";
import IdleComponent from "@/components/site/IdleComponent.vue";
import SidebarComponent from "@/components/site/SidebarComponent.vue";
import { Path } from "@/constants/path";
import ScreenWidth from "@/constants/screenWidth";
import { useAppStore } from "@/stores/app";

const route = useRoute();
const appStore = useAppStore();

const initialized = ref(false);
const windowWidth = ref(0);

const isMobile = computed<boolean>(() => appStore.isMobile);
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

function initializeResizeListener(): void {
    window.addEventListener("resize", onResize);
    onResize();
}

function onResize(): void {
    windowWidth.value = window.innerWidth;

    if (windowWidth.value < ScreenWidth.Mobile) {
        if (!isMobile.value) {
            appStore.setIsMobile(true);
        }
    } else {
        if (isMobile.value) {
            appStore.setIsMobile(false);
        }
    }
}

onBeforeUnmount(() => {
    window.removeEventListener("resize", onResize);
});

onMounted(async () => {
    windowWidth.value = window.innerWidth;

    await nextTick();
    initializeResizeListener();
    initialized.value = true;
});
</script>

<template>
    <v-app v-if="initialized">
        <HeaderComponent v-if="isHeaderVisible" />
        <SidebarComponent />
        <v-main>
            <CommunicationComponent v-if="isCommunicationVisible" />
            <v-container class="pt-6">
                <ErrorCardComponent v-if="!hideErrorAlerts" />
                <router-view />
            </v-container>
        </v-main>
        <IdleComponent />
        <FooterComponent v-if="isFooterVisible" :order="-1" />
    </v-app>
</template>
