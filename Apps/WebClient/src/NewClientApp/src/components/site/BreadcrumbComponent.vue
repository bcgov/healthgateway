<script setup lang="ts">
import { computed } from "vue";

import BreadcrumbItem from "@/models/breadcrumbItem";
import { useAuthStore } from "@/stores/auth";
import { useUserStore } from "@/stores/user";

interface Props {
    items?: BreadcrumbItem[];
}
const props = withDefaults(defineProps<Props>(), {
    items: () => [],
});

const homeBreadcrumbItem: BreadcrumbItem = {
    text: "Home",
    to: "/home",
    dataTestId: "breadcrumb-home",
};

const authStore = useAuthStore();
const userStore = useUserStore();

const isAuthenticated = computed(() => authStore.oidcIsAuthenticated);

const isValidIdentityProvider = computed(
    () => userStore.isValidIdentityProvider
);
const allBreadcrumbItems = computed(() => [homeBreadcrumbItem, ...props.items]);

const displayBreadcrumbs = computed(
    () => isAuthenticated.value && isValidIdentityProvider.value
);
</script>

<template>
    <v-breadcrumbs
        v-if="displayBreadcrumbs"
        data-testid="breadcrumbs"
        class="py-0 mb-4"
        aria-label="Breadcrumb Nav"
    >
        <template v-for="(item, index) in allBreadcrumbItems" :key="item.text">
            <v-breadcrumbs-item
                :to="item.to"
                :active="item.active"
                :data-testid="item.dataTestId"
                :disabled="item.active"
            >
                {{ item.text }}
            </v-breadcrumbs-item>
            <v-breadcrumbs-divider v-if="index < allBreadcrumbItems.length - 1"
                >/</v-breadcrumbs-divider
            >
        </template>
    </v-breadcrumbs>
</template>

<style scoped>
.v-breadcrumbs {
    padding: 0.75rem 0rem;
    margin-bottom: 0rem;
}

.v-breadcrumbs-item {
    padding: 0 0;
}
</style>
