<script setup lang="ts">
import HeaderComponent from "@/components/navmenu/HeaderComponent.vue";
import { useRoute } from "vue-router";
import { computed } from "vue";
import { useAppStore } from "@/stores/app";

const loginCallbackPath = "/logincallback";
const vaccineCardPath = "/vaccinecard";

const route = useRoute();
const appStore = useAppStore();

function currentPathMatches(...paths: string[]): boolean {
    const currentPath = route.path.toLowerCase();
    return paths.some((path) => path === currentPath);
}

const isHeaderVisible = computed(
    () =>
        appStore.appError === undefined &&
        !currentPathMatches(loginCallbackPath, vaccineCardPath)
);
</script>

<template>
    <v-app>
        <HeaderComponent v-if="isHeaderVisible" />
        <v-main>
            <router-view />
        </v-main>
    </v-app>
</template>
