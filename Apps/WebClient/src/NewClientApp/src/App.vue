<script setup lang="ts">
import HeaderComponent from "@/components/navmenu/HeaderComponent.vue";
import SidebarComponent from "@/components/navmenu/SidebarComponent.vue";
import { useRoute } from "vue-router";
import { computed, nextTick, onBeforeUnmount, onMounted, ref } from "vue";
import { useAppStore } from "@/stores/app";
import ScreenWidth from "@/constants/screenWidth";

const loginCallbackPath = "/logincallback";
const vaccineCardPath = "/vaccinecard";

const route = useRoute();
const appStore = useAppStore();

const initialized = ref(false);
const windowWidth = ref(0);

function currentPathMatches(...paths: string[]): boolean {
    const currentPath = route.path.toLowerCase();
    return paths.some((path) => path === currentPath);
}

const isHeaderVisible = computed(
    () =>
        appStore.appError === undefined &&
        !currentPathMatches(loginCallbackPath, vaccineCardPath)
);

const isMobile = computed<boolean>(() => appStore.isMobile);

function initializeResizeListener(): void {
    window.addEventListener("resize", onResize);
    onResize();
}

function onResize(): void {
    windowWidth.value = window.innerWidth;

    if (windowWidth.value < ScreenWidth.Mobile) {
        if (!isMobile.value) {
            setIsMobile(true);
        }
    } else {
        if (isMobile.value) {
            setIsMobile(false);
        }
    }
}

function setIsMobile(isMobile: boolean): void {
    appStore.setIsMobile(isMobile);
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
    <v-app>
        <HeaderComponent v-if="isHeaderVisible" class="d-print-none" />
        <SidebarComponent />
        <v-main>
            <router-view />
        </v-main>
    </v-app>
</template>
