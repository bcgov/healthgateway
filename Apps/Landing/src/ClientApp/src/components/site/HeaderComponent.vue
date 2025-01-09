<script setup lang="ts">
import { computed, nextTick, onUnmounted, ref, watch } from "vue";
import { useRoute } from "vue-router";

import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import { useConfigStore } from "@/stores/config";
import { useLayoutStore } from "@/stores/layout";

const headerScrollThreshold = 100;

const layoutStore = useLayoutStore();
const configStore = useConfigStore();

const route = useRoute();

const isScrollNearBottom = ref(false);
const isHeaderVisible = ref();

const isMobileWidth = computed(() => layoutStore.isMobile);
const isOffline = computed(() => configStore.isOffline);
const isHeaderShown = computed(
    () => layoutStore.isHeaderShown || isScrollNearBottom.value
);
const isLogInButtonShown = computed(() => !isOffline.value);

function testIfScrollIsNearBottom() {
    const scrollPosition = window.scrollY;
    const scrollHeight = document.body.scrollHeight;
    const clientHeight = document.documentElement.clientHeight;
    isScrollNearBottom.value =
        scrollPosition + clientHeight >= scrollHeight - headerScrollThreshold;
}

function setHeaderState(isOpen: boolean): void {
    layoutStore.setHeaderState(isOpen);
}

watch(isMobileWidth, (value) => {
    if (!value) {
        setHeaderState(false);
    }
});

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
        class="d-print-none px-3 px-sm-5 align-center"
        color="white"
        :scroll-threshold="headerScrollThreshold"
        height="82"
    >
        <router-link to="/">
            <v-img
                alt="Go to Health Gateway home page"
                src="@/assets/images/gov/hg-logo.png"
                width="173"
                height="54"
            />
        </router-link>
        <v-spacer />
        <HgButtonComponent
            v-if="isLogInButtonShown"
            variant="secondary"
            inverse
            class="mr-0"
            prepend-icon="fas fa-sign-in-alt"
            data-testid="loginBtn"
            to="/login"
            text="Log In"
        />
    </v-app-bar>
</template>

<style lang="scss">
.v-toolbar__content {
    max-width: var(--site-max-width);
}
</style>
